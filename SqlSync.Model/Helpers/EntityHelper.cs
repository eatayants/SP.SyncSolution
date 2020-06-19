#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Linq.Mapping;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;
using SqlSync.Common;
using SqlSync.Model.DataContext;

#endregion

namespace SqlSync.Model.Helpers
{
	public class CollectionMerger
	{
		private readonly DbContext _context;
		public CollectionMerger(DbContext context)
		{
			_context = context;
		}

        public void Merge<TEntity>(TEntity source, TEntity distanation,IEqualityComparer<TEntity> comparer) where TEntity : class
       {
           if ((source == null) || (distanation == null)) return;
            if (!comparer.Equals(source, distanation)) return;
            _context.Entry(distanation).CurrentValues.SetValues(source);
            _context.Entry(distanation).State = EntityState.Modified;
       }

		public void Merge<TEntity>(ICollection<TEntity> source, ICollection<TEntity> distanation, 
			IEqualityComparer<TEntity> comparer, bool updateExisting = false) where TEntity : class
		{
			if (source.Any())
			{
				var newItems = source.ToList();
				var existingItems = distanation.ToList();
				if (updateExisting)
				{
					foreach (var existingItem in existingItems.Intersect(newItems, comparer))
					{
						var newItem = newItems.FirstOrDefault(item => comparer.Equals(item, existingItem));
						if (newItem == null) continue;
						_context.Entry(existingItem).CurrentValues.SetValues(newItem);
						_context.Entry(existingItem).State = EntityState.Modified;
					}
				}
				foreach (var item in newItems.Except(existingItems, comparer))
				{
					_context.Set<TEntity>().Add(item);
				}
				foreach (var item in existingItems.Where(item => !newItems.Contains(item, comparer)))
				{
					_context.Set<TEntity>().Remove(item);
				}
			}
			else
			{
				var items = distanation.ToList();
				foreach (var item in items)
				{
					_context.Set<TEntity>().Remove(item);
				}
			}

		}

		public IEnumerable<TEntity> Deleted<TEntity>(ICollection<TEntity> source, ICollection<TEntity> distanation, 
			IEqualityComparer<TEntity> comparer)
		{
			if (source.Any())
			{
				var newItems = source.ToList();
				var existingItems = distanation.ToList();
				return existingItems.Where(item => !newItems.Contains(item, comparer));
			}
			else
			{
				return distanation.ToList();
			}
		}
	}

	public class EntityHelper
	{
		/// <summary>
		///     Copies column values from source to dest except PrimaryKey
		/// </summary>
		public static void CopyColumns(object source, object destanation)
		{
			var fields = destanation.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var field in fields.Where(field => field.FieldType.Name.Contains("EntityRef")))
			{
				field.SetValue(destanation, null);
			}

			foreach (var property in destanation.GetType().GetProperties())
			{
				if (!property.CanWrite)
				{
					continue;
				}
				var attr = (ColumnAttribute) Attribute.GetCustomAttribute(property, typeof (ColumnAttribute));
				if (attr != null)
				{
					if (attr.IsPrimaryKey)
					{
						continue;
					}
				}
				var orignProperty = destanation.GetType().GetProperty(property.Name);
				if (orignProperty.GetValue(destanation, null) != property.GetValue(source, null))
				{
					orignProperty.SetValue(destanation, property.GetValue(source, null), null);
				}
			}
		}

		//TODO Implement method for display entity value name 
		public static string GetDisplayName(object value)
		{
			if (value == null)
			{
				return null;
			}
			else
			{
				return value.ToString();
			}
		}

		/// <summary>
		///     Generates new guid value for PrimaryKey attribute
		/// </summary>
		public static void ChangeEntityGuidId(object entity)
		{
			ChangeEntityGuidId(entity, Guid.NewGuid());
		}

		/// <summary>
		///     Sets new guid value for PrimaryKey attribute
		/// </summary>
		public static void ChangeEntityGuidId(object entity, Guid newId)
		{
			if (entity == null)
				return;

			foreach (var property in entity.GetType().GetProperties())
			{
				var attr = (ColumnAttribute) Attribute.GetCustomAttribute(property, typeof (ColumnAttribute));

				if (attr == null || !attr.IsPrimaryKey) continue;
				if (property.PropertyType == typeof (Guid))
				{
					property.SetValue(entity, newId, null);
				}

				break;
			}
		}

		public static Guid GetEntityGuidId(object entity)
		{
			if (entity == null)
				return Guid.Empty;

			foreach (var property in entity.GetType().GetProperties())
			{
				var attr = (ColumnAttribute) Attribute.GetCustomAttribute(property, typeof (ColumnAttribute));

				if (attr == null || !attr.IsPrimaryKey) continue;
				if (property.PropertyType == typeof (Guid))
				{
					return (Guid) property.GetValue(entity, null);
				}

				break;
			}

			return Guid.Empty;
		}
	}
}