using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ezysl.BinStores
{
	public class ConcurrentStore : IBinStore
	{
		private readonly IBinStore _Backing;
		private readonly object _Lock = new object ();

		public ConcurrentStore (IBinStore backing)
		{
			_Backing = backing;
		}

		public byte[] this[string key]
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

		public bool ContainsKey (string key)
		{
			lock (_Lock) {
				return _Backing.ContainsKey (key);
			}
		}

		public bool TryGetValue (string key, out byte[] value)
		{
			lock (_Lock) {
				return _Backing.TryGetValue (key, out value);
			}
		}

		public bool TrySetValue (string key, byte[] value)
		{
			lock (_Lock) {
				return _Backing.TrySetValue (key, value);
			}
		}

		public bool Remove (string key)
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

		public IEnumerable<string> ListKeys ()
		{
			lock (_Lock) {
				return _Backing.ListKeys ();
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

		public string Name
		{
			get
			{
				return _Backing.Name;
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
