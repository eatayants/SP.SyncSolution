using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Roster.Common;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    public static class entityExtension
    {
        public static object CovnertToObject(this string value)
        {
            object result = value;
            if (string.IsNullOrWhiteSpace(value))
            {
                result = string.Empty;
            }
            else if (value.IsGuid())
            {
                result = value.ToGuid();
            }
            else if (value.IsDate())
            {
                result = value.ToDateTime();
            }
            else if (value.IsInt())
            {
                result = value.ToInt();
            }
            return result;
        }

        public static ExpandoObject NamedToExpando(this ICollection<named> items)
        {
            var result =  (IDictionary<string, object>)new ExpandoObject();
            items.ToList().ForEach(_ =>
            {
                result.Add(_.key, _.value.CovnertToObject()); 
            });
            return (ExpandoObject)result;
        }

        public static ICollection<named> ExpandoToNamed(this ExpandoObject item)
        {
            var result = new List<named>();

            if (item == null) return result;
            var list = (item as IDictionary<string, object>);
            list.ToList().ForEach(_ =>
            {
                result.Add(new named(_.Key, _.Value.ToSafeString())); 
            });
            return result;
        }

        public static ICollection<named> ToNamed<T>(this T item)
        {
            var result = new List<named>();
            var properties = item.GetType().GetProperties().Where(_ => _.PropertyType.IsPrimitive()).ToList();
            properties.ForEach(_ =>
            {
                var value = item.GetPropValue<object>(_.Name);
                result.Add(new named(_.Name, Convert.ToString(value)));
            });
            return result;
        }


        private static bool IsPrimitive(this Type t)
        {
            return new[] { 
                typeof(string), 
                typeof(char),
                typeof(byte),
                typeof(sbyte),
                typeof(ushort),
                typeof(short),
                typeof(uint),
                typeof(int),
                typeof(ulong),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(Guid)              
            }.Contains(t);
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
