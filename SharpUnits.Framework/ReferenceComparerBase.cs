using System;
using System.Collections;
using System.Collections.Generic;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework
{
    public abstract class ReferenceComparerBase<T> :
        ReferenceEqualityComparerBase<T>,
        IComparer<T>,
        IComparer 
        where T : class
    {
        public int Compare(T x, T y)
        {
            if (ReferenceEquals(x, null))
                throw new InvalidOperationException(string.Format(Resources.CannotCompareObjects, x, "null"));
            if (ReferenceEquals(y, null))
                throw new InvalidOperationException(string.Format(Resources.CannotCompareObjects, null, y));
            if (ReferenceEquals(x, y))
                return 0;
            return CompareOverride(x, y);
        }

        protected abstract int CompareOverride(T x, T y);

        protected sealed override bool EqualsOverride(T x, T y)
        {
            return CompareOverride(x, y) == 0;
        }

        int IComparer.Compare(object x, object y)
        {
            if (!(x is T) || !(y is T)) 
                throw new InvalidOperationException(string.Format(Resources.CannotCompareObjects, x, y));
            return Compare((T)x, (T)y);
        }
    }
}