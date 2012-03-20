using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zysl.BinStores;
using Zysl.KVS;

namespace Zysl.Test
{
	internal class Program
	{
		private static void Main (string[] args)
		{
			TestSimple ();
			TestAutoCtor ();
			TestCached ();
			new BlockingSetTest ().Test ();
		}

		private static void TestCached ()
		{
			var backing = new GenericBackedCache (
				new DictionaryStore (),
				new FileStore ("./myBackingStore"));
			var store = new KVStore<string, DateTime> (backing);
		}

		private static void TestAutoCtor ()
		{
			var kvs = new KVStore<DateTime, TestItem> ();
			kvs[DateTime.Now] = new TestItem (DateTime.Now, new string[] { "foo" });
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
