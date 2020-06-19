using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDynamic.Queries
{
	public interface IExpression
	{
		Select As(string alias);

		Select AsSelf();

		Function WithFunc(string function);

		IExpression CastTo(string type);
	}
}
