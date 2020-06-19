using System;
using System.Data;
using SqlDynamic.Queries;

namespace SqlDynamic.Providers
{
	public interface IProvider
	{
		IDbCommand GetCommand(BaseQuery query);

		string GetSqlString(BaseQuery query);
	}
}
