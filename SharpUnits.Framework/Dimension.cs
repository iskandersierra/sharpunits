using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework
{
    public class Dimension :
        MeasurementIdentifiedItem,
        IDimensionRuleBase,
        IEquatable<Dimension>, 
        IComparable<Dimension>,
        IComparable
    {
        private static readonly ReadOnlyCollection<DerivationRule> EmptyDerivationRules = new ReadOnlyCollection<DerivationRule>(new DerivationRule[0]);

        internal MeasurementFramework _framework;
        internal ReadOnlyCollection<DerivationRule> _derivationRules;
        internal DerivationRuleKind _kind;
        private DerivationRule _exactRule;
        internal int _order;

        internal Dimension()
        {
        }

        public MeasurementFramework Framework
        {
            get
            {
                return _framework;
            }
        }

        public DerivationRuleKind Kind
        {
            get
            {
                return _kind;
            }
        }

        public int Order
        {
            get { return _order; }
        }

        public bool IsBase
        {
            get
            {
                return Kind == DerivationRuleKind.Basic;
            }
        }

        public bool IsMinimal
        {
            get
            {
                return Kind <= DerivationRuleKind.Minimal;
            }
        }

        public bool IsNormalized
        {
            get
            {
                return Kind <= DerivationRuleKind.Normalized;
            }
        }

        public bool IsExact
        {
            get
            {
                return true;
            }
        }

        public IReadOnlyList<DerivationRule> DerivationRules
        {
            get
            {
                return _derivationRules;
            }
        }

        public DerivationRule ExactDerivationRule
        {
            get
            {
                if (_exactRule == null)
                {
                    _exactRule = new DerivationRule(_framework, new[] {new DerivationRuleComponent(this, 1),});
                }
                return _exactRule;
            }
        }

        public DerivationRule NormalizedDerivationRule
        {
            get
            {
                return ExactDerivationRule;
            }
        }

        public DerivationRule MinimalDerivationRule
        {
            get
            {
                if (IsMinimal) return ExactDerivationRule;
                return DerivationRules[0].MinimalDerivationRule;
            }
        }

        public DerivationRule BasicDerivationRule
        {
            get
            {
                if (IsBase) return ExactDerivationRule;
                return DerivationRules[0].BasicDerivationRule;
            }
        }

        #region [ Comparable and Equatable ]

        public override bool Equals(object obj)
        {
            return Equals(obj as Dimension);
        }

        public bool Equals(Dimension other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (this.Framework != other.Framework) return false;
            return CompareToInternal(other) == 0;
        }

        public override int GetHashCode()
        {
            return Order;
        }

        public int CompareTo(Dimension other)
        {
            if (other == null)
                throw new InvalidOperationException(string.Format(Resources.CannotCompareObjects, this, "null"));
            return CompareToInternal(other);
        }

        public int CompareTo(object obj)
        {
            var dimension = obj as Dimension;
            if (dimension == null)
                throw new InvalidOperationException(string.Format(Resources.CannotCompareObjects, this, obj == null ? "null" : obj.ToString()));
            return CompareToInternal(dimension);
        }

        private int CompareToInternal(Dimension other)
        {
            if (Framework != other.Framework)
                throw new InvalidOperationException(string.Format(Resources.CannotCompareObjects, this, other));
            return Order.CompareTo(other.Order);
        }


        public static bool operator ==(Dimension d1, Dimension d2)
        {
            if (ReferenceEquals(d1, null)) return ReferenceEquals(d2, null);
            return d1.Equals(d2);
        }

        public static bool operator !=(Dimension d1, Dimension d2)
        {
            return !(d1 == d2);
        }

        #endregion

        protected override BracketsFormat DefaultBracketsFormat
        {
            get { return BracketsFormat.SquareBrackets; }
        }

        #region [ Operators ]

        public DerivationRule Power(int exponent)
        {
            var result = new DerivationRule(Framework, new DerivationRuleComponent(this, exponent));
            return result;
        }

        public DerivationRule Multiply(IDimensionRuleBase rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");

            var operand1 = new DerivationRule(Framework, new DerivationRuleComponent(this, 1));

            var result = operand1.Multiply(rule);
            return result;
        }

        public DerivationRule Divide(IDimensionRuleBase rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");

            var operand1 = new DerivationRule(Framework, new DerivationRuleComponent(this, 1));

            var result = operand1.Divide(rule);
            return result;
        }

        public static DerivationRule operator ^(Dimension dimension, int exponent)
        {
            if (dimension == null) throw new ArgumentNullException("dimension");

            var result = dimension.Power(exponent);
            return result;
        }

        public static DerivationRule operator *(Dimension dimension, IDimensionRuleBase rule)
        {
            if (dimension == null) throw new ArgumentNullException("dimension");
            if (rule == null) throw new ArgumentNullException("rule");

            var result = dimension.Multiply(rule);
            return result;
        }

        public static DerivationRule operator /(Dimension dimension, IDimensionRuleBase rule)
        {
            if (dimension == null) throw new ArgumentNullException("dimension");
            if (rule == null) throw new ArgumentNullException("rule");

            var result = dimension.Divide(rule);
            return result;
        }

        #endregion
        
    }
}
