using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSync.Common.Collections
{
	public class ContentModel : IContentModel
	{
		public string ContentId { get; set; }
		public string Keywords { get; set; }
		public int PageSize { get; set; }
	}
}
