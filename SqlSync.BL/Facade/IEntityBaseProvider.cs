#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using Roster.Common;
using Roster.Common.Collections;

#endregion

namespace Roster.BL.Facade
{
	public interface IEntityBaseProvider<TEntity,TKeyType>
	{
		DbContext GetContext();
        TKeyType GetEntityId(TEntity entity);
		string GetEntityDisplayName(TEntity entity);
        TKeyType Add(TEntity entity);
		void Update(TEntity entity);
        void Delete(TKeyType id);
		void Delete(TEntity entity);
        TEntity GetById(TKeyType id);
        List<TEntity> GetByIds(List<TKeyType> ids);
        bool IsExist(TKeyType id);
		bool IsExist(TEntity entity);
		List<TEntity> List();
        List<TEntity> List(IEnumerable<TKeyType> ids);
		PagerList<TEntity> List(int pageIndex, int pageSize);
		PagerList<TEntity> List(SelectCriteria<TEntity> criteria);
		int TotalCount(SelectCriteria<TEntity> criteria);
	}
}