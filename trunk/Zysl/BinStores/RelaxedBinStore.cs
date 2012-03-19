using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zysl.BinStores
{
	public class RelaxedBinStore
	{
		private readonly IBinStore _Backing;

		public RelaxedBinStore (IBinStore backing)
		{
			_Backing = backing;
		}

		// todo: handle errors gracefully, always returning an indicator if something went right or wrong
	}
}
