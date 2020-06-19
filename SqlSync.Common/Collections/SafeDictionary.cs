#region

using System.Collections.Generic;

#endregion

namespace SqlSync.Common.Collections
{
	public class SafeDictionary<TKey, TValue>
		: Dictionary<TKey, TValue>
	{
		public new TValue this[TKey key]
		{
			get
			{
				if (base.ContainsKey(key))
					return base[key];
				return default(TValue);
			}
			set
			{
				if (base.ContainsKey(key))
					base[key] = value;
				else
					base.Add(key, value);
			}
		}
	}
}