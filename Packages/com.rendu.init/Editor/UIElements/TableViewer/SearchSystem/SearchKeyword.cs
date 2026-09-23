#if UNITY_2019_4_OR_NEWER

namespace RD.Init.Editor
{
    /// <summary>
    /// 搜索关键字
    /// </summary>
    public class SearchKeyword : ISearchCommand
    {
        public string SearchTag;
        public string Keyword;

        public bool CompareTo(string value)
        {
            return value.Contains(Keyword);
        }
    }
}
#endif