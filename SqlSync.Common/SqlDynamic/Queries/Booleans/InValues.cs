﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDynamic.Queries
{
	public class InValues : In
	{
		private readonly IEnumerable<object> values;

		public InValues(IExpression expression, IEnumerable<object> values)
			: base(expression)
		{
			if (!values.Any()) {
				throw new MustHaveAtLeastOneException("No values where provided to \"in\".");
			}

			this.values = values;
		}

		public IEnumerable<object> Values
		{
			get { return values; }
		}
	}
}
