using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    [DataContract, Serializable]
    public class QueryDisplayParams
    {
        [DataMember]
        public int? pageNum { get; set; }
        [DataMember]
        public int pageSize { get; set; }
        [DataMember]
        public string sortColumn { get; set; }
        [DataMember]
        public string sortDirection { get; set; }

        public int currentPageNum
        {
            get {
                return pageNum.HasValue ? pageNum.Value : 0;
            }
        }
        public int currentPageSize
        {
            get {
                return pageSize != 0 ? pageSize : 10;
            } // 10 - default value
        }

        public int startRow
        {
            get
            {
                int result = 1;

                if (!pageNum.HasValue)
                    pageNum = 1;

                result = (pageNum.Value - 1) * pageSize + 1;
                return result;
            }
        }

        public int endRow
        {
            get
            {
                int result = 1;

                if (pageSize == 0)
                {
                    result = int.MaxValue;
                }
                else
                {
                    result = startRow + pageSize;
                }

                return result;
            }
        }
    }
}
