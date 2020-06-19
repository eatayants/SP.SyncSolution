#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Roster.Common.Collections;
using Roster.Common.Helpers;

#endregion

namespace Roster.Common
{
	#region EnumExtention
	public class StringValueAttribute : Attribute
	{
		public string StringPresentaion { get; protected set; }
		public StringValueAttribute(string value)
		{
			StringPresentaion = value;
		}
	}

	public static class EnumExtensions
	{
        public static bool In<T>(this T val, params T[] values) where T : struct
        {
            return values.Contains(val);
        }

    	public static string GetStringValue(this object value)
		{
			var type = value.GetType();
			var fieldInfo = type.GetField(value.ToString());
			if (fieldInfo == null)
			{
				return string.Empty;
			}
			var attribs = fieldInfo.GetCustomAttributes(typeof (StringValueAttribute), false) as StringValueAttribute[];
			return attribs != null && attribs.Length > 0 ? attribs[0].StringPresentaion : string.Empty;
		}
	}

	#endregion

	#region Compiled Expression Extention

	public static class CompiledExpression
	{
		public static Func<TObject, TProperty> Getter<TObject, TProperty>(string propertyName)
		{
			var paramExpression = Expression.Parameter(typeof(TObject), "value");
			Expression propertyGetterExpression = Expression.Property(paramExpression, propertyName);
			Func<TObject, TProperty> result = Expression.Lambda<Func<TObject, TProperty>>(propertyGetterExpression, paramExpression).Compile();
			return result;
		}

		public static Action<TObject, TProperty> Setter<TObject, TProperty>(string propertyName)
		{
			ParameterExpression paramExpression = Expression.Parameter(typeof(TObject));
			ParameterExpression paramExpression2 = Expression.Parameter(typeof(TProperty), propertyName);
			MemberExpression propertyGetterExpression = Expression.Property(paramExpression, propertyName);
			Action<TObject, TProperty> result = Expression.Lambda<Action<TObject, TProperty>>
			(
				Expression.Assign(propertyGetterExpression, paramExpression2), paramExpression, paramExpression2
			).Compile();
			return result;
		}
	}

	#endregion Compiled Expression Extention

	public static class EnumerableExtensions
	{
        public static bool IsXml(this object value)
        {
            return value.ToSafeString().StartsWith("<");
        }
        
        public static string ListToXml(this List<string> list)
        {
            return list.IsEmpty() ? string.Empty : 
                new XElement("Items", list.Select(item => new XElement("Value", item))).ToString();
        }

	    public static List<string> XmlToList(this object value)
        {
            var result = new List<string>();
            if (String.IsNullOrWhiteSpace(value.ToSafeString()))
            {
                return new List<string>();
            }
            try
            {
                var items = XDocument.Parse(value.ToSafeString()).Descendants("Items").ToList();
                if (items.IsEmpty())
                {
                    return result;
                }
                result.AddRange(items.Elements("Value").Select(item=>item.Value).ToList());
                return result;
            }
            catch
            {
                return result;
            }
        }

        public static ExpandoObject Find(this List<ExpandoObject> list, string key, Guid criteria)
        {
            return (from item in list let value = item.FirstOrDefault(x => x.Key == key).Value 
                    where value.ToString().ToGuid() == criteria select item).FirstOrDefault();
        }

        public static object GetValue(this IDictionary<string, object> propetylist, string key)
        {
            object result = null;
            if (propetylist.ContainsKey(key))
            {
                result = propetylist[key];
            }
            return result;
        }

		public static object FindValue(this ExpandoObject eo, string key)
        {
            return eo.FirstOrDefault(x => x.Key == key).Value;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			return new HashSet<T>(source);
		}

		public static MinMax<T> WithMinMaxPair<T, TV>(this IEnumerable<T> collection, Func<T, TV> valueSelector)
		where TV : IComparable<TV>
		{
			T withMin = default(T), withMax = default(T);
			bool hasMin = false, hasMax = false;
			TV min = default(TV), max = default(TV);
			foreach (var item in collection)
			{
				var val = valueSelector(item);
				if (!hasMax || val.CompareTo(max) > 0)
				{
					hasMax = true;
					max = val;
					withMax = item;
				}
				if (hasMin && val.CompareTo(min) >= 0) continue;
				hasMin = true;
				min = val;
				withMin = item;
			}
			return new MinMax<T>(withMin, withMax);
		}

		public static string ToString<T>(this IEnumerable<T> source, Func<T, string> toString, string separator)
		{
			StringBuilder sb = null;
			foreach (var item in source)
			{
				if (sb == null)
				{
					sb = new StringBuilder();
				}
				else
				{
					sb.Append(separator);
				}
				sb.Append(toString(item));
			}

			return sb == null ? "" : sb.ToString();
		}

