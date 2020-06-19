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
using Roster.BL.Facade;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Common.Helpers;
using Roster.Model.DataContext;
using Roster.Model.DataContext.Extention;
using Roster.Model.Helpers;
using SqlDynamic.Queries;

#endregion

namespace Roster.BL.Providers
{
    internal class EventStatusPropertyProvider : ProviderSqlAbstract<EventStatusProperty>
    {
        public EventStatusPropertyProvider()
            : base(string.Empty)
        {
        }
        public EventStatusPropertyProvider(string connectionString)
            : base(connectionString)
        {
        }
		public override List<IExpression> GetDefaultFields()
		{
			var tableDef = GetTableDef();
			var result = new List<IExpression>
			{
				tableDef.TableSource().Field(tableDef.PrimaryField),
				tableDef.TableSource().Field(tableDef.ParentField)
			};
			return result;
		}

        public override EventStatusProperty GetTableDef()
        {
            return new EventStatusProperty();
        }

        public override List<string> SystemFields()
        {
            var tableDef = GetTableDef();
			return new List<string> { tableDef.PrimaryField, tableDef.ParentField };
        }
    }
}