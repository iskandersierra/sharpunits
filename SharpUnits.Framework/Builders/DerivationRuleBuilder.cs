using System;
using System.Collections.Generic;
using System.Linq;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework.Builders
{
    public class DerivationRuleBuilder
    {
        private readonly DimensionBuilder _dimensionBuilder;
        internal readonly List<DerivationRuleComponentBuilder> _componentBuilders;

        internal DerivationRuleBuilder(DimensionBuilder dimensionBuilder)
        {
            if (dimensionBuilder == null) throw new ArgumentNullException("dimensionBuilder");
            _dimensionBuilder = dimensionBuilder;
            _componentBuilders = new List<DerivationRuleComponentBuilder>();
        }

        public DerivationRuleBuilder By(DimensionBuilder dimension, int exponent = 1)
        {
            if (dimension == null) throw new ArgumentNullException("dimension");
            if (dimension._frameworkBuilder != _dimensionBuilder._frameworkBuilder)
                throw new ArgumentException(string.Format("Dimension {0} belongs to a measurement framework different than {1}", dimension, _dimensionBuilder));
            _componentBuilders.Add(new DerivationRuleComponentBuilder(dimension, exponent));
            return this;
        }

        public DerivationRuleBuilder Per(DimensionBuilder dimension, int exponent = 1)
        {
            return By(dimension, -exponent);
        }

        internal DerivationRule BuildRule(FrameworkCreationContext context)
        {
            var components = _componentBuilders.Select(e => e.BuildComponent(context)).ToList();
            var rule = new DerivationRule(context.Framework, components);
            return rule;
        }
    }

    public class DerivationRuleComponentBuilder
    {
        internal readonly DimensionBuilder _dimension;
        internal readonly int _exponent;

        internal DerivationRuleComponentBuilder(DimensionBuilder dimension, int exponent)
        {
            if (dimension == null) throw new ArgumentNullException("dimension");
            _dimension = dimension;
            _exponent = exponent;
        }

        internal DerivationRuleComponent BuildComponent(FrameworkCreationContext context)
        {
            Dimension dimension;
            if (!context.Dimensions.TryGetValue(_dimension._name, out dimension))
                throw new ArgumentException(string.Format(Resources.CannotFindDimension, _dimension._name));
            return new DerivationRuleComponent(dimension, _exponent);
        }
    }
}