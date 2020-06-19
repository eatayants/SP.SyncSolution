using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDynamic.Queries
{
	public class BitwiseAnd : IBoolean
	{
        private readonly IExpression bitmask;
        private readonly IExpression expression;

        public BitwiseAnd(IExpression bitmask, IExpression expression)
		{
            this.bitmask = bitmask;
            this.expression = expression;
        }

        public IExpression Bitmask
		{
            get { return bitmask; }
		}

        public IExpression Expression
        {
            get { return expression; }
        }
	}
}
