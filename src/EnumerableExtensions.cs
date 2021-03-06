using System.Collections.Generic;
using System.Collections.Immutable;

namespace VinylCutter
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> Yield<T> (this T item)
		{
			yield return item;
		}
	}
}
