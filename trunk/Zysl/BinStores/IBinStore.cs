using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zysl.BinStores
{
	/// <summary>
	/// Public interface to mark a store for binary data accessed via a
	/// string key.
	///
	/// If the backing store fails to perform the requested action (e.g.
	/// due to a connection issue) an exception will be thrown (even in the
	/// Try-methods).
	/// </summary>
	public interface IBinStore : IDisposable
	{
		/// <summary>
		/// Similar to a dictionary, the indexed access directly gets
		/// or sets a value. Get throws if the key is not present.
		/// </summary>
		byte[] this[string key]
		{
			get;
			set;
		}

		/// <summary>
		/// Returns true if the specified key exists in the store.
		/// </summary>
		bool ContainsKey (string key);

		/// <summary>
		/// Attempts to retrieve a value. Returns false if the key is
		/// not present.
		/// </summary>
		bool TryGetValue (string key, out byte[] value);

		/// <summary>
		/// Removes a key. Returns true if the key was present.
		/// </summary>
		bool Remove (string key);

		/// <summary>
		/// Flushes caches if present, enforcing that all data are
		/// actually written to the backing store.
		/// </summary>
		void Flush ();

		/// <summary>
		/// Enumerates all keys.
		/// </summary>
		IEnumerable<string> ListKeys ();

		/// <summary>
		/// Returns the number of keys currently present.
		/// </summary>
		long Count { get; }

		/// <summary>
		/// Returns the name given to this store.
		/// </summary>
		string Name { get; }
	}
}
