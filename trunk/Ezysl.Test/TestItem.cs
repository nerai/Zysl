using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ezysl.Test
{
	[DataContract]
	internal class TestItem
	{
		public TestItem (DateTime time, IEnumerable<string> strings)
		{
			Time = time;
			List = strings == null ? null : strings.ToList ();
		}

		[DataMember]
		public DateTime Time;
		[DataMember]
		public List<string> List;

		public override string ToString ()
		{
			return string.Format ("TestItem[{0}; ({1})]",
				Time.ToString (),
				List == null ? "null" : string.Join (", ", List));
		}
	}
}
