using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using SqlSync.Common;
using SqlSync.Model.DataContext;
using SqlDynamic.Queries;

namespace SqlSync.BL.Extentions
{
    public class QueryCriteria
    {
        public QueryCriteria()
        {
            SkipRows = 0;
            TakeRows = 0;
            SelectCriteria = new List<IExpression>();
            TrusteeIds = new List<int>();
            JoinCriteria = new List<Tuple<JoinType, TableSource, IBoolean>>();
            WhereCriteria = new List<Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>>();
            InternalWhere = new List<IBoolean>();
            OrderCriteria = new List<Tuple<ListMetadataField, SortDirection, string>>();
			ListMetadataFields = new List<ListMetadataField>();
        }
        public bool AccessCriteria { get; set; }
        public List<int> TrusteeIds { get; set; }
        public bool Dictinct { get; set; }
        public int SkipRows { get; set; }
        public int TakeRows { get; set; }
		public List<ListMetadataField> ListMetadataFields { get; set; }
        public List<IExpression> SelectCriteria { get; set; }
        public List<Tuple<JoinType, TableSource, IBoolean>> JoinCriteria { get; set; }
		public List<Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>> WhereCriteria { get; set; }
		public List<Tuple<ListMetadataField, SortDirection,string>> OrderCriteria { get; set; }
        public List<IBoolean> InternalWhere { get; set; }
		public QueryCriteria Join(QueryParams queryParams)
		{
			if (queryParams == null) return this;
            Dictinct = queryParams.Dictinct;
            TakeRows = queryParams.TakeRows;
			SkipRows = queryParams.SkipRows;
			WhereCriteria.AddRange(queryParams.WhereCriteria);
            InternalWhere.AddRange(queryParams.InternalWhere);
            JoinCriteria.AddRange(queryParams.JoinCriteria);
            if (queryParams.OrderCriteria.Count > 0)
            {
			    OrderCriteria = queryParams.OrderCriteria; // overwrite default view ordering
            }
			return this;
		}

		public QueryCriteria Concat(QueryCriteria queryCriteria)
	    {
		    if (queryCriteria == null) return this;
		    Dictinct = queryCriteria.Dictinct;
			SelectCriteria.AddRange(queryCriteria.SelectCriteria);
			JoinCriteria.AddRange(queryCriteria.JoinCriteria);
			WhereCriteria.AddRange(queryCriteria.WhereCriteria);
            InternalWhere.AddRange(queryCriteria.InternalWhere);
			OrderCriteria.AddRange(queryCriteria.OrderCriteria);
			return this;
		}
    }
}
