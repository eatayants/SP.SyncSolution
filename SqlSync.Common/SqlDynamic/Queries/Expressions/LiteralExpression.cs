using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDynamic.Queries
{
	public class LiteralExpression : Expression
	{
		private readonly string expression;

		public LiteralExpression(string expression)
		{
			this.expression = expression;
		}

		public string Expression
		{
			get { return expression; }
		}
	}
}
