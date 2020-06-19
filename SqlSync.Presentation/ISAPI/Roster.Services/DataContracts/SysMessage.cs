using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    [DataContract, Serializable]
    public class sysMessage : ISysMessage
    {
        public sysMessage()
        {
            message = string.Empty;
        }
        [DataMember]
        public messageLevelEnum messageLevel { get; set; }
        [DataMember]
        public string message { get; set; }
    }
}