		public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> elements)
		{
			foreach (var item in elements)
			{
				queue.Enqueue(item);
			}
		}

		public static class KVP
		{
			public static KeyValuePair<TK, TV> Create<TK, TV>(TK key, TV value)
			{
				return new KeyValuePair<TK, TV>(key, value);
			}
		}

		public static TV TryGetDictionaryValue<TK, TV>(this IDictionary<TK, TV> dictionary, TK key) where TV : class
		{
			if (dictionary == null)
			{
				return null;
			}
			TV result;
			return dictionary.TryGetValue(key, out result) ? result : null;
		}

		public static IEnumerable<TEntity> Descendants<TEntity>(this IEnumerable<TEntity> source, Func<TEntity, IEnumerable<TEntity>> descendBy)
		{
			var enumerable = source as IList<TEntity> ?? source.ToList();
			if (enumerable.IsEmpty()) yield break;
			foreach (var value in enumerable)
			{
				yield return value;
				if (descendBy(value).IsEmpty()) continue;
				foreach (var child in descendBy(value).Descendants<TEntity>(descendBy))
				{
					yield return child;
				}
			}
		}

		public static IEnumerable<Hierarchy<TEntity>> AsHierarchyList<TEntity, TProperty>(
			this IEnumerable<TEntity> allItems, Func<TEntity, TProperty> idProperty, Func<TEntity, TProperty> parentIdProperty) where TEntity : class
		{
			var result = CreateHierarchy(allItems, default(TEntity), idProperty, parentIdProperty, null, 0, 0, string.Empty);
			var flatHierarchies = new List<Hierarchy<TEntity>>();
			foreach (var entity in result)
			{
				flatHierarchies.Add(entity);
				flatHierarchies.AddRange(entity.Items.Descendants(p => p.Items).ToList());
			}
			return flatHierarchies;
		}

		public static IEnumerable<TEntity> AsFlatten<TEntity>(this IEnumerable<Hierarchy<TEntity>> items, Expression<Func<TEntity, object>> depth) where TEntity : class
		{
			var hierarchies = items as IList<Hierarchy<TEntity>> ?? items.ToList();
			var flatHierarchies = new List<Hierarchy<TEntity>>();
			foreach (var entity in hierarchies)
			{
				flatHierarchies.Add(entity);
				flatHierarchies.AddRange(entity.Items.Descendants(p => p.Items).ToList());
			}
			foreach (var entity in flatHierarchies)
			{
				entity.Entity.SetPropertyValue(depth, entity.Depth);
			}
			var stack = flatHierarchies.Select(entity => entity.Entity).ToList();
			return stack;
		}

		public static void SetPropertyValue<T>(this T target, Expression<Func<T, object>> memberExpr, object value)
		{
			var memberSelectorExpression = memberExpr.Body as MemberExpression;
			if (memberSelectorExpression == null)
			{
				var unaryExpr = memberExpr.Body as UnaryExpression;
				if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
					memberSelectorExpression = unaryExpr.Operand as MemberExpression;
			}
			if (memberSelectorExpression == null) return;
			var property = memberSelectorExpression.Member as PropertyInfo;
			if (property != null)
			{
				property.SetValue(target, value, null);
			}
		}

		public static object GetPropValue(this object obj, string name)
		{
			foreach (var part in name.Split('.'))
			{
				if (obj == null) { return null; }

				var type = obj.GetType();
				var info = type.GetProperty(part);
				if (info == null) { return null; }
				obj = info.GetValue(obj, null);
			}
			return obj;
		}

		public static T GetPropValue<T>(this Object obj, String name)
		{
			var retval = GetPropValue(obj, name);
			if (retval == null) { return default(T); }
			return (T)retval;
		}

		private static IEnumerable<Hierarchy<TEntity>>CreateHierarchy<TEntity, TProperty>(
			IEnumerable<TEntity> allItems,
			TEntity parentItem,
			Func<TEntity, TProperty> idProperty,
			Func<TEntity, TProperty> parentIdProperty,
			object rootItemId,
			int maxDepth,
			int depth,
			string path) where TEntity : class
		{
			IEnumerable<TEntity> childs;

			if (rootItemId != null)
			{
				childs = allItems.Where(i => idProperty(i).Equals(rootItemId));
			}
			else
			{
				childs = parentItem == null ? allItems.Where(i => parentIdProperty(i).Equals(default(TProperty))) : 
					allItems.Where(i => parentIdProperty(i).Equals(idProperty(parentItem)));
			}
			if (childs.Any())
			{
				depth++;
				if ((depth <= maxDepth) || (maxDepth == 0))
				{
					foreach (var item in childs)
					{
						yield return new Hierarchy<TEntity>
							{
								Entity = item,
								Items = CreateHierarchy(allItems.AsEnumerable(), item, idProperty, parentIdProperty, null, maxDepth, depth, item.GetPropValue("Name").ToSafeString()),
								Depth = depth,
								Parent = parentItem,
								Path = string.Format("{0}/{1}", path, item.GetPropValue("Name").ToSafeString())
							};
					}
				}
			}
		}

