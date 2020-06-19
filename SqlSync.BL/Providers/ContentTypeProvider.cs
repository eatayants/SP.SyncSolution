#region

using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using Roster.BL.Facade;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Model.DataContext;
using Roster.Model.Helpers;

#endregion

namespace Roster.BL.Providers
{
	internal class ContentTypeProvider : ProviderAbstract<ListMetadataContentType, RosterEntities, Int32>, IListMetadataContentTypeProvider
	{
        public ContentTypeProvider()
            : base(string.Empty)
        {
        }

        public ContentTypeProvider(string connectionString)
            : base(connectionString)
        {
        }

        public override Expression<Func<ListMetadataContentType, bool>> CompareByIds(IEnumerable<Int32> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<ListMetadataContentType, bool>> CompareById(Int32 id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Int32 GetEntityId(ListMetadataContentType entity)
		{
			return entity.Id;
		}

        public override Expression<Func<ListMetadataContentType, bool>> CompareByValue(ListMetadataContentType entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}

        protected override void PreDeleteInternal(RosterEntities db, int id, ListMetadataContentType entity)
        {
            db.ListMetadataActions.RemoveRange(db.ListMetadataActions.Where(item => item.ContentTypeId == id));
        }

        protected override void OnUpdateInternal(RosterEntities db, 
            ListMetadataContentType newEntity, ListMetadataContentType oldEntity)
        {
            if ((newEntity == null) || (oldEntity == null)) return;
            var merger = new CollectionMerger(db);
            {
                merger.Merge(newEntity.ListMetadataFieldContentTypes, oldEntity.ListMetadataFieldContentTypes, new ListMetadataFieldContentTypeComparer(), true);
                merger.Merge(newEntity.ListMetadataActions, oldEntity.ListMetadataActions, new ListMetadataActionComparer(), true);
            }
        }
        #region Comparers

        public class ListMetadataFieldContentTypeComparer : IEqualityComparer<ListMetadataFieldContentType>
        {
            public bool Equals(ListMetadataFieldContentType first, ListMetadataFieldContentType second)
            {
                return first.ContentTypeId == second.ContentTypeId && 
                    first.ListMetadataFieldId == second.ListMetadataFieldId;
            }

            public int GetHashCode(ListMetadataFieldContentType entity)
            {
                return entity.ContentTypeId.GetHashCode() * entity.ListMetadataFieldId.GetHashCode();
            }
        }

        public class ListMetadataActionComparer : IEqualityComparer<ListMetadataAction>
        {
            public bool Equals(ListMetadataAction first, ListMetadataAction second)
            {
                return first.Id == second.Id;
            }

            public int GetHashCode(ListMetadataAction entity)
            {
                return entity.Id.GetHashCode();
            }
        }

        #endregion

	    public List<ListMetadataContentType> GetContentTypes(Guid listId)
	    {
            var db = GetObjectContext();
	        {
	            return db.ListMetadataContentTypes.Where(item => 
                    item.ListMetadataId == listId).OrderBy(item=>item.Name).ToList();

	        }
	    }

        public ListMetadataContentType Create(Guid listId)
	    {
            var db = GetObjectContext();
	        {
                var listMetadata = db.ListMetadatas.FirstOrDefault(i => i.Id == listId);
                return new ListMetadataContentType
                {
                    Id = db.ListMetadataContentTypes.Next(item=>item.Id),
                    ListMetadataId = listId,
                    NewItemUrl = listMetadata == null ? string.Empty:listMetadata.NewItemUrl,
                    EditItemUrl = listMetadata == null ? string.Empty : listMetadata.EditItemUrl,
                    DispItemUrl = listMetadata == null ? string.Empty:listMetadata.DispItemUrl,
                };
            }
	    }
	}
}