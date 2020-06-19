#region

using System;
using System.Collections.Generic;

#endregion

namespace Roster.Common.Collections
{
	public class PagerList<T> : List<T>, IPagerEnumerable<T>
	{
		public PagerList(IEnumerable<T> source, int totalCount, int pageIndex, int pageSize)
			: base(source)
		{
			pageSize = (pageSize == 0 ? 1 : pageSize);
			TotalCount = totalCount;
			TotalPages = totalCount /(pageSize == 0 ? 1 : pageSize);

			if (totalCount%pageSize > 0)
			{
				TotalPages++;
			}
			PageSize = pageSize;
			PageIndex = pageIndex;
		}

		#region IPagerEnumerable<T> Members

		public IContentModel Content { get; set; }
		public int TotalCount { get; private set; }
		public int PageIndex { get; private set; }
		public int TotalPages { get; private set; }
		public int PageSize { get; private set; }


		public bool HasPreviousPage
		{
			get { return (PageIndex > 0); }
		}

		public bool HasNextPage
		{
			get { return (PageIndex + 1 < TotalPages); }
		}

		#endregion
	}
}