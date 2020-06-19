#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.SharePoint;
using SqlSync.BL.Facade;
using SqlSync.Common;
using SqlSync.Common.Collections;
using SqlSync.Common.Helpers;
using SqlSync.Model.DataContext;
using SqlSync.Model.Helpers;

#endregion

namespace SqlSync.BL.Providers
{
	public abstract class ProviderAbstract<TEntity, TDataContext,TKeyType> :
        ProviderBase, IEntityBaseProvider<TEntity, TKeyType>, IDisposable
		where TEntity : class, new()
		where TDataContext : DbContext, new()
        where TKeyType : IComparable
	{
		#region Abstract Methods

	    protected ProviderAbstract(string connectionString)
            : base(connectionString)
	    {
	    }

		public abstract Expression<Func<TEntity, bool>> CompareById(TKeyType id);
		public abstract Expression<Func<TEntity, bool>> CompareByIds(IEnumerable<TKeyType> ids);
		public abstract Expression<Func<TEntity, bool>> CompareByValue(TEntity entity);
        public abstract TKeyType GetEntityId(TEntity entity);

		#endregion

		public DbContext GetContext()
		{
			return CreateObjectContext();
		}

		public virtual string GetEntityDisplayName(TEntity entity)
		{
			return string.Empty;
		}

		#region Service Members

		private TDataContext _dataContextInstance;
		private bool _isDisposed;

        public TDataContext GetObjectContext()
		{
            var context = CreateObjectContext() as TDataContext;
			//context.Configuration.LazyLoadingEnabled = false;
			//context.Configuration.AutoDetectChangesEnabled = false;
			//context.Configuration.ProxyCreationEnabled = true;
			return context;
		}
        #endregion
        
        #region IDisposable

        public void Dispose()
		{
			if (_dataContextInstance != null && !_isDisposed)
			{
				_dataContextInstance.Dispose();
				_dataContextInstance = null;
				_isDisposed = true;
			}
		}

		#endregion

		#region IEntityBase<TEntity> Members

		public virtual TKeyType Add(TEntity entity)
		{
			TKeyType result;
			var context = GetObjectContext();
			{
				context.Set<TEntity>().Add(entity);
				AddInternal(context, entity);
				context.SaveChanges();
				result = GetEntityId(entity);
			}
			return result;
		}

		public virtual void Update(TEntity entity)
		{
			var db = GetObjectContext();
			{
				Update(db, entity);
			}
		}

		public virtual void Delete(TEntity entity)
		{
			var context = GetObjectContext();
			{
				Delete(context, entity);
				context.SaveChanges();
			}
		}

		public virtual void Delete(TKeyType id)
		{
			var db = GetObjectContext();
			{
				Delete(db, id);
			}
			db.SaveChanges();
		}

		public bool IsExist(TKeyType id)
		{
			bool res;
			var db = GetObjectContext();
			{
				var query =
					from entities
						in db.GetTable<TEntity>().Where<TEntity>(CompareById(id))
					select entities;

				res = query.Any();
			}
			return res;
		}

		public bool IsExist(TEntity entity)
		{
			bool res;
			var db = GetObjectContext();
			{
				var query =
					from entities
						in db.GetTable<TEntity>().Where<TEntity>(CompareByValue(entity))
					select entities;

				res = query.Any();
			}
			return res;
		}

		public PagerList<TEntity> List(SelectCriteria<TEntity> criteria)
		{
			PagerList<TEntity> res = null;
			if (criteria.PageIndex <= 0)
			{
				criteria.PageIndex = 1;
			}
			var db = GetObjectContext();
			{
				InitDataContext(db);
				var query = GetBaseListSelectQuery(db);

				if (query is ObjectQuery<TEntity>)
				{
					(query as ObjectQuery<TEntity>).MergeOption = MergeOption.NoTracking;
				}
				//Apply entity select
				if ((criteria.ExpressionSelect != null) && (criteria.ExpressionSelect.Any()))
				{
					query = criteria.ExpressionSelect.Aggregate(query,
					        (current, expressionSelect) => current.Where(expressionSelect));
				}
				var applyPaging = criteria.PageSize > 0;
				//Apply sorting
				if ((criteria.ExpressionSort != null) && (criteria.ExpressionSort.Any()))
				{
					IOrderedQueryable<TEntity> sortedQuery = null;
					foreach (var expressionSort in criteria.ExpressionSort)
					{
						if (sortedQuery == null)
						{
							sortedQuery = criteria.Sort == SortDirection.Ascending ? query.OrderBy(expressionSort) : query.OrderByDescending(expressionSort);
						}
						else
						{
							sortedQuery = criteria.Sort == SortDirection.Ascending ? sortedQuery.ThenBy(expressionSort) : sortedQuery.ThenByDescending(expressionSort);
						}
					}
					query = sortedQuery;
				}
				else
				{
					if (applyPaging)
					{
						query = query.OrderBy(e => 1);
					}
				}
                // apply distinct
                if (query != null && criteria.IsDistinct)
                {
                    query = query.Distinct();
                }

				if (query != null)
				{
					var totalCount = query.Count();
					if (applyPaging)
					{
						query = query.Skip((criteria.PageIndex-1)*criteria.PageSize).Take(criteria.PageSize);
					}
					res = new PagerList<TEntity>(query.ToList(), totalCount, criteria.PageIndex, criteria.PageSize);
				}
			}
			return res;
		}

		public int TotalCount(SelectCriteria<TEntity> criteria)
		{
			int result;
			if (criteria.PageIndex <= 0)
			{
				criteria.PageIndex = 1;
			}
			var db = GetObjectContext();
			{
				InitDataContext(db);
				var query = from entities in db.GetTable<TEntity>() select entities;
				if ((criteria.ExpressionSelect != null) && (criteria.ExpressionSelect.Any()))
				{
					query = criteria.ExpressionSelect.Aggregate(query, (current, expressionSelect) => current.Where(expressionSelect));
				}
				result = query.Count();
			}
			return result;
		}

		public virtual List<TEntity> List()
		{
			return List(-1, -1);
		}

		public List<TEntity> List(IEnumerable<TKeyType> ids)
		{
			List<TEntity> entity;
			var db = GetObjectContext();
			{
				var query =
					from entities
						in db.GetTable<TEntity>().Where<TEntity>(CompareByIds(ids))
					select entities;

				entity = query.ToList();
			}
			return entity;
		}

		public virtual List<TEntity> GetByIds(List<TKeyType> ids)
		{
			List<TEntity> entities;
			var db = GetObjectContext();
			{
				ids = ids ?? new List<TKeyType>();
				var query = from dbEntities in db.GetTable<TEntity>()
								.Where<TEntity>(CompareByIds(ids)) select dbEntities;
				entities = query.ToList();
			}
			return entities;
		}
		
		public virtual TEntity GetById(TKeyType id)
		{
			TEntity entity;
			var db = GetObjectContext();
			{
				var query =
					from entities
						in db.GetTable<TEntity>().Where<TEntity>(CompareById(id))
					select entities;

				entity = query.FirstOrDefault();
			}
			return entity;
		}

		public virtual PagerList<TEntity> List(int pageIndex, int pageSize)
		{
			var criteria = DefaultSelectCriteria();
			if (criteria == null)
			{
				criteria = new SelectCriteria<TEntity>
					{
						PageIndex = pageIndex,
						PageSize = pageSize
					};
			}
			else
			{
				criteria.PageSize = pageSize;
				criteria.PageIndex = pageIndex;
			}
			return List(criteria);
		}

		public virtual TKeyType Add(TDataContext context, TEntity entity)
		{
			context.Set<TEntity>().Add(entity);
			AddInternal(context, entity);
		    context.SaveChanges();
			return GetEntityId(entity);
		}

	    public virtual void Update(TDataContext db, TEntity entity)
		{
			var attachedEntity = (from entities in db.GetTable<TEntity>().
			                                          Where<TEntity>(CompareById(GetEntityId(entity)))
			                      select entities).FirstOrDefault();
			if (attachedEntity == null) return;
			var context = ((IObjectContextAdapter) db).ObjectContext;
			var objSet = context.CreateObjectSet<TEntity>();
			var entityKey = context.CreateEntityKey(objSet.EntitySet.Name, entity);
			object originalItem;
			OnBeforeUpdateInternal(db, entity, attachedEntity);
			if (context.TryGetObjectByKey(entityKey, out originalItem))
			{
				context.ApplyCurrentValues(entityKey.EntitySetName, entity);
			}
			OnUpdateInternal(db, entity, attachedEntity);

			db.SaveChanges();
		}

		public virtual void Delete(TDataContext db, TEntity entity)
		{
			PreDeleteInternal(db, GetEntityId(entity), entity);
			db.Set<TEntity>().Remove(entity);
			OnDeleteComplete(db, GetEntityId(entity), entity);
		}

		public virtual void Delete(TDataContext db, TKeyType id)
		{
			var entity = (from entities in 
				db.GetTable<TEntity>().Where<TEntity>(CompareById(id))
				select entities).FirstOrDefault();
			if (entity == null) return;
			PreDeleteInternal(db, id, entity);
			db.Set<TEntity>().Remove(entity);
			OnDeleteComplete(db, id, entity);
		}

		protected virtual IQueryable<TEntity> GetBaseListSelectQuery(TDataContext db)
		{
			return (from entities in db.GetTable<TEntity>() select entities);
		}

		public int TotalCount(int startIndex, int count)
		{
			int result;
			var db = GetObjectContext();
			{
				result = db.GetTable<TEntity>().Count();
			}
			return result;
		}

		#endregion IEntityBase<TEntity> Members

		#region Internal family Memebers

		public virtual SelectCriteria<TEntity> DefaultSelectCriteria()
		{
			return null;
		}

		protected virtual DataLoadOptions GetDataLoadOptions()
		{
			return null;
		}

		protected virtual void InitDataContext(TDataContext db)
		{
		}

		protected virtual void AddInternal(TDataContext db, TEntity entity)
		{
		}

		protected virtual void OnAddComplete(TEntity entity)
		{
			EntityInfo(EntityOperations.Add, entity);
		}

		protected virtual void OnBeforeUpdateInternal(TDataContext db, TEntity newEntity, TEntity oldEntity)
		{
		}

		protected virtual void OnUpdateInternal(TDataContext db, TEntity newEntity, TEntity oldEntity)
		{
		}

		protected virtual void OnUpdateComplete(TEntity entity)
		{
			EntityInfo(EntityOperations.Update, entity);
		}

		protected virtual void PreDeleteInternal(TDataContext db, TKeyType id, TEntity entity)
		{
		}

		protected virtual void OnDeleteComplete(TDataContext db, TKeyType id, TEntity entity)
		{
			EntityInfo(EntityOperations.Delete, entity);
		}

		protected virtual void GetByIdInternal(TDataContext db, TKeyType id, TEntity entity)
		{
			EntityInfo(EntityOperations.Get, entity);
		}

		protected virtual void TrimEntityFields(TEntity entity)
		{
		}

		private void EntityInfo(string operationName, TEntity entity)
		{
			try
			{
				LogHelper.Instance.Info("{0},{1},{2}", operationName, entity.GetType().Name, GetEntityId(entity));
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Error("Logging Entity Info", ex);
			}
		}

		#endregion
	}
}