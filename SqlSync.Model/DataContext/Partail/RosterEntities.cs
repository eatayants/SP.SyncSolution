using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Roster.Model.DataContext
{
    public partial class RosterEntities
    {
        public RosterEntities(string connectionString)
            : base(connectionString)
        {
        }
    }
}

