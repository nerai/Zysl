using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Zysl.KVS
{
	public class ConcurrentKVStore<TKey, TValue> :
		IDisposable,
		IKVStore<TKey, TValue>
	{
		private readonly IKVStore<TKey, TValue> _Backing;
		private readonly BlockingSet<TKey> _Lock = new BlockingSet<TKey> ();

		public ConcurrentKVStore (IKVStore<TKey, TValue> backing)
		{
			_Backing = backing;
		}

		public TValue this[TKey key]
		{
			get
			{
				using (_Lock.Block (key)) {
					return _Backing[key];
				}
			}
			set
			{
				using (_Lock.Block (key)) {
					_Backing[key] = value;
				}
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			using (_Lock.Block ()) {
				return _Backing.GetEnumerator ();
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public bool ContainsKey (TKey key)
		{
			using (_Lock.Block (key)) {
				return _Backing.ContainsKey (key);
			}
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			using (_Lock.Block (key)) {
				return _Backing.TryGetValue (key, out value);
			}
		}

		public bool Remove (TKey key)
		{
			using (_Lock.Block (key)) {
				return _Backing.Remove (key);
			}
		}

		public void Flush ()
		{
			using (_Lock.Block ()) {
				_Backing.Flush ();
			}
		}

		public long Count
		{
			get
			{
				using (_Lock.Block ()) {
					return _Backing.Count;
				}
			}
		}

		public void Dispose ()
		{
			using (_Lock.Block ()) {
				_Backing.Dispose ();
			}
		}
	}
}
