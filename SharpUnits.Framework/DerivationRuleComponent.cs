using System;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework
{
    public class DerivationRuleComponent :
        ReferenceComparableBase<DerivationRuleComponent>,
        IFormattable
    {
        private readonly Dimension _dimension;
        private readonly int _exponent;

        public DerivationRuleComponent(Dimension dimension, int exponent)
        {
            if (dimension == null) throw new ArgumentNullException("dimension");
            _dimension = dimension;
            _exponent = exponent;
        }

        public Dimension Dimension
        {
            get { return _dimension; }
        }

        public int Exponent
        {
            get { return _exponent; }
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var exponent = Exponent;
            if (!string.IsNullOrEmpty(format) && format[0] == '-')
            {
                exponent = -exponent;
                format = format.Substring(1);
            }
            var dimensionText = Dimension.ToString(format, formatProvider);
            if (exponent == 1)
                return dimensionText;
            return string.Format(Resources.ValueToThePowerOf, dimensionText, exponent);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Dimension.GetHashCode() * 397 ^ _exponent;
                return hashCode;
            }
        }

        protected override int CompareOverride(DerivationRuleComponent other)
        {
            var result = Exponent.CompareTo(other.Exponent);
            if (result != 0)
                return result;

            result = Dimension.CompareTo(other.Dimension);
            return result;
        }
    }
}