﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDynamic.Queries
{
	public class MustHaveAtLeastOneException : Exception
	{
		public MustHaveAtLeastOneException(string message)
			: base(message)
		{
		}
	}
}
