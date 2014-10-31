using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework.Builders
{
    public class MeasurementFrameworkBuilder :
        MeasurementLocalizableItemBuilder<MeasurementFrameworkBuilder>
    {
        #region [ Internals ]
        internal List<DimensionBuilder> dimensionsList;
        internal Dictionary<string, DimensionBuilder> dimensionsDict;
        internal MeasurementFrameworkBuilder(string name)
            : base(name)
        {
            dimensionsList = new List<DimensionBuilder>();
            dimensionsDict = new Dictionary<string, DimensionBuilder>();
        }

        internal MeasurementFramework CreateFramework()
        {
            var context = new FrameworkCreationContext()
            {
                FrameworkBuilder = this,
                Framework = new MeasurementFramework(),
                NextDimensionId = 1,
                NextDimensionOrder = 1,
                Dimensions = new Dictionary<string, Dimension>(),
            };

            InitFramework(context);

            return context.Framework;
        }

        private void InitFramework(FrameworkCreationContext context)
        {

            InitLocalizableItem(context.Framework);

            context.DimensionIdentifiers = new HashSet<int>(
                dimensionsList
                    .Where(e => e.Identifier != null)
                    .Select(e => e.Identifier.Value));

            foreach (var dimensionBuilder in dimensionsList)
            {
                var dimension = dimensionBuilder.BuildDimension(context);

                context.Dimensions.Add(dimension.Name, dimension);
                context.DimensionIdentifiers.Add(dimension.Identifier);
            }

            context.Framework._dimensions = new ReadOnlyCollection<Dimension>(context.Dimensions.Values.ToList());
            context.Framework._dimensionsByName = context.Framework._dimensions.ToDictionary(e => e.Name);
            context.Framework._dimensionsByIdentifier = context.Framework._dimensions.ToDictionary(e => e.Identifier);

            InitFrameworkBySymbols(context);
            InitFrameworkByRules(context);
        }

        private void InitFrameworkBySymbols(FrameworkCreationContext context)
        {
            var allSymbols = context.Dimensions.SelectMany(
                    p1 => p1.Value._symbols.Select(
                            p2 => new { Symbol = p2.Value, CultureName = p2.Key, Dimension = p1.Value }))
                            .ToList();

            // Find duplicates of [culture, symbol]
            var duplicateSymbols = allSymbols
                .Where(e => !string.IsNullOrWhiteSpace(e.Symbol))
                .GroupBy(e => Tuple.Create(e.CultureName, e.Symbol))
                .Where(g => g.Skip(1).Any())
                .OrderBy(e => e.Key.Item1)
                .ThenBy(e => e.Key.Item2)
                .ToList();
            if (duplicateSymbols.Any())
            {
                var sb = new StringBuilder();
                if (duplicateSymbols.Count > 1)
                    sb.AppendLine();
                foreach (var duplicateSymbol in duplicateSymbols)
                {
                    if (duplicateSymbols.Count > 1)
                        sb.Append("\t");
                    sb.AppendFormat("[{0}] {1} -> {2}",
                            duplicateSymbol.Key.Item1,
                            duplicateSymbol.Key.Item2,
                            string.Join(", ", duplicateSymbol.Select(e => e.Dimension.Name)));
                    if (duplicateSymbols.Count > 1)
                        sb.AppendLine();
                    throw new InvalidOperationException(string.Format(Resources.DuplicateSymbolsFound, sb));
                }
            }

            context.Framework._dimensionsBySymbol = allSymbols
                .GroupBy(e => e.CultureName)
                .ToDictionary(e => e.Key,
                    e => e.ToDictionary(a => a.Symbol, a => a.Dimension));
        }

        private void InitFrameworkByRules(FrameworkCreationContext context)
        {
            var rulesAndDimensions = context.Framework.Dimensions
                .SelectMany(
                    d => d.DerivationRules
                        .Select(r => new { Dimension = d, Rule = r })
                        .Concat(new[] { new { Dimension = d, Rule = d.ExactDerivationRule } })
                )
                .ToList();

            context.Framework._dimensionsByExactRule = rulesAndDimensions
                .GroupBy(e => e.Rule.ExactDerivationRule)
                .ToDictionary(e => e.Key, e => e.Select(o => o.Dimension).Distinct().OrderBy(d => d).ToList());

            context.Framework._dimensionsByNormalizedRule = rulesAndDimensions
                .GroupBy(e => e.Rule.NormalizedDerivationRule)
                .ToDictionary(e => e.Key, e => e.Select(o => o.Dimension).Distinct().OrderBy(d => d).ToList());

            context.Framework._dimensionsByMinimalRule = rulesAndDimensions
                .GroupBy(e => e.Rule.MinimalDerivationRule)
                .ToDictionary(e => e.Key, e => e.Select(o => o.Dimension).Distinct().OrderBy(d => d).ToList());

            context.Framework._dimensionsByBasicRule = rulesAndDimensions
                .GroupBy(e => e.Rule.BasicDerivationRule)
                .ToDictionary(e => e.Key, e => e.Select(o => o.Dimension).Distinct().OrderBy(d => d).ToList());
        }

        #endregion [ Internals ]

        public DimensionBuilder WithDimension(string name)
        {
            if (dimensionsDict.ContainsKey(name))
                throw new ArgumentException(string.Format(Resources.DuplicateElementNameAlreadyExist, name));

            var builder = new DimensionBuilder(this, name);

            dimensionsList.Add(builder);
            dimensionsDict.Add(name, builder);

            return builder;
        }
    }

    internal class FrameworkCreationContext
    {
        public MeasurementFrameworkBuilder FrameworkBuilder { get; set; }

        public MeasurementFramework Framework { get; set; }

        public Dictionary<string, Dimension> Dimensions { get; set; }

        public HashSet<int> DimensionIdentifiers { get; set; }

        public int NextDimensionId { get; set; }

        public int NextDimensionOrder { get; set; }
    }
}
