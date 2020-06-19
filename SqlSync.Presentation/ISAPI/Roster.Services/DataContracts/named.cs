using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    [Serializable, DataContract]
    public class named
    {
        private string _key = string.Empty;
        public named()
        {
        }
        public named(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
        [DataMember(Name="k")]
        public string key {
            get { 
                return _key.Trim();
            }
            set {
                _key = value;
            } 
        }
        [DataMember(Name="v")]
        public string value { get; set; }
    }
}
