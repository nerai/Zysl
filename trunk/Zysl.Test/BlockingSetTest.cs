using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Zysl
{
	public class BlockingSetTest
	{
		public void Test ()
		{
			var rnd = new Random ();
			var set = new BlockingSet<string> ();

			Action thread = () => {
				for (; ; ) {
					var id = rnd.NextDouble () < 0.5 ? "a" : "b";

					Console.WriteLine (Thread.CurrentThread.Name + " waits for " + id);
					set.Add (id);
					Console.WriteLine (Thread.CurrentThread.Name + " locked " + id);
					Thread.Sleep (rnd.Next (0, 100));

					set.Remove (id);
					Console.WriteLine (Thread.CurrentThread.Name + " released " + id);
					Thread.Sleep (rnd.Next (0, 100));
				}
			};

			new Thread (() => thread ()) {
				Name = "L1",
				IsBackground = true
			}.Start ();

			new Thread (() => thread ()) {
				Name = "L2",
				IsBackground = true
			}.Start ();

			Thread.Sleep (1000);
		}
	}
}
