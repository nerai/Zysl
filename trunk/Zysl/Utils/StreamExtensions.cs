using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Zysl.Utils
{
	internal static class StreamExtensions
	{
		public static byte[] ReadAll (this Stream stream)
		{
			var buffer = new byte[32768];

			using (var ms = new System.IO.MemoryStream ()) {
				while (true) {
					int read = stream.Read (buffer, 0, buffer.Length);
					if (read <= 0) {
						break;
					}
					ms.Write (buffer, 0, read);
				}

				return ms.ToArray ();
			}
		}
	}
}
