using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    [DataContract]
    public class dateSearch : abstractSearch
    {
        [DataMember]
        public DateTime? searchTerm { get; set; }
        [DataMember]
        public DateTime? searchTerm2 { get; set; }

        [DataMember]
        public dateComparators comparator { get; set; }

        protected override Expression BuildExpression(MemberExpression property)
        {
            Expression searchExpression1 = null;
            Expression searchExpression2 = null;

            if (this.searchTerm.HasValue)
            {
                searchExpression1 = this.GetFilterExpression(property);
            }

            if (this.comparator == dateComparators.InRange && this.searchTerm2.HasValue)
            {
                searchExpression2 = Expression.LessThanOrEqual(property, Expression.Constant(this.searchTerm2.Value));
            }

            if (searchExpression1 == null && searchExpression2 == null)
            {
                return null;
            }
            else if (searchExpression1 != null && searchExpression2 != null)
            {
                var combinedExpression = Expression.AndAlso(searchExpression1, searchExpression2);
                return combinedExpression;
            }
            else if (searchExpression1 != null)
            {
                return searchExpression1;
            }
            else
            {
                return searchExpression2;
            }
        }

        private Expression GetFilterExpression(MemberExpression property)
        {
            switch (this.comparator)
            {
                case dateComparators.Less:
                    return Expression.LessThan(property, Expression.Constant(this.searchTerm.Value));
                case dateComparators.LessOrEqual:
                    return Expression.LessThanOrEqual(property, Expression.Constant(this.searchTerm.Value));
                case dateComparators.Equal:
                    return Expression.Equal(property, Expression.Constant(this.searchTerm.Value));
                case dateComparators.GreaterOrEqual:
                case dateComparators.InRange:
                    return Expression.GreaterThanOrEqual(property, Expression.Constant(this.searchTerm.Value));
                case dateComparators.Greater:
                    return Expression.GreaterThan(property, Expression.Constant(this.searchTerm.Value));
                default:
                    throw new InvalidOperationException("Comparator not supported.");
            }
        }
    }
}
