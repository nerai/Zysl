using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Zysl.BinStores;

namespace Zysl.KVS
{
	public class KVStore<TKey, TValue> :
		IDisposable,
		IKVStore<TKey, TValue>
	{
		private static SHA512Managed _Sha = new SHA512Managed ();

		private readonly IBinStore _Backing;
		private readonly NetDataContractSerializer _Ser;

		public KVStore (string path) :
			this (new FileStore (path))
		{
		}

		public KVStore () :
			this (new FileStore ("./KVS~" + typeof (TKey).ToString () + "~" + typeof (TValue).ToString ()))
		{
		}

		public KVStore (IBinStore backing)
		{
			_Backing = backing;
			_Ser = new NetDataContractSerializer ();
		}

		public void RepairFileIds ()
		{
			foreach (var file in _Backing.ListKeys ()) {
				var item = ReadItem (file);
				try {
					var test = this[item.Key];
				}
				catch (Exception ex) {
					Console.WriteLine ("Fixing item " + item.Key + " in " + file);
					WriteItem (file, item.Key, item.Value);
					_Backing.Remove (file);
				}
			}
		}

		private string GetPath (TKey key)
		{
			var text = key.ToString ();
			var bytes = ASCIIEncoding.UTF8.GetBytes (text);
			var hash = _Sha.ComputeHash (bytes);
			return bytes.Length + "-" + HexStr (hash);
		}

		private static string HexStr (byte[] p)
		{
			char[] c = new char[p.Length * 2];
			byte b;
			for (int y = 0, x = 0; y < p.Length; ++y, ++x) {
				b = ((byte) (p[y] >> 4));
				c[x] = (char) (b > 9 ? b + 0x37 : b + 0x30);
				b = ((byte) (p[y] & 0xF));
				c[++x] = (char) (b > 9 ? b + 0x37 : b + 0x30);
			}
			return new string (c);
		}

		public TValue this[TKey key]
		{
			get
			{
				var p = GetPath (key);
				return ReadItem (p).Value;
			}
			set
			{
				var p = GetPath (key);
				WriteItem (p, key, value);
			}
		}

		private void WriteItem (string path, TKey key, TValue value)
		{
			var item = new KeyValuePair<TKey, TValue> (key, value);

			using (var buf = new MemoryStream ()) {
				_Ser.Serialize (buf, item);
				var p = GetPath (key);
				_Backing[p] = buf.ToArray ();
			}
		}

		private KeyValuePair<TKey, TValue> ReadItem (string path)
		{
			var bytes = _Backing[path];
			var stream = new MemoryStream (bytes);
			if (stream.Length == 0) {
				throw new Exception (); // ? todo
			}
			return (KeyValuePair<TKey, TValue>) _Ser.Deserialize (stream);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			foreach (var key in _Backing.ListKeys ()) {
				yield return ReadItem (key);
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public bool ContainsKey (TKey key)
		{
			var p = GetPath (key);
			return _Backing.ContainsKey (p);
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			if (ContainsKey (key)) {
				value = this[key];
				return true;
			}
			else {
				value = default (TValue);
				return false;
			}
		}

		public bool Remove (TKey key)
		{
			var p = GetPath (key);
			return _Backing.Remove (p);
		}

		public void Flush ()
		{
			_Backing.Flush ();
		}

		public long Count
		{
			get
			{
				return _Backing.Count;
			}
		}

		public void Dispose ()
		{
			_Backing.Dispose ();
		}

		public IEnumerable<TKey> Keys
		{
			get
			{
				return this.Select (x => x.Key);
			}
		}

		public IEnumerable<TValue> Values
		{
			get
			{
				return this.Select (x => x.Value);
			}
		}
	}
}
