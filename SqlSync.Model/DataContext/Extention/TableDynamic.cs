using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDynamic.Queries;

namespace SqlSync.Model.DataContext.Extention
{
    public class TableDynamic
    {
        public string Name { get; set; }
        public string PrimaryField { get; set; }
        public string ParentField { get; set; }

        public string ParentName { get; set; }
        public string ParentPrimariField { get; set; }

        public virtual Field GetPrimaryField()
        {
            return GetTable().Field(PrimaryField);
        }
        public virtual Field GetParentField()
        {
            return GetTable().Field(ParentField);
        }
        public virtual Field GetParentPrimariField()
        {
			return GetParentTable().Field(ParentPrimariField);
        }

        public virtual TableSource ParentTableSource()
        {
            return new TableSource(GetParentTable());
        }

        public virtual Table GetParentTable()
        {
            return new Table(ParentName);
        }

        public virtual Table GetTable()
        {
            return new Table(Name);
        }

        public virtual TableSource TableSource()
        {
            return new TableSource(GetTable());
        }

        public virtual Query GetQuery()
        {
            return Query.From(GetTable()).SelectAll();
        }
		public virtual Query GetQueryDef()
		{
			return Query.From(GetTable());
		}

        public virtual Query GetCount()
        {
            return Query.From(GetTable()).Select(Field.All().Count());
        }
    }
}
