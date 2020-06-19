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
    public class decimalSearch : abstractSearch
    {
        [DataMember]
        public decimal? searchTerm { get; set; }        
        [DataMember]
        public numericComparators comparator { get; set; }
        protected override Expression BuildExpression(MemberExpression property)
        {
            if (!searchTerm.HasValue)
            {
                return null;
            }
            var searchExpression = GetFilterExpression(property);
            return searchExpression;
        }

        private Expression GetFilterExpression(MemberExpression property)
        {
            switch (this.comparator)
            {
                case numericComparators.Less:
                    return Expression.LessThan(property, Expression.Constant(this.searchTerm.Value));
                case numericComparators.LessOrEqual:
                    return Expression.LessThanOrEqual(property, Expression.Constant(this.searchTerm.Value));
                case numericComparators.Equal:
                    return Expression.Equal(property, Expression.Constant(this.searchTerm.Value));
                case numericComparators.GreaterOrEqual:
                    return Expression.GreaterThanOrEqual(property, Expression.Constant(this.searchTerm.Value));
                case numericComparators.Greater:
                    return Expression.GreaterThan(property, Expression.Constant(this.searchTerm.Value));
                default:
                    throw new InvalidOperationException("Comparator not supported.");
            }
        }
    }
}
