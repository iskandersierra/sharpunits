using System;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework
{
    public abstract class ReferenceComparableBase<T> :
        ReferenceEquatableBase<T>,
        IComparable<T>,
        IComparable
        where T : class
    {
        public int CompareTo(T other)
        {
            if (ReferenceEquals(other, null))
                throw new InvalidOperationException(string.Format(Resources.CannotCompareObjects, this, null));
            return CompareOverride(other);
        }

        protected sealed override bool EqualsOverride(T other)
        {
            var result = CompareOverride(other) == 0;
            return result;
        }

        protected abstract int CompareOverride(T other);

        int IComparable.CompareTo(object obj)
        {
            if (!(obj is T))
                throw new InvalidOperationException(string.Format(Resources.CannotCompareObjects, this, obj));
            return CompareTo((T) obj);
        }
    }
}