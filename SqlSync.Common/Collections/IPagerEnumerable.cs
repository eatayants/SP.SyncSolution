#region

using System.Collections.Generic;

#endregion

namespace Roster.Common.Collections
{
	public interface IPagerEnumerable<T> : IEnumerable<T>
	{
		IContentModel Content { get; set; }
		int PageIndex { get; }
		int PageSize { get; }
		int TotalCount { get; }
		int TotalPages { get; }
		bool HasPreviousPage { get; }
		bool HasNextPage { get; }
	}
}