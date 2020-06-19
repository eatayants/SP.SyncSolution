using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    public static class searchExtension
    {
        public static IQueryable<T> ApplySearchCriterias<T>(this IQueryable<T> query, IEnumerable<abstractSearch> searchCriterias)
        {
            if (searchCriterias == null)
            {
                return query;
            }

            foreach (var criteria in searchCriterias)
            {
                if (!criteria.isIgnore)
                {
                    query = criteria.ApplyToQuery(query);
                }
            }

            return query;
        }

        public static ICollection<abstractSearch> GetDefaultSearchCriterias(this Type type)
        {
            var properties = type.GetProperties()
                .Where(p => p.CanRead && p.CanWrite)
                .OrderBy(p => p.Name);

            var searchCriterias = properties
                .Select(p => CreateSearchCriteria(p.PropertyType, p.Name))
                .Where(s => s != null)
                .ToList();

            return searchCriterias;
        }

        public static ICollection<abstractSearch> AddCustomSearchCriteria<T>(this ICollection<abstractSearch> searchCriterias, Expression<Func<T, object>> property)
        {
            Type propertyType = null;
            string fullPropertyPath = GetPropertyPath(property, out propertyType);

            abstractSearch searchCriteria = CreateSearchCriteria(propertyType, fullPropertyPath);

            if (searchCriteria != null)
            {
                searchCriterias.Add(searchCriteria);
            }

            return searchCriterias;
        }

        private static abstractSearch CreateSearchCriteria(Type type, string property)
        {
            abstractSearch result = null;

            if (type.Equals(typeof(string)))
            {
                result = new textSearch();
            }
            else if (type.Equals(typeof(int)) || type.Equals(typeof(int?)))
            {
                result = new intSearch();
            }
            else if (type.Equals(typeof(decimal)) || type.Equals(typeof(decimal?)))
            {
                result = new decimalSearch();
            }
            else if (type.Equals(typeof(DateTime)) || type.Equals(typeof(DateTime?)))
            {
                result = new dateSearch();
            }

            if (result != null)
            {
                result.property = property;
            }

            return result;
        }
        
        private static string GetPropertyPath<T>(Expression<Func<T, object>> expression, out Type targetType)
        {
            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException(@"Please provide a lambda expression like 'n => n.PropertyName'", "expression");
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;
            targetType = propertyInfo.PropertyType;

            string property = memberExpression.ToString();

            return property.Substring(property.IndexOf('.') + 1);
        }
    }
}
