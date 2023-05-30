namespace QueryPack.RestApi.Model
{
    public class Range<TModel> where TModel : class
    {
        public Range(int first, int last, IEnumerable<TModel> results, int totalCount)
        {
            TotalCount = totalCount;
            Results = results;
            First = first;
            Last = last;
        }

        public IEnumerable<TModel> Results { get; }
        public int ResultCount => Results.Count();
        public int TotalCount { get; set; }
        public int First { get; }
        public int Last { get; }
    }
}