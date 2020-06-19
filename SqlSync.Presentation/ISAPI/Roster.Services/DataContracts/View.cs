using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    [DataContract, Serializable]
    public class baseViewData
    {
        public baseViewData()
        {
            this.message = new sysMessage();
        }
        [DataMember]
        public sysMessage message { get; set; }

    }

    [DataContract(Name = "listView{0}"), Serializable]
    public class listView<T> : baseViewData
    {
        public listView()
        {
            
        }

        [DataMember]
        public pager<T> page { get; set; }
        [DataMember]
        public int total { get; set; }
    }

    [DataContract(Name = "itemData{0}"), Serializable]
    public class itemView<T> : baseViewData
    {
        public itemView()
        {

        }
        [DataMember]
        public T item { get; set; }    
    }
}
