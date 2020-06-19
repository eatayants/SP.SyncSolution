using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSync.Common.Collections
{
	public interface IContentModel
	{
		string ContentId { get; set; }
		string Keywords { get; set; }
		int PageSize { get; set; }
	}
}
