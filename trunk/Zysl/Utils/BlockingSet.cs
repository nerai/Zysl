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
	///
	/// Global locks are supported as well.
	/// </summary>
	public class BlockingSet<T>
	{
		private readonly Dictionary<T, ReaderWriterLockSlim> _Locks = new Dictionary<T, ReaderWriterLockSlim> ();

		private readonly ReaderWriterLockSlim _Global = new ReaderWriterLockSlim ();

		/// <summary>
		/// Adds an item. If the item is already known, this method
		/// blocks until the item got removed by a different thread.
		/// </summary>
		public void Enter (T id)
		{
			_Global.EnterReadLock ();

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
						break;
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
		public void Exit (T id)
		{
			/*
			 * release and remove lock
			 */
			lock (_Locks) {
				var slim = _Locks[id];
				_Locks.Remove (id);
				slim.ExitWriteLock ();
			}

			_Global.ExitReadLock ();
		}

		/// <summary>
		/// Enters a global lock, blocking until all individual items
		/// have been released. This lock takes precedence over
		/// subsequent finer grained locks and is thus very invasive.
		/// </summary>
		public void EnterGlobal ()
		{
			_Global.EnterWriteLock ();
		}

		/// <summary>
		/// Releases a global lock, allowing any other locks to
		/// continue.
		/// </summary>
		public void ExitGlobal ()
		{
			_Global.ExitWriteLock ();
		}

		private class BlockingSetLocalLock : IDisposable
		{
			private readonly BlockingSet<T> _Parent;
			private readonly T _Id;

			public BlockingSetLocalLock (BlockingSet<T> parent, T id)
			{
				_Parent = parent;
				_Id = id;
				_Parent.Enter (_Id);
			}

			public void Dispose ()
			{
				_Parent.Exit (_Id);
			}
		}

		/// <summary>
		/// Wrapper of local Enter-Exit for 'using' statements.
		/// </summary>
		public IDisposable Block (T id)
		{
			return new BlockingSetLocalLock (this, id);
		}

		private class BlockingSetGlobalLock : IDisposable
		{
			private readonly BlockingSet<T> _Parent;

			public BlockingSetGlobalLock (BlockingSet<T> parent)
			{
				_Parent = parent;
				_Parent.EnterGlobal ();
			}

			public void Dispose ()
			{
				_Parent.ExitGlobal ();
			}
		}

		/// <summary>
		/// Wrapper of global Enter-Exit for 'using' statements.
		/// </summary>
		public IDisposable Block ()
		{
			return new BlockingSetGlobalLock (this);
		}
	}
}
