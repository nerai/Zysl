using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zysl.BinStores
{
	public interface IBinStore : IDisposable
	{
		byte[] this[string key]
		{
			get;
			set;
		}

		bool ContainsKey (string key);

		bool TryGetValue (string key, out byte[] value);

		bool TrySetValue (string key, byte[] value);

		// false genau dann wenn verbindung fehlgeschlagen
		bool Remove (string key);

		void Flush ();

		IEnumerable<string> ListKeys ();

		long Count { get; }

		string Name { get; }
	}
}

// todo: spezifizieren was bei fehlern passiert (key not found etc)
