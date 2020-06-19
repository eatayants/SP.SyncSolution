using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    [KnownType(typeof(dateSearch))]
    [KnownType(typeof(intSearch))]
    [KnownType(typeof(decimalSearch))]
    [KnownType(typeof(textSearch))]
    [KnownType(typeof(boolSearch))]
    [DataContract]
    public abstract class abstractSearch
    {
        [DataMember]
        public string property { get; set; }

        public string LabelText
        {
            get
            {
                if (this.property == null)
                {
                    return null;
                }

                string[] parts = this.property.Split('.');

                return parts[parts.Length - 1];
            }
        }
        [DataMember]
        public bool isIgnore { get; set; }
        
        internal IQueryable<T> ApplyToQuery<T>(IQueryable<T> query)
        {
            var arg = Expression.Parameter(typeof(T), "p");
            var property = this.GetPropertyAccess(arg);

            Expression searchExpression = null;

            if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                searchExpression = this.BuildExpression(Expression.Property(property, "Value"));
            }
            else
            {
                searchExpression = this.BuildExpression(property);
            }

            if (searchExpression == null)
            {
                return query;
            }
            else
            {
                var predicate = CreatePredicateWithNullCheck<T>(searchExpression, arg, property);
                return query.Where(predicate);
            }
        }

        protected abstract Expression BuildExpression(MemberExpression property);

        private MemberExpression GetPropertyAccess(ParameterExpression arg)
        {
            string[] parts = this.property.Split('.');

            MemberExpression property = Expression.Property(arg, parts[0]);

            for (int i = 1; i < parts.Length; i++)
            {
                property = Expression.Property(property, parts[i]);
            }

            return property;
        }

        private Expression<Func<T, bool>> CreatePredicateWithNullCheck<T>(Expression searchExpression, ParameterExpression arg, MemberExpression targetProperty)
        {
            string[] parts = this.property.Split('.');

            Expression nullCheckExpression = null;
            if (parts.Length > 1)
            {
                MemberExpression property = Expression.Property(arg, parts[0]);
                nullCheckExpression = Expression.NotEqual(property, Expression.Constant(null));

                for (int i = 1; i < parts.Length - 1; i++)
                {
                    property = Expression.Property(property, parts[i]);
                    Expression innerNullCheckExpression = Expression.NotEqual(property, Expression.Constant(null));

                    nullCheckExpression = Expression.AndAlso(nullCheckExpression, innerNullCheckExpression);
                }
            }

            if (!targetProperty.Type.IsValueType || (targetProperty.Type.IsGenericType && targetProperty.Type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                var innerNullCheckExpression = Expression.NotEqual(targetProperty, Expression.Constant(null));

                if (nullCheckExpression == null)
                {
                    nullCheckExpression = innerNullCheckExpression;
                }
                else
                {
                    nullCheckExpression = Expression.AndAlso(nullCheckExpression, innerNullCheckExpression);
                }
            }

            if (nullCheckExpression == null)
            {
                return Expression.Lambda<Func<T, bool>>(searchExpression, arg);
            }
            else
            {
                var combinedExpression = Expression.AndAlso(nullCheckExpression, searchExpression);

                var predicate = Expression.Lambda<Func<T, bool>>(combinedExpression, arg);

                return predicate;
            }
        }
    }
}
