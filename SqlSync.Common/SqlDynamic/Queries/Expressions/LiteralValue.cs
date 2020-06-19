using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDynamic.Queries
{
	public class LiteralValue : Expression
	{
		private readonly object value;

		public LiteralValue(object value)
		{
			this.value = value;
		}

		public object Value
		{
			get { return value; }
		}
	}
}
