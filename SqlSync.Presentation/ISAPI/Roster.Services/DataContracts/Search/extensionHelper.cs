using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    public static class OrderByExtension
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sortColumn, bool sortAsc)
        {
            string methodName = string.Format("OrderBy{0}", sortAsc ? "" : "Descending");
            PropertyInfo property = query.ElementType.GetProperty(sortColumn);
            ParameterExpression expression = Expression.Parameter(query.ElementType, "p");
            LambdaExpression expression3 = Expression.Lambda(Expression.MakeMemberAccess(expression, property), new ParameterExpression[] { expression });
            MethodCallExpression expression4 = Expression.Call(typeof(Queryable), methodName, new Type[] { query.ElementType, property.PropertyType }, new Expression[] { query.Expression, Expression.Quote(expression3) });
            return query.Provider.CreateQuery<T>(expression4);
        }

        public static IQueryable<T> ThenBy<T>(this IQueryable<T> query, string sortColumn, bool sortAsc)
        {
            IOrderedQueryable queryable = query as IOrderedQueryable;
            string methodName = string.Format("ThenBy{0}", sortAsc ? "" : "Descending");
            PropertyInfo property = queryable.ElementType.GetProperty(sortColumn);
            ParameterExpression expression = Expression.Parameter(queryable.ElementType, "p");
            LambdaExpression expression3 = Expression.Lambda(Expression.MakeMemberAccess(expression, property), new ParameterExpression[] { expression });
            MethodCallExpression expression4 = Expression.Call(typeof(Queryable), methodName, new Type[] { queryable.ElementType, property.PropertyType }, new Expression[] { queryable.Expression, Expression.Quote(expression3) });
            return queryable.Provider.CreateQuery<T>(expression4);
        }
    }

    public static class filterSearchExtension
    {
        public static decimal filterDecimalValue(this ICollection<abstractSearch> filter, string property)
        {
            decimal result = 0;

            decimalSearch ds = filter.Where(x => x.property.ToLower() == property.ToLower()).FirstOrDefault() as decimalSearch;

            if (ds != default(decimalSearch))
            {
                result = ds.searchTerm.Value;
            }

            return result;
        }

        public static string filterTextValue(this ICollection<abstractSearch> filter, string property)
        {
            string result = string.Empty;

            try
            {
                textSearch ds = filter.Where(x => x.property.ToLower() == property.ToLower()).FirstOrDefault() as textSearch;

                if (ds != default(textSearch))
                {
                    result = ds.searchTerm;
                }
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(string.Format("Property '{0}' not found.", property), ex);
            }

            return result;
        }

        public static int filterIntValue(this ICollection<abstractSearch> filter, string property)
        {
            int result = 0;
            var ds = filter.FirstOrDefault(x => x.property.ToLower() == property.ToLower()) as intSearch;
            if (ds != default(intSearch))
            {

                result = ds.searchTerm ?? 0;
            }

            return result;
        }

        public static DateTime filterDateTimeValue(this ICollection<abstractSearch> filter, string property)
        {
            DateTime result = default(DateTime);

            var ds = filter.Where(x => x.property.ToLower() == property.ToLower()).FirstOrDefault() as dateSearch;

            if (ds != default(dateSearch))
            {
                result = ds.searchTerm.Value;
            }
            return result;
        }

        public static bool filterBoolValue(this ICollection<abstractSearch> filter, string property)
        {
            bool result = false;

            boolSearch ds = filter.Where(x => x.property.ToLower() == property.ToLower()).FirstOrDefault() as boolSearch;

            if (ds != default(boolSearch))
            {
                result = ds.searchTerm;
            }

            return result;
        }

    }

}
