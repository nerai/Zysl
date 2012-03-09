using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ezysl.BinStores
{
	public class PathSelector
	{
		public readonly string Root;

		public PathSelector (string root)
		{
			Root = root;
		}

		public virtual string GetPath (string key)
		{
			return Root + "/" + key;
		}

		public virtual IEnumerable<string> PossibleDirectories ()
		{
			yield return Root;
		}
	}

	public class PathSelectorPartitions : PathSelector
	{
		private readonly int _Partitions;

		public PathSelectorPartitions (string root, int partitions)
			: base (root)
		{
			_Partitions = partitions;
		}

		public override string GetPath (string key)
		{
			var s = key.Sum (x => (int) x) % _Partitions;
			return Root + "/" + s.ToString () + "/" + key;
		}

		public override IEnumerable<string> PossibleDirectories ()
		{
			for (int i = 0; i < _Partitions; i++) {
				var p = i.ToString ();
				yield return Root + "/" + p;
			}
		}
	}
}
