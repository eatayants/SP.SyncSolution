using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Presentation.Controls.Interfaces
{
    public interface IStatusFilter
    {
        string StatusFieldName { get; }
        string StatusIDs { get; }
    }
}
