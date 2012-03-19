using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Zysl.Utils;

namespace Zysl.BinStores
{
	public class FileStore : IBinStore
	{
		private const string CachePrefix = "._FS_cache_";

		private readonly PathSelector _Pathes;

		public FileStore (string path)

			: this (new PathSelector (path))
		{
		}

		public FileStore (PathSelector pathes)
		{
			_Pathes = pathes;

			foreach (var dir in _Pathes.PossibleDirectories ()) {
				Directory.CreateDirectory (dir);
			}

			/*
			 * Recovery process, as per http://stackoverflow.com/questions/9096380/implementing-atomic-file-writes-in-a-nontransactional-filesystem
			 * This is required for acting as if NTFS supported atomic write natively.
			 */
			foreach (var path in Directory.GetFiles (_Pathes.Root, CachePrefix + "*", SearchOption.AllDirectories)) {
				var file = path.Substring (_Pathes.Root.Length + CachePrefix.Length);

				if (File.Exists (_Pathes.Root + file)) {
					File.Delete (path);
				}
				else {
					File.Move (path, _Pathes.Root + file);
				}
			}
		}

		public byte[] this[string key]
		{
			get
			{
				byte[] value;
				if (!TryGetValue (key, out value)) {
					throw new Exception ("Failed to read value of " + key + " (path: " + _Pathes.GetPath (key) + ")");
				}
				return value;
			}
			set
			{
				if (!TrySetValue (key, value)) {
					throw new Exception ("Failed to write value of " + key + " (path: " + _Pathes.GetPath (key) + ")");
				}
			}
		}

		public bool ContainsKey (string key)
		{
			var path = _Pathes.GetPath (key);
			return File.Exists (path);
		}

		public bool TryGetValue (string key, out byte[] value)
		{
			if (!ContainsKey (key)) {
				value = null;
				return false;
			}

			var path = _Pathes.GetPath (key);
			using (var file = new FileStream (path, FileMode.Open, FileAccess.Read)) {
				value = file.ReadAll ();
			}
			return true;
		}

		public bool TrySetValue (string key, byte[] value)
		{
			// todo: dont throw. use a subfolder instead (?)
			if (Path.GetFileName (key).StartsWith (CachePrefix)) {
				throw new ArgumentException ("Key must not start with cache prefix <" + CachePrefix + ">", "key");
			}

			/*
			 * Note that this acts atomic as long as FileMove is atomic (the case on
			 * NTFS). If the operation is not completed due to power outage etc a
			 * consistent state will be restored upon restarting this class.
			 */
			var path = _Pathes.GetPath (key);
			var tmp = path.Insert (path.Length - Path.GetFileName (path).Length, CachePrefix);

			using (var file = new FileStream (tmp, FileMode.Create, FileAccess.Write)) {
				file.Write (value, 0, value.Length);
			}

			File.Delete (path);
			File.Move (tmp, path);

			return true;
		}

		public bool Remove (string key)
		{
			var path = _Pathes.GetPath (key);

			if (File.Exists (path)) {
				File.Delete (path);
				return true;
			}
			else {
				return false;
			}
		}

		public void Flush ()
		{
			// no need to do anything here
		}

		public void Dispose ()
		{
			// no need to do anything here
		}

		public IEnumerable<string> ListKeys ()
		{
			var files = Directory.GetFiles (_Pathes.Root, "*", SearchOption.AllDirectories);
			return files.Select (x => x.Substring (_Pathes.Root.Length));
		}

		public long Count
		{
			get
			{
				return Directory.GetFiles (_Pathes.Root, "*", SearchOption.AllDirectories).Count ();
			}
		}

		public string Name
		{
			get { return _Pathes.Root; }
		}
	}
}
