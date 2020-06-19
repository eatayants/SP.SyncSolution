using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SqlDynamic.Queries
{
	public class JoinList : ExpressionList<Join>
	{
		private static readonly JoinList empty = new JoinList();
		public static JoinList Empty
		{
			get { return empty; }
		}

		public JoinList()
			: base()
		{
		}

		public JoinList(Join expression)
			: base(expression)
		{
		}

		private JoinList(JoinList list, Join expression)
			: base(list, expression)
		{
		}

		public JoinList Add(Join join)
		{
			return new JoinList(this, join);
		}
	}
}
