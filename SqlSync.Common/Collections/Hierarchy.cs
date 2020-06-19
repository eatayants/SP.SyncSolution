using System.Collections.Generic;

namespace Roster.Common.Collections
{
	public class Hierarchy<T> where T : class
	{
		public T Entity { get; set; }
		public T Parent { get; set; }
		public IEnumerable<Hierarchy<T>> Items { get; set; }
		public int Depth { get; set; }
		public string Path { get; set; }
        public string Order { get; set; }
	}
}
