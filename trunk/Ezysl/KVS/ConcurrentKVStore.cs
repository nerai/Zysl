using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Zysl.KVS
{
	public class ConcurrentKVStore<TKey, TValue> :
		IDisposable,
		IKVStore<TKey, TValue>
	{
		private readonly object _Lock = new object ();
		private readonly IKVStore<TKey, TValue> _Backing;

		public ConcurrentKVStore (IKVStore<TKey, TValue> backing)
		{
			_Backing = backing;
		}

		public TValue this[TKey key]
		{
			get
			{
				lock (_Lock) {
					return _Backing[key];
				}
			}
			set
			{
				lock (_Lock) {
					_Backing[key] = value;
				}
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			lock (_Lock) {
				return _Backing.GetEnumerator ();
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public bool ContainsKey (TKey key)
		{
			lock (_Lock) {
				return _Backing.ContainsKey (key);
			}
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			lock (_Lock) {
				return _Backing.TryGetValue (key, out value);
			}
		}

		public bool Remove (TKey key)
		{
			lock (_Lock) {
				return _Backing.Remove (key);
			}
		}

		public void Flush ()
		{
			lock (_Lock) {
				_Backing.Flush ();
			}
		}

		public long Count
		{
			get
			{
				lock (_Lock) {
					return _Backing.Count;
				}
			}
		}

		public void Dispose ()
		{
			lock (_Lock) {
				_Backing.Dispose ();
			}
		}
	}
}
