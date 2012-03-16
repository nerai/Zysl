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
		private readonly IKVStore<TKey, TValue> _Backing;
		private readonly BlockingSet<TKey> _LockXXX = new BlockingSet<TKey> ();

		public ConcurrentKVStore (IKVStore<TKey, TValue> backing)
		{
			_Backing = backing;
		}

		public TValue this[TKey key]
		{
			get
			{
				using (_LockXXX.Block (key)) {
					return _Backing[key];
				}
			}
			set
			{
				using (_LockXXX.Block (key)) {
					_Backing[key] = value;
				}
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			using (_LockXXX.Block ()) {
				return _Backing.GetEnumerator ();
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public bool ContainsKey (TKey key)
		{
			using (_LockXXX.Block (key)) {
				return _Backing.ContainsKey (key);
			}
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			using (_LockXXX.Block (key)) {
				return _Backing.TryGetValue (key, out value);
			}
		}

		public bool Remove (TKey key)
		{
			using (_LockXXX.Block (key)) {
				return _Backing.Remove (key);
			}
		}

		public void Flush ()
		{
			using (_LockXXX.Block ()) {
				_Backing.Flush ();
			}
		}

		public long Count
		{
			get
			{
				using (_LockXXX.Block ()) {
					return _Backing.Count;
				}
			}
		}

		public void Dispose ()
		{
			using (_LockXXX.Block ()) {
				_Backing.Dispose ();
			}
		}
	}
}
