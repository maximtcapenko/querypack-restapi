namespace QueryPack.RestApi.Model;

public class Range<TModel>(int first, int last, IEnumerable<TModel> results, int totalCount) where TModel : class
{
    public IEnumerable<TModel> Results { get; } = results;
    public int ResultCount => Results.Count();
    public int TotalCount { get; set; } = totalCount;
    public int First { get; } = first;
    public int Last { get; } = last;
}