//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SqlSync.Model.DataContext
{
    using System;
    
    public partial class SqlServerAgent_JobList_Result
    {
        public System.Guid JobID { get; set; }
        public string JobName { get; set; }
        public string JobCategory { get; set; }
        public string JobDescription { get; set; }
        public string IsEnabled { get; set; }
        public System.DateTime JobCreatedOn { get; set; }
        public System.DateTime JobLastModifiedOn { get; set; }
        public string OriginatingServerName { get; set; }
        public Nullable<int> JobStartStepNo { get; set; }
        public string JobStartStepName { get; set; }
        public string IsScheduled { get; set; }
        public Nullable<System.Guid> JobScheduleID { get; set; }
        public string JobScheduleName { get; set; }
        public string JobDeletionCriterion { get; set; }
        public Nullable<System.DateTime> LastRunDateTime { get; set; }
        public string LastRunStatus { get; set; }
        public string LastRunDuration__HH_MM_SS_ { get; set; }
        public string LastRunStatusMessage { get; set; }
        public Nullable<System.DateTime> NextRunDateTime { get; set; }
    }
}
