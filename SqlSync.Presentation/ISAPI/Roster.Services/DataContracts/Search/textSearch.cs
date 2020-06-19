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
    public class textSearch : abstractSearch
    {
        [DataMember]
        public string searchTerm { get; set; }
        [DataMember]
        public textComparators comparator { get; set; }
        protected override Expression BuildExpression(MemberExpression property)
        {
            if (this.searchTerm == null)
            {
                return null;
            }

            var searchExpression = Expression.Call(
                property,
                typeof(string).GetMethod(this.comparator.ToString(), new[] { typeof(string) }),
                Expression.Constant(this.searchTerm));

            return searchExpression;
        }
    }

    public enum textComparators
    {
        Contains=0,
        Equals=1
    }
}
