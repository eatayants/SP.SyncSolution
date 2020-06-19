using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
	[DataContract]
	[Serializable]
	public class GeneralErrorFault
	{
		[DataMember]
		public string Message { get; set; }
		[DataMember]
		public string Details { get; set; }
	}
}
