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
		private readonly string _Root;
		private readonly string _Tmp;

		public FileStore (string root)
		{
			_Root = root;
			_Tmp = _Root + "/tmp";
			Directory.CreateDirectory (_Root);
			Directory.CreateDirectory (_Tmp);

			/*
			 * Recovery process, as per http://stackoverflow.com/questions/9096380/implementing-atomic-file-writes-in-a-nontransactional-filesystem
			 * This is required for acting as if NTFS supported atomic write natively.
			 */
			foreach (var path in Directory.GetFiles (_Tmp, "*", SearchOption.TopDirectoryOnly)) {
				var file = path.Substring (_Tmp.Length);

				if (File.Exists (_Root + file)) {
					File.Delete (path);
				}
				else {
					File.Move (path, _Root + file);
				}
			}
		}

		public byte[] this[string key]
		{
			get
			{
				byte[] value;
				if (!TryGetValue (key, out value)) {
					throw new Exception ("Failed to read value of " + key + " (path: " + _Root + "/" + key + ")");
				}
				return value;
			}
			set
			{
				SetValue (key, value);
			}
		}

		public bool ContainsKey (string key)
		{
			var path = _Root + "/" + key;
			return File.Exists (path);
		}

		public bool TryGetValue (string key, out byte[] value)
		{
			if (!ContainsKey (key)) {
				value = null;
				return false;
			}

			var path = _Root + "/" + key;
			using (var file = new FileStream (path, FileMode.Open, FileAccess.Read)) {
				value = file.ReadAll ();
			}
			return true;
		}

		private void SetValue (string key, byte[] value)
		{
			/*
			 * Note that this acts atomic as long as FileMove is atomic (the case on
			 * NTFS). If the operation is not completed due to power outage etc a
			 * consistent state will be restored upon restarting this class.
			 */
			var path = _Root + "/" + key;
			var tmp = _Tmp + "/" + key;

			using (var file = new FileStream (tmp, FileMode.Create, FileAccess.Write)) {
				file.Write (value, 0, value.Length);
			}

			File.Delete (path);
			File.Move (tmp, path);
		}

		public bool Remove (string key)
		{
			var path = _Root + "/" + key;

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
			var files = Directory.GetFiles (_Root, "*", SearchOption.TopDirectoryOnly);
			return files.Select (x => x.Substring (_Root.Length));
		}

		public long Count
		{
			get
			{
				return Directory.GetFiles (_Root, "*", SearchOption.TopDirectoryOnly).Count ();
			}
		}

		public string Name
		{
			get { return _Root; }
		}
	}
}
