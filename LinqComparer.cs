using System;
using System.Collections.Generic;

namespace WaterSortPuzzle
{
	/// <summary>
	/// Helper class for LinqExtensions
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	public class LinqComparer<TSource> : IEqualityComparer<TSource>
	{
		private readonly Func<TSource, TSource, bool> _linqCmp;
		private readonly Func<TSource, int> _hashCode;

		public LinqComparer(Func<TSource, TSource, bool> cmp, Func<TSource, int> hashCode = null)
		{
			if (cmp == null)
			{
				var errMsg = $"Comparison ({nameof(cmp)}) function may not be null in LinqComparer(..) ctor.";
				throw new ArgumentException(errMsg, nameof(cmp));
			}

			_linqCmp = cmp;
			_hashCode = hashCode ?? (_ => 0);
		}

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool Equals(TSource x, TSource y) => _linqCmp(x, y);

		/// <summary>
		/// Returns a hash code for the specified object.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public int GetHashCode(TSource x) => _hashCode(x);
	}
}
