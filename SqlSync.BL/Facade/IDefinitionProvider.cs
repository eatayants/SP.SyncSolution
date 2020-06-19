#region

using System;
using System.Collections.Generic;
using System.Dynamic;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Model.DataContext;

#endregion

namespace Roster.BL.Facade
{
	public interface IDefinitionProvider : IEntityBaseProvider<Definition, Guid>
	{
		Dictionary<string, string> ListBoolean();
		List<Definition> List(DefTypes defTypes);
		DefType GetByType(DefTypes currentLookup);
		List<DefType> GetByUserTypes();
	    Definition GetDefinityionIdByName(string name, DefTypes type);
        List<Tuple<int, ExpandoObject>> ListTableContent(string table, string key, string field, List<Tuple<string, string>> whereCriteria = null);
		List<string> ListTables();
		List<string> ListFields(string table);
        void SaveDataSource(string datasource, string statements);
	    string ReadDataSource(string datasource);
	}
}