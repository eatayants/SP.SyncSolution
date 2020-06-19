using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDynamic.Queries
{
	public class RowNumber : Expression
	{
        private readonly OrderByClause _rowNumberOrderBy;
	    private readonly bool _partitionBy;
        public RowNumber(OrderByClause rowNumberOrderBy, bool partitionBy)
		{
            this._rowNumberOrderBy = rowNumberOrderBy;
            this._partitionBy = partitionBy;
		}
	    public bool UsePartitionBy
	    {
            get { return _partitionBy; }
	    }
        public List<IExpression> PartitionBy
        {
            get { return this._rowNumberOrderBy.Select(item=>item.Expression).ToList();}
        }
        public OrderByClause RowNumberOrderBy
		{
            get { return this._rowNumberOrderBy; }
		}
    }
}
