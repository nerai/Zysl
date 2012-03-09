using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Ezysl.BinStores;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Ezysl.KVS
{
	public class KVStore<TKey, TValue> :
		IDisposable,
		IKVStore<TKey, TValue>
	{
		[ThreadStatic]
		private static SHA512Managed _Sha = new SHA512Managed ();

		public enum SerializationMethod
		{
			Default = 0,
			NetDataContract = Default,
			Protobuf,
		}

		SerializationMethod _SerializationMethod;
		private readonly NetDataContractSerializer _Ser = null;

		private readonly IBinStore _Backing;
		private readonly string _Prefix;

		public KVStore (IBinStore backing)
			: this (backing, SerializationMethod.NetDataContract)
		{
		}

		public KVStore (IBinStore backing, SerializationMethod serializer)
		{
			_Backing = backing;
			_SerializationMethod = serializer;
			_Prefix = "KVStore " + _SerializationMethod.ToString () + " " + backing.Name + " " + typeof (TKey).ToString () + " " + typeof (TValue).ToString () + " ";

			switch (serializer) {
				case SerializationMethod.NetDataContract:
					_Ser = new NetDataContractSerializer ();
					break;
				default:
					throw new ArgumentException ();
			}
		}

		private string GetPath (TKey key)
		{
			var text = _Prefix + key.ToString ();
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
				switch (_SerializationMethod) {
					case SerializationMethod.NetDataContract:
						_Ser.Serialize (buf, item);
						break;
					case SerializationMethod.Protobuf:
						Serializer.Serialize<KeyValuePair<TKey, TValue>> (buf, item);
						break;
				}

				var p = GetPath (key);
				_Backing[p] = buf.ToArray ();
			}
		}

		private KeyValuePair<TKey, TValue> ReadItem (string path)
		{
			var bytes = _Backing[path];
			var stream = new MemoryStream (bytes);
			if (stream.Length == 0) {
				throw new Exception ();
			}

			switch (_SerializationMethod) {
				case SerializationMethod.NetDataContract:
					return (KeyValuePair<TKey, TValue>) _Ser.Deserialize (stream);
				case SerializationMethod.Protobuf:
					return Serializer.Deserialize<KeyValuePair<TKey, TValue>> (stream);
				default:
					throw new InvalidProgramException ();
			}
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
	}
}
