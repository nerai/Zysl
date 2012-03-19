using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zysl.BinStores
{
	/// <summary>
	/// Public interface to mark a store for binary data accessed via a string key.
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

		// todo connection failed error??
		/// <summary>
		/// Attempts to retrieve a value. Returns false if the key is
		/// not present.
		/// </summary>
		bool TryGetValue (string key, out byte[] value);

		/// <summary>
		/// Attempts to set a value. Returns false if something went
		/// wrong.
		/// </summary>
		bool TrySetValue (string key, byte[] value);

		// false genau dann wenn verbindung fehlgeschlagen
		bool Remove (string key);

		/// <summary>
		/// Flushes caches if present, enforcing that all data are
		/// actually written to the backing store.
		/// </summary>
		void Flush ();

		IEnumerable<string> ListKeys ();

		long Count { get; }

		string Name { get; }
	}
}

// todo: spezifizieren was bei fehlern passiert (key not found etc), connection issue, ...
