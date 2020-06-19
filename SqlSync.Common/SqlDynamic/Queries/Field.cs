using System;

namespace SqlDynamic.Queries
{
	public class Field : Expression
	{
		private readonly TabularDataSource tabularDataSource;
		private readonly string name;
        private readonly string xmlFunction;

        public Field(TabularDataSource tabularDataSource, string name)
        {
            this.tabularDataSource = tabularDataSource;
            this.name = name;
        }

		public Field(TabularDataSource tabularDataSource, string name,string xmlFunction)
		{
			this.tabularDataSource = tabularDataSource;
			this.name = name;
		    this.xmlFunction = xmlFunction;
		}

		public TabularDataSource TabularDataSource
		{
			get { return tabularDataSource; }
		}

        public string XmlFunction
        {
            get { return xmlFunction; }
        }

		public string Name
		{
			get { return name; }
		}

		public static Field All(TabularDataSource tabularDataSource)
		{
			return new Field(tabularDataSource, "*");
		}

		public static Star All()
		{
			return new Star();
		}

		public static Field Get(string owner, string name)
		{
			return new Field(new TableSource(new Table(owner)), name);
		}

		public static Field Get(string name)
		{
			return new Field(null, name);
		}
	}
}
