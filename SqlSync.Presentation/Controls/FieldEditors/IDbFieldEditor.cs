using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldEditors
{
    public interface IDbFieldEditor
    {
        DbField GetField(string fieldName);
    }
}
