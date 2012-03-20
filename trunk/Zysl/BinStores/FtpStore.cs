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
		private readonly string _Root;

		public FtpStore (string server, string user, string pass, bool passive, string root)
		{
			_Ftp = new FtpControl (server, user, pass, passive);
			_Root = root;
			_Ftp.CreateDirectory (_Root);

			/*
			 * Recovery process, see FileStore class
			 */
			foreach (var file in ListKeys ()) {
				// todo: getfilelist sollte bereits nach cache prefix filtern damit nicht so viel ankommt
				// todo: prüfen wie genau file path augebaut ist und ob er zu lang ist (enthält CWD?)

				if (!Path.GetFileName (file).StartsWith (CachePrefix)) {
					continue;
				}

				if (_Ftp.Exists (_Root + file)) {
					if (!_Ftp.Delete (_Root + CachePrefix + file)) {
						throw new Exception ("FTP recovery process failed: Unable to delete cache file " + file);
					}
				}
				else {
					if (!_Ftp.Rename (_Root + CachePrefix + file, _Root + file)) {
						throw new Exception ("FTP recovery process failed: Unable to move cache file " + file);
					}
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
			return _Ftp.Exists (path);
		}

		public bool TryGetValue (string key, out byte[] value)
		{
			if (!ContainsKey (key)) {
				value = null;
				return false;
			}

			var path = _Root + "/" + key;
			value = _Ftp.Downloadaw (path);
			return true;
		}

		private void SetValue (string key, byte[] value)
		{
			// todo: use subfolder instead (in filestore too)
			if (Path.GetFileName (key).StartsWith (CachePrefix)) {
				throw new ArgumentException ("Key must not start with cache prefix <" + CachePrefix + ">", "key");
			}

			var path = _Root + "/" + key;
			var tmp = path.Insert (path.Length - Path.GetFileName (path).Length, CachePrefix);

			_Ftp.Upload (tmp, value);

			_Ftp.Delete (path);
			_Ftp.Rename (tmp, path);
		}

		public bool Remove (string key)
		{
			var path = _Root + "/" + key;
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
			foreach (var file in _Ftp.GetFileList (_Root)) {
				// todo: liefert das den namen, pfad oder pfad inkl root?? entsprechend abschneiden...
				yield return file;
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
			get { return _Root; }
		}
	}
}
