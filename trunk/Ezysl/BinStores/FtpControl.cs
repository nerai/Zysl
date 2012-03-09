using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Ezysl.Utils;
using NLog;

namespace Ezysl.BinStores
{
	internal class FtpControl
	{
		private static readonly Logger _L = LogManager.GetCurrentClassLogger ();

		private readonly string _Server;
		private readonly string _User;
		private readonly string _Pass;
		private readonly bool _Passive;

		public FtpControl (string server, string user, string pass, bool passive)
		{
			if (string.IsNullOrEmpty (server)) {
				throw new ArgumentException ("Server address was not specified.", "server");
			}
			else {
				_Server = server;
			}

			_User = user;
			_Pass = pass;
			_Passive = passive;

			Ping ();
		}

		private FtpWebRequest CreateRequest (string method, string file)
		{
			var path = "ftp://" + _Server + file;
			var cred = new NetworkCredential (_User, _Pass);

			var request = (FtpWebRequest) WebRequest.Create (path);
			request.Method = method;
			request.Credentials = cred;
			request.UseBinary = true;
			request.UsePassive = _Passive;

			return request;
		}

		public DateTime Ping ()
		{
			_L.Trace ("Ping");

			try {
				var request = CreateRequest (WebRequestMethods.Ftp.PrintWorkingDirectory, "");

				var now = DateTime.UtcNow;
				using (var response = (FtpWebResponse) request.GetResponse ()) {
					_L.Info (response.StatusDescription);
				}
				return now;
			}
			catch (WebException ex) {
				_L.Warn (ex);
			}

			return new DateTime (0);
		}

		public bool Upload (string filename, byte[] content)
		{
			_L.Trace ("Upload {0}b: {1}", content.Length, filename);

			try {
				var request = CreateRequest (WebRequestMethods.Ftp.UploadFile, "/" + filename);

				request.ContentLength = content.Length;
				var stream = request.GetRequestStream ();
				stream.Write (content, 0, content.Length);
				stream.Close ();

				using (var response = (FtpWebResponse) request.GetResponse ()) {
					_L.Info (response.StatusDescription);
				}
				return true;
			}
			catch (WebException ex) {
				_L.Warn (ex);
			}

			return false;
		}

		public byte[] Download (string filename)
		{
			_L.Trace ("Download {0}", filename);

			try {
				var request = CreateRequest (WebRequestMethods.Ftp.DownloadFile, "/" + filename);

				using (var response = (FtpWebResponse) request.GetResponse ()) {
					_L.Info (response.StatusDescription);

					var stream = response.GetResponseStream ();
					var bytes = stream.ReadAll ();

					_L.Info (response.StatusDescription);
					stream.Close ();

					return bytes;
				}
			}
			catch (WebException ex) {
				_L.Warn (ex);
			}

			return null;
		}

		public bool Exists (string filename)
		{
			_L.Trace ("Exists {0}", filename);

			try {
				var request = CreateRequest (WebRequestMethods.Ftp.GetFileSize, "/" + filename);

				try {
					using (var response = (FtpWebResponse) request.GetResponse ()) {
						_L.Info (response.StatusDescription);
					}
					return true;
				}
				catch (WebException) {
					return false;
				}
			}
			catch (WebException ex) {
				_L.Warn (ex);
			}

			return false;
		}

		public bool Delete (string filename)
		{
			_L.Trace ("Delete {0}", filename);

			try {
				var request = CreateRequest (WebRequestMethods.Ftp.DeleteFile, "/" + filename);

				try {
					using (var response = (FtpWebResponse) request.GetResponse ()) {
						_L.Info (response.StatusDescription);
					}
					return true;
				}
				catch (WebException) {
					return false;
				}
			}
			catch (WebException ex) {
				_L.Warn (ex);
			}

			return false;
		}

		public List<string> GetFileList (string path)
		{
			_L.Trace ("GetFileList");

			try {
				var request = CreateRequest (WebRequestMethods.Ftp.ListDirectory, "/" + path);

				using (var response = (FtpWebResponse) request.GetResponse ()) {
					_L.Info (response.StatusDescription);

					var stream = response.GetResponseStream ();
					var reader = new System.IO.StreamReader (stream);
					var result = new List<string> ();

					var line = reader.ReadLine ();
					while (line != null) {
						result.Add (line);
						line = reader.ReadLine ();
					}

					reader.Close ();

					return result;
				}
			}
			catch (WebException ex) {
				_L.Warn (ex);
			}

			return null;
		}

		public bool CreateDirectory (string path)
		{
			_L.Trace ("CreateDirectory");

			try {
				var request = CreateRequest (WebRequestMethods.Ftp.MakeDirectory, "/" + path);

				using (var response = (FtpWebResponse) request.GetResponse ()) {
					_L.Info (response.StatusDescription);
				}
				return true;
			}
			catch (WebException ex) {
				if (false) {
					// todo: wenn "550 directory already exists" dann alles ok
				}
				else {
					_L.Warn (ex);
				}
			}

			return false;
		}

		public bool Rename (string src, string dst)
		{
			_L.Trace ("Rename");

			try {
				var request = CreateRequest (WebRequestMethods.Ftp.Rename, "/" + src);
				request.RenameTo = dst;

				using (var response = (FtpWebResponse) request.GetResponse ()) {
					_L.Info (response.StatusDescription);
				}
				return true;
			}
			catch (WebException ex) {
				_L.Warn (ex);
			}

			return false;
		}
	}
}
