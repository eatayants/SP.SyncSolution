using System;
using System.Collections.Generic;
using SqlSync.Common;
using SqlSync.Model.DataContext;
using SqlDynamic.Queries;

namespace SqlSync.BL.Extentions
{
	public class QueryParams
	{
		public QueryParams()
		{
			SkipRows = 0;
			TakeRows = 0;
            SelectCriteria = new List<ListMetadataField>();
            JoinCriteria = new List<Tuple<JoinType, TableSource, IBoolean>>();
            WhereCriteria = new List<Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>>();
            InternalWhere = new List<IBoolean>();
            OrderCriteria = new List<Tuple<ListMetadataField, SortDirection, string>>();
		}
        public bool Dictinct { get; set; }
        public int SkipRows { get; set; }
		public int TakeRows { get; set; }
        public List<ListMetadataField> SelectCriteria { get; set; }
        public List<Tuple<JoinType, TableSource, IBoolean>> JoinCriteria { get; set; }
        public List<Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>> WhereCriteria { get; set; }
        public List<Tuple<ListMetadataField, SortDirection, string>> OrderCriteria { get; set; }
        public List<IBoolean> InternalWhere { get; set; }
        public QueryParams Join(QueryParams queryParams)
        {
            if (queryParams == null) return this;
            Dictinct = queryParams.Dictinct;
            TakeRows = queryParams.TakeRows;
            SkipRows = queryParams.SkipRows;
            InternalWhere.AddRange(queryParams.InternalWhere);
            WhereCriteria.AddRange(queryParams.WhereCriteria);
            JoinCriteria.AddRange(queryParams.JoinCriteria);
            OrderCriteria.AddRange(queryParams.OrderCriteria);
            return this;
        }
    }
}
