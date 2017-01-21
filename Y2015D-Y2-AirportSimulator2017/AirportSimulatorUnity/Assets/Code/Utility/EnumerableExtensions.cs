using System.Collections.Generic;
using System.Linq;

static class EnumerableExtensions
{
	public static IEnumerable<T> DeepClone<T>(this IEnumerable<T> target) where T : IDeepCloneable<T>
	{
		return target.Select(x => x.DeepClone());
	}
}