		/// <summary>
		///   LINQ to Objects (IEnumerable) AsHierachy() extension method
		/// </summary>
		/// <typeparam name = "TEntity">Entity class</typeparam>
		/// <typeparam name = "TProperty">Property of entity class</typeparam>
		/// <param name = "allItems">Flat collection of entities</param>
		/// <param name = "idProperty">Func delegete to Id/Key of entity</param>
		/// <param name = "parentIdProperty">Func delegete to parent Id/Key</param>
		/// <returns>Hierarchical structure of entities</returns>
		public static IEnumerable<Hierarchy<TEntity>> AsHierarchy<TEntity, TProperty>(
			this IEnumerable<TEntity> allItems,Func<TEntity, TProperty> idProperty,Func<TEntity, TProperty> parentIdProperty) where TEntity : class
		{
			var result = CreateHierarchy(allItems, default(TEntity), idProperty, parentIdProperty, null, 0, 0, string.Empty);
			return result.ToList();
		}

		/// <summary>
		///   LINQ to Objects (IEnumerable) AsHierachy() extension method
		/// </summary>
		/// <typeparam name = "TEntity">Entity class</typeparam>
		/// <typeparam name = "TProperty">Property of entity class</typeparam>
		/// <param name = "allItems">Flat collection of entities</param>
		/// <param name = "idProperty">Func delegete to Id/Key of entity</param>
		/// <param name = "parentIdProperty">Func delegete to parent Id/Key</param>
		/// <param name = "rootItemId">Value of root item Id/Key</param>
		/// <returns>Hierarchical structure of entities</returns>
		public static IEnumerable<Hierarchy<TEntity>> AsHierarchy<TEntity, TProperty>(this IEnumerable<TEntity> allItems,
			Func<TEntity, TProperty> idProperty,Func<TEntity, TProperty> parentIdProperty, object rootItemId) where TEntity : class
		{
			var result = CreateHierarchy(allItems, default(TEntity), idProperty, parentIdProperty, rootItemId, 0, 0, string.Empty);
			return result.ToList();

		}

		/// <summary>
		///   LINQ to Objects (IEnumerable) AsHierachy() extension method
		/// </summary>
		/// <typeparam name = "TEntity">Entity class</typeparam>
		/// <typeparam name = "TProperty">Property of entity class</typeparam>
		/// <param name = "allItems">Flat collection of entities</param>
		/// <param name = "idProperty">Func delegete to Id/Key of entity</param>
		/// <param name = "parentIdProperty">Func delegete to parent Id/Key</param>
		/// <param name = "rootItemId">Value of root item Id/Key</param>
		/// <param name = "maxDepth">Maximum depth of tree</param>
		/// <returns>Hierarchical structure of entities</returns>
		public static IEnumerable<Hierarchy<TEntity>> AsHierarchy<TEntity, TProperty>(
			this IEnumerable<TEntity> allItems,
			Func<TEntity, TProperty> idProperty,
			Func<TEntity, TProperty> parentIdProperty,
			object rootItemId,
			int maxDepth) where TEntity : class
		{
			var result = CreateHierarchy(allItems, default(TEntity), idProperty, parentIdProperty, rootItemId, maxDepth, 0, string.Empty);
			return result.ToList();
		}

