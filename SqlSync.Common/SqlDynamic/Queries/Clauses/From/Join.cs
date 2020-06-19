﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDynamic.Queries
{
	public class Join
	{
		private readonly JoinType joinType;
		private readonly TabularDataSource source;
		private readonly IBoolean expression;

		public Join(TabularDataSource source, IBoolean expression)
		{
			this.joinType = JoinType.Inner;
			this.source = source;
			this.expression = expression;
		}

		public Join(JoinType joinType, TabularDataSource source, IBoolean expression)
		{
			this.joinType = joinType;
			this.source = source;
			this.expression = expression;
		}

		public TabularDataSource Source
		{
			get { return source; }
		}

		public IBoolean Expression
		{
			get { return expression; }
		}

		public JoinType JoinType
		{
			get { return joinType; }
		}
	}

	public enum JoinType
	{
		Inner, 
		LeftOuter,
		RightOuter,
		FullOuter,
		Cross
	}
}
