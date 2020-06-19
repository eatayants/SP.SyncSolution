using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    [DataContract, Serializable]
    public class pager
    {
        public pager()
        {

        }
        public int pageCount
        {
            get
            {
                if (pageSize == 0)
                {
                    return 1;                  
                }
                return (int)Math.Ceiling((decimal)(this.totalRowCount / this.pageSize));
            }
        }
        [DataMember]
        public int pageNum { get; set; }

        [DataMember]
        public int pageSize { get; set; }

        [DataMember]
        public string sortString { get; set; }

        [DataMember]
        public int totalRowCount { get; set; }
    }

    [DataContract(Name = "pagerOf{0}"), Serializable]
    public class pager<T> : pager
    {
        public pager()
        {
            pageItems = new List<T>();
        }
        [DataMember]
        public IList<T> pageItems { get; set; }
    }

    public static class pagerHelper
    {
        public static pager<TTarget> GetPage<TTarget>(IQueryable<TTarget> data, int? pageNum, int pageSize, bool safe, string sortString)
        {
            pager<TTarget> result = new pager<TTarget>();

            var q = data;

            if (!pageNum.HasValue)
                pageNum = 1;

            if (string.IsNullOrEmpty(sortString.Trim()))
            {
                sortString = string.Format("{0} {1}", "Id", "asc");
            }

            result.sortString = sortString;

            var sortParts = result.sortString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < sortParts.Length; i++)
            {
                var sortPart = sortParts[i];
                var part = sortPart.Split(' ');

                /*if (i == 0)
                    q = q.OrderBy(part[0], part[1] == "asc");
                else
                    q = q.ThenBy(part[0], part[1] == "asc");*/
            }


            if (pageNum == 0 || pageSize == 0)
            {
                throw new NotImplementedException();
                /*
                result.pageItems = q.ToList();
                result.totalRowCount = result.pageItems.Count;
                result.pageNum = 0;
                result.pageSize = 0;
                 */
            }
            else
            {
                if (safe)
                {
                    result.totalRowCount = q.ToList().Count;
                }
                else
                {
                    result.totalRowCount = q.Count();
                }

                result.pageItems = q.Skip((pageNum.Value - 1) * pageSize).Take(pageSize).ToList();
                result.pageNum = pageNum.Value;
                result.pageSize = pageSize;
            }

            return result;
        }

        public static pager<TTarget> GetPage<TTarget>(this IQueryable<TTarget> query, QueryDisplayParams settings)
        {
            if (settings.pageSize == 0)
            {
                settings.pageSize = 10;
            }
            return GetPage(query, settings.pageNum, settings.pageSize, false, string.Format("{0} {1}", settings.sortColumn, settings.sortDirection));
        }
    }
}
