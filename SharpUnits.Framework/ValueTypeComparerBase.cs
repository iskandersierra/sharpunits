using System;
using System.Collections;
using System.Collections.Generic;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework
{
    public abstract class ValueTypeComparerBase<T> :
        ValueTypeEqualityComparerBase<T>,
        IComparer<T>,
        IComparer
        where T : struct
    {
        public abstract int Compare(T x, T y);

        public sealed override bool Equals(T x, T y)
        {
            return Compare(x, y) == 0;
        }

        int IComparer.Compare(object x, object y)
        {
            if (!(x is T) || !(y is T)) 
                throw new InvalidOperationException(string.Format(Resources.CannotCompareObjects, x, y));
            return Compare((T)x, (T)y);
        }
    }
}