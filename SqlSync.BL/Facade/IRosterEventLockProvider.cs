﻿#region

using System;
using System.Collections.Generic;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Model.DataContext;

#endregion

namespace Roster.BL.Facade
{
    public interface IRosterEventLockProvider : IEntityBaseProvider<RosterEventLock, Guid>
    {
        void Save(int workerId, DateTime startDate, DateTime endDate);
        bool IsExist(int workerId, DateTime startDate, DateTime endDate);
    }
}