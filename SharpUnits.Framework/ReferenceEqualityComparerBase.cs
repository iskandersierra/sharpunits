using System.Collections;
using System.Collections.Generic;

namespace SharpUnits.Framework
{
    public abstract class ReferenceEqualityComparerBase<T> : 
        IEqualityComparer<T>, 
        IEqualityComparer
        where T: class
    {
        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, null)) return ReferenceEquals(y, null);
            if (ReferenceEquals(y, null)) return false;
            if (ReferenceEquals(x, y)) return true;
            return EqualsOverride(x, y);
        }

        public int GetHashCode(T obj)
        {
            if (ReferenceEquals(obj, null)) return 0;
            return GetHashCodeOverride(obj);
        }

        protected abstract bool EqualsOverride(T x, T y);
        protected abstract int GetHashCodeOverride(T obj);

        bool IEqualityComparer.Equals(object x, object y)
        {
            if (!(x is T) || !(y is T)) return false;
            return Equals((T)x, (T)y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            if (!(obj is T)) return 0;
            return GetHashCode((T)obj);
        }
    }
}