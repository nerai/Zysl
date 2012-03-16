using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zysl.BinStores
{
	public class ConcurrentStore : IBinStore
	{
		private readonly IBinStore _Backing;
		private readonly BlockingSet<string> _Lock = new BlockingSet<string> ();

		public ConcurrentStore (IBinStore backing)
		{
			_Backing = backing;
		}

		public byte[] this[string key]
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

		public bool ContainsKey (string key)
		{
			using (_Lock.Block (key)) {
				return _Backing.ContainsKey (key);
			}
		}

		public bool TryGetValue (string key, out byte[] value)
		{
			using (_Lock.Block (key)) {
				return _Backing.TryGetValue (key, out value);
			}
		}

		public bool TrySetValue (string key, byte[] value)
		{
			using (_Lock.Block (key)) {
				return _Backing.TrySetValue (key, value);
			}
		}

		public bool Remove (string key)
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

		public IEnumerable<string> ListKeys ()
		{
			using (_Lock.Block ()) {
				return _Backing.ListKeys ();
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

		public string Name
		{
			get
			{
				return _Backing.Name;
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
