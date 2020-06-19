﻿#region

using System;
using System.Collections.Generic;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Model.DataContext;

#endregion

namespace Roster.BL.Facade
{
    public interface IListMetadataContentTypeProvider : IEntityBaseProvider<ListMetadataContentType, Int32>
    {
        List<ListMetadataContentType> GetContentTypes(Guid listId);
        ListMetadataContentType Create(Guid listId);
    }
}