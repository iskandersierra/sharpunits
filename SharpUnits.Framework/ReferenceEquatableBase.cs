using System;

namespace SharpUnits.Framework
{
    public abstract class ReferenceEquatableBase<T> :
        IEquatable<T>
        where T : class
    {
        public bool Equals(T other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualsOverride(other);
        }

        protected abstract bool EqualsOverride(T other);

        public override bool Equals(object obj)
        {
            return Equals(obj as T);
        }

        public override abstract int GetHashCode();
    }
}