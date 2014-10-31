using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework.Builders
{
    public class DimensionBuilder : 
        MeasurementIdentifiedItemBuilder<DimensionBuilder>
    {
        internal readonly MeasurementFrameworkBuilder _frameworkBuilder;
        private bool _isMinimal;
        private readonly List<DerivationRuleBuilder> _derivationRuleBuilders;
        private static readonly DerivationRule[] EmptyDerivationRuleArray = new DerivationRule[0];

        internal DimensionBuilder(MeasurementFrameworkBuilder frameworkBuilder, string name)
            : base(name)
        {
            if (frameworkBuilder == null) throw new ArgumentNullException("frameworkBuilder");
            _frameworkBuilder = frameworkBuilder;
            _derivationRuleBuilders = new List<DerivationRuleBuilder>();
        }

        protected MeasurementFrameworkBuilder FrameworkBuilder
        {
            get { return _frameworkBuilder; }
        }

        protected override IEnumerable<DimensionBuilder> GetIdentifierSpace()
        {
            return _frameworkBuilder.dimensionsDict.Values;
        }

        internal Dimension BuildDimension(FrameworkCreationContext context)
        {
            var dimension = new Dimension();
            dimension._framework = context.Framework;
            dimension._order = context.NextDimensionOrder++;

            context.NextDimensionId = InitIdentifiedItem(dimension, context.DimensionIdentifiers, context.NextDimensionId);

            if (_isMinimal)
            {
                dimension._kind = DerivationRuleKind.Minimal;
                if (_derivationRuleBuilders.Count == 0)
                    throw new ArgumentException(string.Format(Resources.MinimalDimensionMustHaveRule, _name));
            }
            else if (_derivationRuleBuilders.Count == 0)
                dimension._kind = DerivationRuleKind.Basic;
            else
                dimension._kind = DerivationRuleKind.Normalized;

            if (_derivationRuleBuilders.Count == 0)
                dimension._derivationRules = new ReadOnlyCollection<DerivationRule>(EmptyDerivationRuleArray);
            else
            {
                var rules = _derivationRuleBuilders.Select(b => b.BuildRule(context)).ToList();
                if (rules.Count > 1)
                {
                    // all rules must have the same minimal rule
                    var minimalRules = rules.Select(r => r.MinimalDerivationRule).ToList();
                    for (int i = 1; i < minimalRules.Count; i++)
                    {
                        if (!minimalRules[i].Equals(minimalRules[0]))
                            throw new ArgumentException(string.Format("Inconsistent rules found in dimension {0}: {1} and {2}", _name, rules[0], rules[i]));
                    }
                }
                dimension._derivationRules = new ReadOnlyCollection<DerivationRule>(rules);
            }

            return dimension;
        }

        public DimensionBuilder IsMinimal()
        {
            _isMinimal = true;
            return this;
        }

        public DimensionBuilder WithDerivationRule(Action<DerivationRuleBuilder> ruleActions)
        {
            if (ruleActions == null) throw new ArgumentNullException("ruleActions");
            var builder = new DerivationRuleBuilder(this);
            ruleActions(builder);
            _derivationRuleBuilders.Add(builder);
            return this;
        }

        public DimensionBuilder WithEmptyDerivationRule()
        {
            return WithDerivationRule(r => { });
        }
    }
}