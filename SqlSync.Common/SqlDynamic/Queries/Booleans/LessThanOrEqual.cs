using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDynamic.Queries
{
	public class GreaterThanOrEqual : Operator
	{
		public GreaterThanOrEqual(IExpression expressionLeft, IExpression expressionRight)
			: base (expressionLeft, expressionRight)
		{
		}

		public override string OperatorSymbol
		{
			get { return ">="; }
		}
	}
}
