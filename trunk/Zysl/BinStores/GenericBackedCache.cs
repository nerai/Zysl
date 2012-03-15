using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace Zysl.BinStores
{
	public class GenericBackedCache :
		IBinStore,
		IDisposable
	{
		private readonly Logger _Log = LogManager.GetCurrentClassLogger ();

		private readonly IBinStore _Cache;
		private readonly IBinStore _Files;

		private readonly Dictionary<string, int> _AccessCache = new Dictionary<string, int> ();
		private readonly HashSet<string> _Dirty = new HashSet<string> ();

		public int MaxSize = 1024;

		public double EfficiencyFactor { get; private set; }

		public GenericBackedCache (IBinStore cache, IBinStore store)
		{
			_Cache = cache;
			_Files = store;

			EfficiencyFactor = 1.0;
		}

		public bool IsCached (string key)
		{
			return _Cache.ContainsKey (key);
		}

		private void Compact ()
		{
			if (_AccessCache.Count > MaxSize) {
				/*
				 * calc stats
				 */
				var oldEff = EfficiencyFactor;
				EfficiencyFactor = 0.5 * EfficiencyFactor + 0.5 * _AccessCache.Values.Sum () / _AccessCache.Count;
				_Log.Info ("Cache.Compact: MaxSize {0}, Dirty {1}, Access {2}, AccessSum {3}, efficiency previous {4:0.00}, now {5:0.00}. Cache {6} items, Backend {7} items.",
					 MaxSize,
					 _Dirty.Count,
					 _AccessCache.Count,
					 _AccessCache.Values.Sum (),
					 oldEff,
					 EfficiencyFactor,
					 _Cache.Count,
					 _Files.Count);

				/*
				 * compact cache
				 */
				var items = _AccessCache
					.OrderByDescending (x => x.Value)
					.Skip (MaxSize / 2)
					.ToArray ();

				foreach (var item in items) {
					if (_Dirty.Contains (item.Key)) {
						_Files[item.Key] = _Cache[item.Key];
						_Dirty.Remove (item.Key);
					}

					_Cache.Remove (item.Key);
					_AccessCache.Remove (item.Key);
				}

				foreach (var item in _AccessCache.ToArray ()) {
					_AccessCache[item.Key] = item.Value / 2;
				}
			}
		}

		public void Flush ()
		{
			foreach (var item in _Dirty) {
				_Files[item] = _Cache[item];
			}
			_Dirty.Clear ();

			_Files.Flush ();
		}

		public bool ContainsKey (string key)
		{
			return _Cache.ContainsKey (key) || _Files.ContainsKey (key);
		}

		public bool TryGetValue (string key, out byte[] value)
		{
			if (_Cache.TryGetValue (key, out value)) {
				int count;
				_AccessCache.TryGetValue (key, out count);
				_AccessCache[key] = count + 1;

				return true;
			}

			if (_Files.TryGetValue (key, out value)) {
				Compact ();

				_Cache[key] = value;
				_AccessCache.Add (key, 1);
				return true;
			}

			return false;
		}

		public bool TrySetValue (string key, byte[] value)
		{
			Compact ();

			int count;
			_AccessCache.TryGetValue (key, out count);
			_AccessCache[key] = count;

			_Cache[key] = value;

			if (_Cache.TrySetValue (key, value)) {
				_Dirty.Add (key);
				return true;
			}
			else {
				return false;
			}
		}

		public void Dispose ()
		{
			Flush ();
			MaxSize = 0;
			Compact ();
		}

		public byte[] this[string key]
		{
			get
			{
				byte[] value;

				if (TryGetValue (key, out value)) {
					return value;
				}
				else {
					throw new KeyNotFoundException ();
				}
			}
			set
			{
				if (!TrySetValue (key, value)) {
					throw new Exception ("Failed to set value.");
				}
			}
		}

		public bool Remove (string key)
		{
			if (_Cache.Remove (key)) {
				_AccessCache.Remove (key);
				_Dirty.Remove (key);
			}
			else {
				return false;
			}

			return _Files.Remove (key);
		}

		public IEnumerable<string> ListKeys ()
		{
			// has to be calculated completely here due to caching
			var set = new HashSet<string> (_Files.ListKeys ());

			foreach (var item in _Cache.ListKeys ()) {
				set.Add (item);
			}

			return set;
		}

		public long Count
		{
			get
			{
				return _Files.Count + _AccessCache.Count (x => !_Files.ContainsKey (x.Key));
			}
		}

		public string Name
		{
			get { return _Files.Name + "-" + _Cache.Name; }
		}
	}
}
