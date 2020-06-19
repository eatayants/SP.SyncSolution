using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDynamic.Queries
{
	public class TableSource : TabularDataSource
	{
		private readonly Table table;

		public TableSource(Table table)
			: base(null)
		{
			this.table = table;
		}

		public TableSource(Table table, string alias)
			: base(alias)
		{
			this.table = table;
		}

		public Table Table
		{
			get { return table; }
		}

		public new Field Field(string name)
		{
			return new Field(this, name);
		}

        public Field Field(string name, string xmlFunction)
        {
            return new Field(this, name, xmlFunction);
        }

		public Field All()
		{
			return Queries.Field.All(this);
		}
	}
}
