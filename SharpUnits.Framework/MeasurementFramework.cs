using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using SharpUnits.Framework.Builders;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework
{
    public class MeasurementFramework : 
        MeasurementLocalizableItem
    {
        internal ReadOnlyCollection<Dimension> _dimensions;
        internal Dictionary<string, Dimension> _dimensionsByName;
        internal Dictionary<int, Dimension> _dimensionsByIdentifier;
        internal Dictionary<string, Dictionary<string, Dimension>> _dimensionsBySymbol;
        internal Dictionary<DerivationRule, List<Dimension>> _dimensionsByExactRule;
        internal Dictionary<DerivationRule, List<Dimension>> _dimensionsByNormalizedRule;
        internal Dictionary<DerivationRule, List<Dimension>> _dimensionsByMinimalRule;
        internal Dictionary<DerivationRule, List<Dimension>> _dimensionsByBasicRule;

        internal MeasurementFramework()
        {
        }

        #region [ Build ]
        public static MeasurementFramework Build(string name, Action<MeasurementFrameworkBuilder> builderActions)
        {
            if (builderActions == null) throw new ArgumentNullException("builderActions");

            var builder = new MeasurementFrameworkBuilder(name);
            builderActions(builder);
            var framework = builder.CreateFramework();

            return framework;
        }
        #endregion [ Build ]

        #region [ Dimensions ]
        public IReadOnlyList<Dimension> Dimensions
        {
            get
            {
                return _dimensions;
            }
        }

        public Dimension GetDimensionByName(string dimensionName)
        {
             if (string.IsNullOrWhiteSpace(dimensionName)) throw new ArgumentNullException("dimensionName");
           Dimension dimension;
            if (TryGetDimensionByName(dimensionName, out dimension))
                return dimension;
            throw new ArgumentException(string.Format(Resources.ElementNameNotFound, dimensionName));
        }

        public bool TryGetDimensionByName(string dimensionName, out Dimension dimension)
        {
            if (string.IsNullOrWhiteSpace(dimensionName)) throw new ArgumentNullException("dimensionName");
            var result = _dimensionsByName.TryGetValue(dimensionName, out dimension);
            return result;
        }

        public Dimension GetDimensionByIdentifier(int dimensionIdentifier)
        {
           Dimension dimension;
            if (TryGetDimensionByIdentifier(dimensionIdentifier, out dimension))
                return dimension;
            throw new ArgumentException(string.Format(Resources.ElementIdentifierNotFound, dimensionIdentifier));
        }

        public bool TryGetDimensionByIdentifier(int dimensionIdentifier, out Dimension dimension)
        {
            return _dimensionsByIdentifier.TryGetValue(dimensionIdentifier, out dimension);
        }

        public Dimension GetDimensionBySymbol(string symbol, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentNullException("symbol");
            Dimension dimension;
            if (TryGetDimensionBySymbol(symbol, culture, out dimension))
                return dimension;
            throw new ArgumentException(string.Format("Element symbol {0} not found for culture {1}", symbol, (culture ?? CultureInfo.CurrentCulture).Name));
        }

        public Dimension GetDimensionBySymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentNullException("symbol");
            var result = GetDimensionBySymbol(symbol, CultureInfo.CurrentCulture);
            return result;
        }

        public bool TryGetDimensionBySymbol(string symbol, CultureInfo culture, out Dimension dimension)
        {
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentNullException("symbol");
            culture = culture ?? CultureInfo.CurrentCulture;
            Dictionary<string, Dimension> dictionary;
            if (!_dimensionsBySymbol.TryGetValue(culture.Name, out dictionary))
            {
                dimension = null;
                return false;
            }
            var result = dictionary.TryGetValue(symbol, out dimension);
            return result;
        }

        public bool TryGetDimensionBySymbol(string symbol, out Dimension dimension)
        {
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentNullException("symbol");
            var result = TryGetDimensionBySymbol(symbol, CultureInfo.CurrentCulture, out dimension);
            return result;
        }

        public Dimension ParseDimension(string text, CultureInfo culture = null)
        {
            if (text == null) throw new ArgumentNullException("text");

            Dimension dimension;
            if (TryParseDimension(text, culture, out dimension))
                return dimension;
            throw new FormatException(string.Format("There is no element which can be parsed from {0}", text));
        }

        public bool TryParseDimension(string text, out Dimension dimension)
        {
            if (text == null) throw new ArgumentNullException("text");

            return TryParseDimension(text, null, out dimension);
        }

        public bool TryParseDimension(string text, CultureInfo culture, out Dimension dimension)
        {
            if (text == null) throw new ArgumentNullException("text");

            if (TryGetDimensionBySymbol(text, culture, out dimension))
                return true;
            if (TryGetDimensionByName(text, out dimension))
                return true;
            return false;
        }

        #endregion [ Dimensions ]

        #region [ Internal ]
        //internal DerivationRule GetNormalizedRule(DerivationRule derivationRule)
        //{
        //    var normalizedComponents = derivationRule.Components.OrderBy(e => e);
        //    var result = new DerivationRule(this, normalizedComponents);
        //    return result;
        //}

        //internal DerivationRule GetMinimalRule(DerivationRule derivationRule)
        //{
        //    var minimalComponents = GetMinimalRuleAux(derivationRule);
        //    minimalComponents = minimalComponents
        //        .GroupBy(e => e.Dimension)
        //        .Select(g => new DerivationRuleComponent(g.Key, g.Sum(c => c.Exponent)))
        //        .OrderBy(c => c);
        //    var result = new DerivationRule(this, minimalComponents);
        //    return result;
        //}

        //private IEnumerable<DerivationRuleComponent> GetMinimalRuleAux(DerivationRule derivationRule)
        //{
        //    if (derivationRule.Components.Count == 0) return derivationRule.Components;
        //    if (derivationRule.Components.Count == 1 && derivationRule.Components[0].Dimension.IsMinimal)
        //        return derivationRule.Components;
        //    var allComponents = derivationRule.Components
        //        .SelectMany(c => c.Dimension.MinimalDerivationRule.Components
        //            .Select(e => c.Exponent == 1 ? e : new DerivationRuleComponent(e.Dimension, e.Exponent*c.Exponent))).ToArray();
        //    return allComponents;
        //}

        //internal DerivationRule GetBasicRule(DerivationRule derivationRule)
        //{
        //    var minimalComponents = GetBasicRuleAux(derivationRule);
        //    minimalComponents = minimalComponents
        //        .GroupBy(e => e.Dimension)
        //        .Select(g => new DerivationRuleComponent(g.Key, g.Sum(c => c.Exponent)))
        //        .OrderBy(c => c);
        //    var result = new DerivationRule(this, minimalComponents);
        //    return result;
        //}

        //private IEnumerable<DerivationRuleComponent> GetBasicRuleAux(DerivationRule derivationRule)
        //{
        //    if (derivationRule.Components.Count == 0) return derivationRule.Components;
        //    if (derivationRule.Components.Count == 1 && derivationRule.Components[0].Dimension.IsBase) 
        //        return derivationRule.Components;
        //    var allComponents = derivationRule.Components
        //        .SelectMany(c => c.Dimension.BasicDerivationRule.Components
        //            .Select(e => c.Exponent == 1 ? e : new DerivationRuleComponent(e.Dimension, e.Exponent * c.Exponent))).ToArray();
        //    return allComponents;
        //}
        #endregion [ Internal ]

        public bool FindDimension(DerivationRule rule, out Dimension dimension)
        {
            throw new NotImplementedException();
        }
    }
}