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
    internal class ProviderSqlDynamic: ProviderSqlAbstract<TableDynamic>
    {
        private readonly TableDynamic _table;

        public ProviderSqlDynamic(TableDynamic table, string connectionString)
            : base(connectionString)
        {
            _table = table;
        }

        public override List<IExpression> GetDefaultFields()
		{
			var tableDef = GetTableDef();
			var result = new List<IExpression>
			{
				tableDef.TableSource().Field(tableDef.PrimaryField)
			};
			return result;
		}

        public override TableDynamic GetTableDef()
        {
            return _table;
        }

        public override List<string> SystemFields()
        {
            var tableDef = GetTableDef();
			return new List<string> { tableDef.PrimaryField };
        }
    }
}