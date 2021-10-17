using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WaterSortPuzzle
{
	public static class TubePermutations
	{
		public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> src)
		{
			var lSrc = src.ToList();

			if (lSrc.Count < 2)
			{
				yield return lSrc;
				yield break;
			}

			for (var i = 0; i < lSrc.Count; ++i)
			{
				var lm1 = lSrc.Take(i).Union(lSrc.Skip(i + 1)).ToList();
				var perm1 = lm1.GetPermutations().ToList();

				foreach (var lp in perm1)
				{
					var ret = new List<T> { lSrc[i] };
					foreach (var p in lp) ret.Add(p);
					yield return ret;
				}
			}
		}
	}
}
