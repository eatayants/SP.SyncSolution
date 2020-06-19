using System;
using System.Collections.Generic;

namespace Roster.Common.Collections
{
    [Serializable]
    public struct MinMax<T> : IEquatable<MinMax<T>>
    {
        public readonly T Min;
        public readonly T Max;

        public MinMax(T min, T max)
        {
            this.Min = min;
            this.Max = max;
        }

        public override string ToString()
        {
            return string.Format("[Min = {0}, Max = {1}]",Min, Max);
        }

        public bool Equals(MinMax<T> value)
        {
            return
                EqualityComparer<T>.Default.Equals(Min, value.Min) &&
                EqualityComparer<T>.Default.Equals(Max, value.Max);
        }

        public override bool Equals(object obj)
        {
            return obj is MinMax<T> && Equals((MinMax<T>)obj);
        }

        public override int GetHashCode()
        {
            int num = -722197669;
            num = (-1521134295 * num) + EqualityComparer<T>.Default.GetHashCode(this.Min);
            return (-1521134295 * num) + EqualityComparer<T>.Default.GetHashCode(this.Max);
        }
    }
}
