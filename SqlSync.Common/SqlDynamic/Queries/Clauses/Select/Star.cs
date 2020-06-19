using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDynamic.Queries
{
	public class Star : Select
	{
		public Count Count()
		{
			return new Count(this);
		}
	}
}