		public static T FirstOrNull<T>(this IEnumerable<T> list)
		{
			var enumerable = list as IList<T> ?? list.ToList();
			return !enumerable.IsEmpty() ? enumerable.First() : default(T);
		}

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>
			(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			var seenKeys = new HashSet<TKey>();
			return source.Where(element => seenKeys.Add(keySelector(element)));
		}
        public static IEnumerable<dynamic> DistinctBy<TSource, TKey>
        (this IEnumerable<dynamic> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

		public static Boolean IsEmpty<T>(this IEnumerable<T> source)
		{
			if (source == null)
			{
				return true;
			}
			return !source.Any();
		}

		public static void RemoveAll<T>(this IList<T> list, Func<T, bool> condition)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (condition(list[i]))
					list.RemoveAt(i);
			}
		}
	}

	public static class ExceptionExtensions
	{
		public static string ToReadbleSrting(this Exception exception, string message, bool isHtml = true, bool isIncludeStack = true)
		{
			try
			{
				return string.Format("{0} {4} Message: {1} {4} Type: {3}{4} {2}{4}", message,
					exception.GetFullMessage(), isIncludeStack ? "StackTrace:"+exception.StackTrace: string.Empty,
					exception.GetType(),isHtml ? "<br/>" : Environment.NewLine);
			}
			catch
			{
				return string.Empty;
			}
		}


		public static string ToReadbleSrting(this IEnumerable<string> list)
		{
			var result = string.Empty;
			if (list != null)
			{
				result = list.Aggregate(result, (current, item) => string.Format("{0} {1}", current, item)).Trim();
			}
			return result;
		}

		public static string GetFullMessage(this Exception exception)
		{
			return GetFullMessage(exception, "; Inner: ");
		}

		/// <summary>
		///     Concatenates messages form exception and all inner exceptions.
		/// </summary>
		public static string GetFullMessage(this Exception exception, string exceptionDivider)
		{
			var res = exception.Message;
			var innerException = exception.InnerException;
			while (innerException != null)
			{
				res += exceptionDivider;
				res += innerException.Message;
				innerException = innerException.InnerException;
			}
			return res;
		}

		public static string GetStackTrace(this Exception exception)
		{
			var sb = new StringBuilder();
			{
				var trace = new System.Diagnostics.StackTrace(exception);
				var frames = trace.GetFrames();
				if (frames != null)
				{
					var frameLists = frames.ToList();
					foreach (var frame in frameLists)
					{
						sb.AppendLine(string.Format("{0}:{1}{2}<br/>", frame.GetMethod(), frame.GetFileName(), frame.GetFileLineNumber()));
					}
				}
			}
			return sb.ToString();
		}
	}

	public static class Extensions
	{
        public static string ToFirstString(this ExpandoObject value, int start, int end ,string separator = " ")
        {
            if (value == null) return string.Empty;
            var list = (value as IDictionary<string, object>);
            return String.Join(separator, list.Select(item=>item.Value.ToSafeString()));
        }

        public static string ToSeparatedString(this ExpandoObject value, string separator = " ")
        {
            if (value == null) return string.Empty;
            var list = (value as IDictionary<string, object>);
            return String.Join(separator, list.Select(item => item.Value.ToSafeString()));
        }

        public static string FirstValue(this ExpandoObject value)
        {
            if (value == null) return string.Empty;
            var list = (value as IDictionary<string, object>);
            return list.FirstOrDefault().Value.ToSafeString();
        }

        public static string ToNamedString(this ExpandoObject value, int start = 0, string separator = " ")
        {
            if (value == null) return string.Empty;
            var list = (value as IDictionary<string, object>);
            var resultList = new List<string>();
            for (var i = start; i <= list.Count() - 1; i++)
            {
                var item = list.ElementAt(i);
                resultList.Add(string.Format("{0}:{1}", item.Key, item.Value.ToSafeString()));
            }
            return String.Join(separator, resultList);
        }

        public static string ExtractValue(this string value, int position)
        {
            var list = value.ToSafeString().Split('_').ToList();
            return list.Count() == 2 ? list[position] : string.Empty;
        }

        public static string ExtractDataSource(this string value)
        {
            return value.ExtractValue(0);
        }

        public static string ExtractField(this string value)
        {
            return value.ExtractValue(1);
        }

		public static IEnumerable<T> For<T>(this T start, Func<T, bool> condition, Func<T, T> increment)
		{
			for (var i = start; condition(i); i = increment(i))
				yield return i;
		}

		public static T Do<T>(this T t, Action<T> action)
		{
			action(t);
			return t;
		}

		private static readonly Regex GuidRegex = new
			Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$",RegexOptions.Compiled);

        
        private static readonly Regex BooleanRegex = new Regex(@"^(YES|Yes|yes|TRUE|True|true)$", RegexOptions.Compiled);

        public static int ExtractNumber(this string value)
		{
            return Regex.Match(value.ToSafeString(), @"\d+").Value.ToInt();
		}

		public static string ToUniqueNumber(this Guid value)
		{
			return value.ToString("N");
		}

		public static string ToNormalize(this string value)
		{
			return new Regex("[^a-zA-Z0-9]").Replace(value, "");
		}


		public static bool IsGuid(this string value)
		{
			return !string.IsNullOrEmpty(value) && GuidRegex.IsMatch(value);
		}

        public static bool IsDate(this string value)
		{
            try
            {
                DateTime.Parse(value);
                return true;
            }
            catch
            {
                return false;
            }
		}

        public static bool IsInt(this string value)
		{
            try
            {
                Int32.Parse(value);
                return true;
            }
            catch
            {
                return false;
            }
		}
        
		public static Byte[] ToByteArray(this Stream stream)
		{
			stream.Position = 0;
			var buffer = new byte[stream.Length];
			for (var totalBytesCopied = 0; totalBytesCopied < stream.Length;)
			{
				totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
			}
			return buffer;
		}

		public static Stream ToStream(this Byte[] array)
		{
			return new MemoryStream(array);
		}

		public static Byte[] ToByteArray(this Image value)
		{
			return value.ToByteArray(ImageFormat.Gif);
		}

		public static Byte[] ToByteArray(this Image value, ImageFormat imageFormat)
		{
			if (value == null) return null;
			var memoryStream = new MemoryStream();
			value.Save(memoryStream, imageFormat);
			return memoryStream.ToArray();
		}

		public static DateTime? ToNullableDateTime(this object value)
		{
			return value == null ? null : value.ToString().ToNullableDateTime();
		}

		public static DateTime? ToNullableDateTime(this string value)
		{
			DateTime result;
			if (!DateTime.TryParse(value, out result))
			{
				return null;
			}
			return result;
		}

		public static DateTime ToDateTime(this object value, DateTime defaultValue = default(DateTime))
		{
			return value == null ? defaultValue : value.ToString().ToDateTime(defaultValue);
		}

		public static DateTime ToDateTime(this string value, DateTime defaultValue = default(DateTime))
		{
			DateTime result;
			if (!DateTime.TryParse(value, out result))
			{
				result = defaultValue;
			}
			return result;
		}

		public static string ToSafeString(this object value, string defaultvalue = "")
		{
			return (value ?? defaultvalue).ToString();
		}

		public static int? ToNullableInt(this object value)
		{
            return string.IsNullOrWhiteSpace(value.ToSafeString()) ? (int?)null : value.ToSafeString().ToInt();
		}

		public static int ToInt(this object value)
		{
			if (value is Enum)
			{
				var stringValue = Enum.GetName(value.GetType(), value);
				return string.IsNullOrWhiteSpace(stringValue) ? 0 : (int)Enum.Parse(value.GetType(), stringValue);
			}
			return value == null ? 0 : value.ToString().ToInt();
		}

		public static int ToInt(this string value, int defaultvalue = 0)
		{
			int result;
			if (!Int32.TryParse(value, out result))
			{
				result = defaultvalue;
			}
			return result;
		}

		public static double? ToNullableDouble(this object value)
		{
			return value == null ? (double?) null : value.ToString().ToDouble();
		}

		public static double ToDouble(this object value)
		{
			return value == null ? 0.0 : value.ToString().ToDouble();
		}

		public static double ToDouble(this string value)
		{
			double result;
			if (!Double.TryParse(value, out result))
			{
				result = 0.0;
			}
			if (Double.IsNaN(result))
			{
				result = 0.0;
			}
			return result;
		}

		public static bool ToBoolean(this object value)
		{
			return value.ToSafeString().ToBoolean();
		}

		public static bool ToBoolean(this string value)
		{
			return !string.IsNullOrEmpty(value) && (BooleanRegex.IsMatch(value));
		}

		public static Guid ToGuid(this object value)
		{
			return (value is string) ? ((string)value).ToGuid() : Guid.Empty;
		}

		public static Guid ToGuid(this string value)
		{
			return string.IsNullOrEmpty(value)
					   ? Guid.Empty
					   : (GuidRegex.IsMatch(value) ? new Guid(value) : Guid.Empty);
		}

		public static Guid? ToNullableGuid(this object value)
		{
			return value == null ? null : value.ToString().ToNullableGuid();
		}

		public static Guid? ToNullableGuid(this string value)
		{
			return string.IsNullOrEmpty(value)
					   ? null
					   : (GuidRegex.IsMatch(value) ? new Guid(value) : default(Guid?));
		}

	    public static bool ContainsIgnoreCase(this string value, string key)
	    {
	        return value.IndexOf(key, StringComparison.InvariantCultureIgnoreCase) != -1;
	    }

        public static object DefaultValue(this Type t)
        {
            return t.IsValueType && Nullable.GetUnderlyingType(t) == null ? Activator.CreateInstance(t) : null;
        }
	}
}