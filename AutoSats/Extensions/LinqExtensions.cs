namespace AutoSats.Extensions;

public static class LinqExtensions
{
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> property)
    {
        return source.GroupBy(property).Select(x => x.First());
    }

    public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
        this IEnumerable<TOuter> outer,
        IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner?, TResult> resultSelector)
    {
        return outer
            .GroupJoin(inner, outerKeySelector, innerKeySelector, (outerObj, inners) =>
            new
            {
                outerObj,
                inners = inners.DefaultIfEmpty()
            })
        .SelectMany(a => a.inners.Select(innerObj => resultSelector(a.outerObj, innerObj)));
    }
}
