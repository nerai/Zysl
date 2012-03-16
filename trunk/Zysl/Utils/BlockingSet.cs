using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Zysl
{
	/// <summary>
	/// The blocking set is a thread safe class used for locking a large
	/// number of seperate values. If an element is already contained,
	/// BlockingSet blocks until it got removed by a different thread.
	///
	/// Internally, a hash map in combination with Monitors is used.
	/// </summary>
	public class BlockingSet<T>
	{
		private readonly Dictionary<T, ReaderWriterLockSlim> _Locks = new Dictionary<T, ReaderWriterLockSlim> ();

		/// <summary>
		/// Adds an item. If the item is already known, this method
		/// blocks until the item got removed by a different thread.
		/// </summary>
		public void Add (T id)
		{
			for (; ; ) {
				ReaderWriterLockSlim slim;

				lock (_Locks) {
					/*
					 * if row not in use, grab it
					 */
					if (!_Locks.TryGetValue (id, out slim)) {
						slim = new ReaderWriterLockSlim (); // todo: this can probably be replaced by a different, cheap signal class
						slim.EnterWriteLock ();
						_Locks.Add (id, slim);
						return;
					}
				}

				/*
				 * row is in use, wait until released, then try again
				 */
				slim.EnterWriteLock ();
			}
		}

		/// <summary>
		/// Removes an item that was previously added to the set,
		/// unblocking the next thread waiting to add this item.
		/// </summary>
		/// <param name="id"></param>
		public void Remove (T id)
		{
			/*
			 * release and remove lock
			 */
			lock (_Locks) {
				var slim = _Locks[id];
				_Locks.Remove (id);
				slim.ExitWriteLock ();
			}
		}
	}
}
