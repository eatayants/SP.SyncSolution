using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDynamic.Queries
{
	public class Take
	{
		private readonly int rows;

		public Take()
		{
		}

		public Take(int rows)
		{
			this.rows = rows;
		}

		public int Rows
		{
			get { return rows; }
		}
	}
}
