﻿using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roster.BL;
using Roster.Common.Helpers;
using Roster.Presentation.Extensions;

namespace Roster.Presentation.EventReceivers
{
    public class SyncDataReceiver : SPItemEventReceiver
    {
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);
            try
            {
                var configProvider = new RosterConfigService(properties.Web);
                var mappingSetting = configProvider.GetMappingByList(properties.ListId.ToString());
                if (mappingSetting == null) return;
                var dataProvider = new RosterDataService(properties.Web);
                {
                    dataProvider.SaveRow(mappingSetting, properties.ListItem.ToSyncObject(mappingSetting));
                }
            }
            catch(Exception ex)
            {
                LogHelper.Instance.ErrorULS("ItemAdded error", ex);
            }
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
            try
            {
                var configProvider = new RosterConfigService(properties.Web);
                var mappingSetting = configProvider.GetMappingByList(properties.ListId.ToString());
                if (mappingSetting == null) return;
                var dataProvider = new RosterDataService(properties.Web);
                {
                    dataProvider.SaveRow(mappingSetting, properties.ListItem.ToSyncObject(mappingSetting));
                }
            }
            catch(Exception ex)
            {
                LogHelper.Instance.ErrorULS("ItemUpdated error", ex);
            }
        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            base.ItemDeleting(properties);
            try
            {
                var configProvider = new RosterConfigService(properties.Web);
                var mappingSetting = configProvider.GetMappingByList(properties.ListId.ToString());
                if (mappingSetting == null) return;
                var dataProvider = new RosterDataService(properties.Web);
                {
                    dataProvider.Delete(mappingSetting.TableName, mappingSetting.Key, properties.ListItemId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.ErrorULS("ItemDeleting error", ex);
            }
        }
    }
}
