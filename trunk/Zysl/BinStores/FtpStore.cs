using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Zysl.BinStores
{
	public class FtpStore : IBinStore
	{
		private const string CachePrefix = "FS_cache_";

		private readonly FtpControl _Ftp;
		private readonly PathSelector _Pathes;

		public FtpStore (string server, string user, string pass, bool passive, PathSelector pathes)
		{
			_Ftp = new FtpControl (server, user, pass, passive);
			_Pathes = pathes;

			foreach (var dir in _Pathes.PossibleDirectories ()) {
				_Ftp.CreateDirectory (dir);
			}

			/*
			 * Recovery process, see FileStore class
			 */
			foreach (var file in ListKeys ()) {
				// todo: getfilelist sollte bereits nach cache prefix filtern damit nicht so viel ankommt
				// todo: prüfen wie genau file path augebaut ist und ob er zu lang ist (enthält CWD?)

				if (!Path.GetFileName (file).StartsWith (CachePrefix)) {
					continue;
				}

				if (_Ftp.Exists (_Pathes.Root + file)) {
					if (!_Ftp.Delete (_Pathes.Root + CachePrefix + file)) {
						throw new Exception ("FTP recovery process failed: Unable to delete cache file " + file);
					}
				}
				else {
					if (!_Ftp.Rename (_Pathes.Root + CachePrefix + file, _Pathes.Root + file)) {
						throw new Exception ("FTP recovery process failed: Unable to move cache file " + file);
					}
				}
			}
		}

		public byte[] this[string key]
		{
			get
			{
				var path = _Pathes.GetPath (key);
				return _Ftp.Download (path);
			}
			set
			{
				if (!TrySetValue (key, value)) {
					throw new Exception ("Failed to write value to " + _Pathes.GetPath (key));
				}
			}
		}

		public bool ContainsKey (string key)
		{
			var path = _Pathes.GetPath (key);
			return _Ftp.Exists (path);
		}

		public bool TryGetValue (string key, out byte[] value)
		{
			if (ContainsKey (key)) {
				value = this[key];
				return true;
			}
			else {
				value = null;
				return false;
			}
		}

		public bool TrySetValue (string key, byte[] value)
		{
			if (Path.GetFileName (key).StartsWith (CachePrefix)) {
				throw new ArgumentException ("Key must not start with cache prefix <" + CachePrefix + ">", "key");
			}

			var path = _Pathes.GetPath (key);
			var tmp = path.Insert (path.Length - Path.GetFileName (path).Length, CachePrefix);

			if (!_Ftp.Upload (tmp, value)) {
				return false;
			}
			if (!_Ftp.Delete (path)) {
				return false;
			}
			return _Ftp.Rename (tmp, path);
		}

		public bool Remove (string key)
		{
			var path = _Pathes.GetPath (key);
			return _Ftp.Delete (path);
		}

		public void Flush ()
		{
			// no need to do anything here
		}

		public void Dispose ()
		{
			// no need to do anything here
		}

		public DateTime Ping ()
		{
			return _Ftp.Ping ();
		}

		public IEnumerable<string> ListKeys ()
		{
			foreach (var dir in _Pathes.PossibleDirectories ()) {
				foreach (var file in _Ftp.GetFileList (dir)) {
					// todo: liefert das den namen, pfad oder pfad inkl root?? entsprechend abschneiden...
					yield return file;
				}
			}
		}

		public long Count
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public string Name
		{
			get { return _Pathes.Root; }
		}
	}
}
