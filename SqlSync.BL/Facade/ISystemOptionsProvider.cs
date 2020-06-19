#region

using System;
using System.Collections.Generic;
using SqlSync.Model.DataContext;

#endregion

namespace SqlSync.BL.Facade
{
	public interface ISystemOptionProvider : IEntityBaseProvider<SystemOption,String>
	{
		string DatabaseVersion { get; set; }
        List<string> ListTables();
        List<string> ListFields(string table);
        void SaveDataSource(string datasource, string statements);
        string ReadDataSource(string datasource);
    }
}