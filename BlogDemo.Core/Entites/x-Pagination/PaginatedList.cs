using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Core.Entites.x_xxx
{
    public class PaginatedList<T>:List<T> where T:class
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }

        //总行数
        private int _totalItemsCount;
        public int TotalItemsCount
        {
            get => _totalItemsCount;
            set => _totalItemsCount = value >= 0 ? value : 0;
        }

        //总页数
        public int PageCount => TotalItemsCount / PageSize + (TotalItemsCount % PageSize > 0 ? 1 : 0);

        //是否存在上一页下一页
        public bool HasPrevious => PageIndex > 0;
        public bool HasNext => PageIndex < PageCount - 1;

        public PaginatedList(int pageIndex, int pageSize, int totalItemsCount, IEnumerable<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalItemsCount = totalItemsCount;
            AddRange(data);
        }
    }
}
