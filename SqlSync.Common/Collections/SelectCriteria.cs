#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#endregion

namespace SqlSync.Common.Collections
{
	public class SelectCriteria
	{
		public SortDirection Sort { get; set; }

		public int PageIndex { get; set; }

		public int PageSize { get; set; }

		public int RowIndex { get; set; }

		public void Init(SelectCriteria source)
		{
			Sort = source.Sort;
			PageIndex = source.PageIndex;
			PageSize = source.PageSize;
		}
	}

	public class SelectCriteria<TEntity> : SelectCriteria
	{
		public SelectCriteria()
		{
		}

		public SelectCriteria(SelectCriteria source)
		{
			Init(source);
		}

		public IList<Expression<Func<TEntity, bool>>> ExpressionSelect { get; set; }
		public IList<Expression<Func<TEntity, dynamic>>> ExpressionSort { get; set; }
	    public bool IsDistinct { get; set; }
	}
}