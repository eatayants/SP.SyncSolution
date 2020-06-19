using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDynamic.Queries
{
	public class Function : Expression
	{
		private readonly string function;
		private readonly IExpression expression;

		public Function(string function, IExpression expression)
		{
			this.function = function;
			this.expression = expression;
		}

		public virtual string FunctionName {
			get { return function; }
		}

		public IExpression Expression
		{
			get { return expression; }
		}
	}
}
