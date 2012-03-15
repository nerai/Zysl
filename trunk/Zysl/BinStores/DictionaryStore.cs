using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zysl.BinStores
{
	public class DictionaryStore :
		Dictionary<string, byte[]>,
		IBinStore
	{
		public void Flush ()
		{
			// no need to do anything
		}

		public void Dispose ()
		{
			// no need to do anything
		}

		public bool TrySetValue (string key, byte[] value)
		{
			this[key] = value;
			return true;
		}

		public IEnumerable<string> ListKeys ()
		{
			return Keys;
		}

		public new long Count
		{
			get
			{
				return base.Count;
			}
		}

		public string Name
		{
			get { return ""; }
		}
	}
}
