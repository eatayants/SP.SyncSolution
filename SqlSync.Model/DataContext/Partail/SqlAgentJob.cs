using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Model.DataContext.Partail
{
    public class SqlAgentJob
    {
        public Guid Id { get; set; }
        public string JobName { get; set; }
        public string JobDescription { get; set; }
        public string IsEnabled { get; set; }
        public string IsScheduled { get; set; }
        public string JobScheduleName { get; set; }    
        public DateTime? LastRunDateTime { get; set; }                                                                                                                     
        public DateTime? NextRunDateTime { get; set; } 
        public string LastRunStatus { get; set; }  
        public string LastRunDuration { get; set; }
        public string LastRunStatusMessage { get; set; }   
    }
}
