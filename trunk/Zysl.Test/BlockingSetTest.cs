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
					set.Enter (id);
					Console.WriteLine (Thread.CurrentThread.Name + " locked " + id);
					Thread.Sleep (rnd.Next (0, 100));

					Console.WriteLine (Thread.CurrentThread.Name + " lets go of " + id);
					set.Exit (id);
					Console.WriteLine (Thread.CurrentThread.Name + " released " + id);
					Thread.Sleep (rnd.Next (0, 100));
				}
			};

			new Thread (() => thread ()) {
				Name = "1",
				IsBackground = true
			}.Start ();

			new Thread (() => thread ()) {
				Name = "2",
				IsBackground = true
			}.Start ();

			new Thread (() => {
				for (; ; ) {
					Console.WriteLine (Thread.CurrentThread.Name + " waits");
					set.EnterGlobal ();
					Console.WriteLine (Thread.CurrentThread.Name + " entered");
					Thread.Sleep (rnd.Next (0, 100));

					Console.WriteLine (Thread.CurrentThread.Name + " leaves ");
					set.ExitGlobal ();
					Console.WriteLine (Thread.CurrentThread.Name + " left ");
					Thread.Sleep (rnd.Next (0, 100));
				}
			}) {
				Name = "*",
				IsBackground = true
			}.Start ();

			Thread.Sleep (1000);
		}
	}
}
