using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDynamic.Queries
{
	public class GreaterThan : Operator
	{
		public GreaterThan(IExpression expressionLeft, IExpression expressionRight)
			: base (expressionLeft, expressionRight)
		{
		}

		public override string OperatorSymbol
		{
			get { return ">"; }
		}
	}
}
