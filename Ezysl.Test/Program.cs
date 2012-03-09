using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ezysl.BinStores;
using Ezysl.KVS;

namespace Ezysl.Test
{
	internal class Program
	{
		private static void Main (string[] args)
		{
			TestSimple ();
		}

		private static void TestSimple ()
		{
			// todo: in protobuf: empty lists are returned as null (known bug by design)

			var backing = new FileStore ("TestSimple");
			var store = new KVStore<string, TestItem> (backing);

			Action<TestItem> test = item => {
				store[item.Time.Ticks.ToString ()] = item;
				var read = store[item.Time.Ticks.ToString ()];
				Console.WriteLine (item);
				Console.WriteLine (read);
			};

			test (new TestItem (DateTime.Now, null));
			test (new TestItem (DateTime.Now, new string[0]));
			test (new TestItem (DateTime.Now, new string[] { "1", "2" }));
		}
	}
}
