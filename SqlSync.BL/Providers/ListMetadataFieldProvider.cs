#region

using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using Microsoft.SharePoint.ApplicationPages.Calendar.Exchange;
using Roster.BL.Facade;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Model.DataContext;

#endregion

namespace Roster.BL.Providers
{
    internal class ListMetadataFieldProvider : ProviderAbstract<ListMetadataField, RosterEntities,Guid>, IListMetadataFieldProvider
	{
        public ListMetadataFieldProvider()
            : base(string.Empty)
        {
        }
        public ListMetadataFieldProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<ListMetadataField, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<ListMetadataField, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(ListMetadataField entity)
		{
			return entity.Id;
		}

        public override Expression<Func<ListMetadataField, bool>> CompareByValue(ListMetadataField entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}
        protected override void PreDeleteInternal(RosterEntities db, Guid id, ListMetadataField entity)
        {
            db.ViewMetadataFields.RemoveRange(db.ViewMetadataFields.Where(item => item.ListMetadataFieldId == id));
            db.ViewMetadataPopupSettings.RemoveRange(db.ViewMetadataPopupSettings.Where(item => item.ListMetadataFieldId == id));
            db.ViewMetadataWhereCriteries.RemoveRange(db.ViewMetadataWhereCriteries.Where(item => item.ListMetadataFieldId == id));
            db.ListMetadataFieldContentTypes.RemoveRange(db.ListMetadataFieldContentTypes.Where(item => item.ListMetadataFieldId == id));
        }
        public string GetFieldRefrerence(Guid id)
        {
            //todo
            return string.Empty;
        }
    }
}