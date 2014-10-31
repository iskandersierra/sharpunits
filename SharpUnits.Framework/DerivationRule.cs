using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework
{
    public class DerivationRule : 
        ReferenceComparableBase<DerivationRule>,
        IDimensionRuleBase,
        IEnumerable<DerivationRuleComponent>,
        IEnumerable,
    IFormattable
    {
        internal MeasurementFramework _framework;
        internal IReadOnlyList<DerivationRuleComponent> _components;
        internal DerivationRule _normalizedRule;
        internal DerivationRule _minimalRule;
        internal DerivationRule _basicRule;
        internal DerivationRuleKind? _kind;

        //public static DerivationRule EmptyRule = new DerivationRule(Enumerable.Empty<DerivationRuleComponent>());

        public DerivationRule(
            MeasurementFramework framework,
            params DerivationRuleComponent[] components)
            : this(framework, components as IEnumerable<DerivationRuleComponent>)
        {
        }

        public DerivationRule(
            MeasurementFramework framework,
            IEnumerable<DerivationRuleComponent> components
        )
        {
            if (framework == null) throw new ArgumentNullException("framework");
            if (components == null) throw new ArgumentNullException("components");

            _framework = framework;
            _components = new ReadOnlyCollection<DerivationRuleComponent>(components.ToList());
            if (_components.Any(e => e == null))
                throw new ArgumentNullException("components");
            if (_components.Any(e => e.Dimension.Framework != framework))
                throw new ArgumentException("components");
        }

        public IReadOnlyList<DerivationRuleComponent> Components
        {
            get
            {
                return _components;
            }
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
                if (!_kind.HasValue)
                {
                    if (ReferenceEquals(BasicDerivationRule, this))
                        _kind = DerivationRuleKind.Basic;
                    else if (ReferenceEquals(MinimalDerivationRule, this))
                        _kind = DerivationRuleKind.Minimal;
                    else if (ReferenceEquals(NormalizedDerivationRule, this))
                        _kind = DerivationRuleKind.Normalized;
                    else 
                        _kind = DerivationRuleKind.Exact;
                    

                }
                return _kind.Value;
            }
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

        public DerivationRule ExactDerivationRule
        {
            get
            {
                return this;
            }
        }

        public DerivationRule NormalizedDerivationRule
        {
            get
            {
                if (_normalizedRule == null)
                {
                    var result = new DerivationRule(_framework, Components.OrderBy(e => e));
                    if (result.Equals(this))
                        _normalizedRule = this;
                    else
                        _normalizedRule = result;
                }
                return _normalizedRule;
            }
        }

        public DerivationRule MinimalDerivationRule
        {
            get
            {
                if (_minimalRule == null)
                {
                    var minimalComponents = GetMinimalComponents(this)
                        .GroupBy(e => e.Dimension)
                        .Select(g => g.Count() == 1 ? g.First() : new DerivationRuleComponent(g.Key, g.Sum(c => c.Exponent)))
                        .Where(c => c.Exponent != 0);
                    var result = new DerivationRule(_framework, minimalComponents);
                    if (result.Equals(this))
                        _minimalRule = this;
                    else
                        _minimalRule = result;
                }
                return _minimalRule;
            }
        }

        private static IEnumerable<DerivationRuleComponent> GetMinimalComponents(DerivationRule derivationRule)
        {
            foreach (var component in derivationRule)
            {
                if (component.Exponent == 0) continue;
                foreach (var minimalComponent in component.Dimension.MinimalDerivationRule)
                {
                    var result = minimalComponent;
                    if (component.Exponent != 1)
                        result = new DerivationRuleComponent(result.Dimension, result.Exponent * component.Exponent);
                    yield return result;
                }
            }
        }

        public DerivationRule BasicDerivationRule
        {
            get
            {
                if (_basicRule == null)
                {
                    var basicComponents = GetBasicComponents(this)
                        .GroupBy(e => e.Dimension)
                        .Select(g => g.Count() == 1 ? g.First() : new DerivationRuleComponent(g.Key, g.Sum(c => c.Exponent)))
                        .Where(c => c.Exponent != 0);
                    var result = new DerivationRule(_framework, basicComponents);
                    if (result.Equals(this))
                        _basicRule = this;
                    else
                        _basicRule = result;
                }
                return _basicRule;
            }
        }

        private static IEnumerable<DerivationRuleComponent> GetBasicComponents(DerivationRule derivationRule)
        {
            foreach (var component in derivationRule)
            {
                if (component.Exponent == 0) continue;
                foreach (var minimalComponent in component.Dimension.BasicDerivationRule)
                {
                    var result = minimalComponent;
                    if (component.Exponent != 1)
                        result = new DerivationRuleComponent(result.Dimension, result.Exponent * component.Exponent);
                    yield return result;
                }
            }
        }

        protected override int CompareOverride(DerivationRule other)
        {
            int pos = 0;
            while (true)
            {
                if (pos >= Components.Count)
                {
                    if (pos >= other.Components.Count)
                    {
                        // in this case all components are the same
                        return 0;
                    }
                    // in this case the second rule is the same as the first one but with extra components at the end
                    var component = other.Components[pos];
                    if (component.Exponent != 0)
                    {
                        // if the first extra component have positive exponent the the first rule is lesser than the second
                        // if the first extra component have negative exponent the the first rule is greater than the second
                        return -component.Exponent;
                    }
                    // if the first extra component have zero exponent the the first rule is greater than the second
                    return 1;
                }
                if (pos >= other.Components.Count)
                {
                    // in this case the first rule is the same as the second one but with extra components at the end
                    var component = Components[pos];
                    if (component.Exponent != 0)
                    {
                        // if the first extra component have positive exponent the the first rule is greater than the second
                        // if the first extra component have negative exponent the the first rule is lesser than the second
                        return component.Exponent;
                    }
                    // if the first extra component have zero exponent the the first rule is lesser than the second
                    return -1;
                }
                var component1 = Components[pos];
                var component2 = other.Components[pos];
                var result = component1.CompareTo(component2);
                // if next two components are different then the lesser gives the lesser rule
                if (result != 0) return result;
                // otherwise keep traversing components until a difference is found;
                pos++;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var component in Components)
                {
                    hash = hash*397 ^ component.GetHashCode();
                }
                return hash;
            }
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var numerator = string
                .Join(Resources.MultiplicationSign, 
                    Components.Where(c => c.Exponent >= 0)
                        .Select(c => c.ToString(format, formatProvider)));
            var denominator = string
                .Join(Resources.MultiplicationSign, 
                    Components.Where(c => c.Exponent < 0)
                        .Select(c => c.ToString("-" + format, formatProvider)));
            if (numerator.Length == 0)
                if (denominator.Length == 0)
                    return "[1]";
                else
                    return string.Format(Resources.DivisionOperation, "[1]", denominator);
            else
                if (denominator.Length == 0)
                    return numerator;
                else
                    return string.Format(Resources.DivisionOperation, numerator, denominator);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public IEnumerator<DerivationRuleComponent> GetEnumerator()
        {
            return Components.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region [ Operators ]

        public DerivationRule Power(int exponent)
        {
            if (exponent == 1) return this;

            var components = Components.Select(c => new DerivationRuleComponent(c.Dimension, c.Exponent * exponent));
            var result = new DerivationRule(Framework, components);

            return result;
        }

        public DerivationRule Multiply(IDimensionRuleBase rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");

            var components = Components.Concat(rule.ExactDerivationRule.Components);
            var result = new DerivationRule(Framework, components);
            return result;
        }

        public DerivationRule Divide(IDimensionRuleBase rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");

            var operand2 = rule.ExactDerivationRule.Power(-1);
            var result = this.Multiply(operand2);
            return result;
        }

        public static DerivationRule operator ^(DerivationRule rule, int exponent)
        {
            if (rule == null) throw new ArgumentNullException("rule");

            var result = rule.Power(exponent);
            return result;
        }

        public static DerivationRule operator *(DerivationRule rule, IDimensionRuleBase other)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            if (other == null) throw new ArgumentNullException("other");

            var result = rule.Multiply(other);
            return result;
        }

        public static DerivationRule operator /(DerivationRule rule, IDimensionRuleBase other)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            if (other == null) throw new ArgumentNullException("other");

            var result = rule.Divide(other);
            return result;
        }

        #endregion
    }
}