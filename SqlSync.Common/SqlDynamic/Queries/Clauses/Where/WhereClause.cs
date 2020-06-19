﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDynamic.Queries
{
	public class WhereClause : Clause<Conjunction>
	{
		public WhereClause()
			: base(new Conjunction(true))
		{
		}

		public WhereClause(Conjunction list)
			: base(list)
		{
		}

		public override string ClauseName
		{
			get { return "where"; }
		}

	}
}
