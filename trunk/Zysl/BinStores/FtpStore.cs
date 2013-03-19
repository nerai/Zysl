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
		private readonly FtpControl _Ftp;
		private readonly string _Root;
		private readonly string _Tmp;

		public FtpStore (string server, string user, string pass, bool passive, string root)
		{
			_Ftp = new FtpControl (server, user, pass, passive);
			_Root = root;
			_Tmp = _Root + "/tmp";
			_Ftp.CreateDirectory (_Root);
			_Ftp.CreateDirectory (_Tmp);

			/*
			 * Recovery process, see FileStore class
			 */
			foreach (var file in ListKeys (_Tmp)) {
				// todo: getfilelist sollte bereits nach cache prefix filtern damit nicht so viel ankommt
				// todo: prüfen wie genau file path augebaut ist und ob er zu lang ist (enthält komplettes CWD?)

				if (_Ftp.Exists (_Root + file)) {
					if (!_Ftp.Delete (_Tmp + file)) {
						throw new IOException ("FTP recovery process failed: Unable to delete temp file " + file);
					}
				}
				else {
					if (!_Ftp.Rename (_Tmp + file, _Root + file)) {
						throw new IOException ("FTP recovery process failed: Unable to move temp file " + file);
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
					throw new KeyNotFoundException ("Failed to read value of " + key + " (path: " + _Root + "/" + key + ")");
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
			var path = _Root + "/" + key;
			var tmp = _Tmp + "/" + key;

			_Ftp.Upload (tmp, value);

			if (!_Ftp.Delete (path)) {
				throw new IOException ("Failed to delete temporary file on server.");
			}
			if (!_Ftp.Rename (tmp, path)) {
				throw new IOException ("Failed to rename temporary file on server.");
			}
		}

		public bool AttemptSetValue (string key, byte[] value)
		{
			try {
				SetValue (key, value);
				return true;
			}
			catch (ArgumentException ex) {
				return false;
			}
			catch (IOException ex) {
				return false;
			}
		}

		public bool AttemptGetValue (string key, out byte[] value)
		{
			try {
				return TryGetValue (key, out value);
			}
			catch (ArgumentException ex) {
				value = null;
				return false;
			}
			catch (IOException ex) {
				value = null;
				return false;
			}
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

		private IEnumerable<string> ListKeys (string dir)
		{
			foreach (var file in _Ftp.GetFileList (dir)) {
				// todo: liefert das den namen, pfad oder pfad inkl root?? entsprechend abschneiden...
				yield return file;
			}
		}

		public IEnumerable<string> ListKeys ()
		{
			return ListKeys (_Root);
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
