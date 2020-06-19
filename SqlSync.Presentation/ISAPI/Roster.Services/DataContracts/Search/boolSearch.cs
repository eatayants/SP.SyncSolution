using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    public class boolSearch : abstractSearch
    {
        [DataMember]
        public bool searchTerm { get; set; }
        [DataMember]
        public boolComparators comparator { get; set; }

        protected override Expression BuildExpression(MemberExpression property)
        {
            Expression searchExpression = this.GetFilterExpression(property);

            return searchExpression;
        }
        protected virtual Expression GetFilterExpression(MemberExpression property)
        {
            switch (this.comparator)
            {
                case boolComparators.Equal:
                    return Expression.Equal(property, Expression.Constant(this.searchTerm));
                case boolComparators.NotEqual:
                    return Expression.Equal(property, Expression.Constant(!this.searchTerm));
                default:
                    throw new InvalidOperationException("Comparator not supported.");
            }
        }
    }

    public enum boolComparators
    {
        Equal,
        NotEqual
    }
}
