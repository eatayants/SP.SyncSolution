#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

#endregion

namespace SqlSync.Model.Helpers
{
	public static class ContextExtensions
	{
        public static int Next<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
        {
            int? result = source.Max(selector);
            return (result ?? 0) + 1;
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            foreach (var item in values)
            {
                collection.Add(item);
            }
        }

		public static string EntityName(this DbContext context, DbEntityEntry entity)
		{
			var objectContext = ((IObjectContextAdapter)context).ObjectContext;
			var entityType = entity.Entity.GetType();
			if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
			{
				entityType = entityType.BaseType;
			}
			var entityTypeName = entityType.Name;
			var container = objectContext.MetadataWorkspace.GetEntityContainer(objectContext.DefaultContainerName, DataSpace.CSpace);
			var entitySetName = (from meta in container.BaseEntitySets where meta.ElementType.Name == entityTypeName select meta.Name).First();
			return entitySetName;
		}

		public static List<string> MappedTables(this DbContext context)
		{
			var result = new List<string>();
			var objectContext = ((IObjectContextAdapter)context).ObjectContext;
			var metadataWorkspace = objectContext.MetadataWorkspace.GetItems(DataSpace.SSpace);
			result.AddRange(metadataWorkspace.Where(item => item.BuiltInTypeKind == 
                BuiltInTypeKind.EntityType).Select(item => (item as EntityType).Name));
			return result;
		}
        public static string GetTableName<T>(this DbContext context) where T : class
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            return objectContext.GetTableName<T>();
        }

        public static string GetTableName<T>(this ObjectContext context) where T : class
        {
            var sqlStmt = context.CreateObjectSet<T>().ToTraceString();
            var table = new Regex("FROM (?<table>.*) AS").Match(sqlStmt).Groups["table"].Value;
            return table;
        }

		public static IQueryable<TEntity> GetTable<TEntity>(this DbContext context)
		{
			var objectContext = ((IObjectContextAdapter) context).ObjectContext;
			return objectContext.GetTable<TEntity>();
		}

		public static IQueryable<TEntity> GetTable<TEntity>(this ObjectContext context)
		{
			return context.CreateQuery<TEntity>("[" + context.GetEntitySet<TEntity>().Name + "]");
		}

		public static EntitySetBase GetEntitySet<TEntity>(this DbContext context)
		{
			var objectContext = ((IObjectContextAdapter) context).ObjectContext;
			return objectContext.GetEntitySet<TEntity>();
		}

		public static EntitySetBase GetEntitySet<TEntity>(this ObjectContext context)
		{
			var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
			var baseType = typeof (TEntity); /*GetBaseType(typeof (TEntity));*/
			return container.BaseEntitySets.FirstOrDefault(item => item.ElementType.Name.Equals(baseType.Name));
		}

		public static Type GetBaseType(Type type)
		{
			var baseType = type.BaseType;
			if (baseType != null && baseType != typeof (EntityObject))
			{
				return GetBaseType(type.BaseType);
			}
			return type;
		}

        public static Type GetClrType(this SqlDbType sqlType, bool isNullable = false)
        {
            switch (sqlType)
            {
                case SqlDbType.BigInt:
                    return isNullable ? typeof(long?) : typeof(long);

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    return typeof(byte[]);

                case SqlDbType.Bit:
                    return isNullable ? typeof(bool?) : typeof(bool);

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    return typeof(string);

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Date:
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                    return isNullable ? typeof(DateTime?) : typeof(DateTime);

                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return isNullable ? typeof(decimal?) : typeof(decimal);

                case SqlDbType.Float:
                    return isNullable ? typeof(double?) : typeof(double);

                case SqlDbType.Int:
                    return isNullable ? typeof(int?) : typeof(int);

                case SqlDbType.Real:
                    return isNullable ? typeof(float?) : typeof(float);

                case SqlDbType.UniqueIdentifier:
                    return isNullable ? typeof(Guid?) : typeof(Guid);

                case SqlDbType.SmallInt:
                    return isNullable ? typeof(short?) : typeof(short);

                case SqlDbType.TinyInt:
                    return isNullable ? typeof(byte?) : typeof(byte);

                case SqlDbType.Variant:
                case SqlDbType.Udt:
                    return typeof(object);

                case SqlDbType.Structured:
                    return typeof(DataTable);

                case SqlDbType.DateTimeOffset:
                    return isNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);

                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }
	}
}