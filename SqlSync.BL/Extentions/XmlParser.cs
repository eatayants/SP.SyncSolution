using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Roster.BL.Extentions
{
    public class DocParser
    {
        private XmlDocument Document { get; set; }
        public DocParser(string fp)
        {
            try
            {
                Document = new XmlDocument();
                Document.LoadXml(fp);
            }
            catch (Exception ex)
            {
                throw new Exception("Error with document load or doc type.", ex);
            }
        }

        public ExpandoObject GetElements(string regex)
        {
            return XmlToExpandoObject(regex);
        }

        private ExpandoObject XmlToExpandoObject(string regex)
        {
            var result = new ExpandoObject();
            var dictionary = result as IDictionary<string, object>;
            var xnl = Document.DocumentElement.SelectNodes(regex);
            if (xnl == null) return result;
            foreach (XmlNode xn in xnl) {
                dictionary[xn.Name] = xn.InnerXml.Trim();
            }
            return result;
        }
    }
}
