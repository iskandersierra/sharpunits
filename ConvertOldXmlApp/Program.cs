using System.Data;
using SharpUnits.SourceData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpUnits.SourceData;
using Unit = SharpUnits.SourceData.Unit;

namespace ConvertOldXmlApp
{
    class Program
    {
        public Program()
        {
        }

        static void Main(string[] args)
        {
            try
            {
                TestNewFormat();
                //ConvertFormat();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
            }
            if (Debugger.IsAttached)
            {
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private static void TestNewFormat()
        {
            var inFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleUnits.xml");
            var outFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleUnitsOut.xml");

            var data = SharpUnitsDefinitionServices.Load(inFileName);
            SharpUnitsDefinitionServices.Save(outFileName, data);
        }

        private static void ConvertFormat()
        {
            var xmlFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Magnitudes.xml");
            var xamlFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Units.xml");
            var data = new PhysicalQuantities(xmlFileName);
            if (data.Context.Errors.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Errors reading input file!!!");

                foreach (var error in data.Context.Errors)
                {
                    Console.WriteLine(error);
                }
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.WriteLine("Converting units");
                var context = new ConvertContext(data);
                var definition = CreateSharpUnitsDefinition(context);
                SharpUnitsDefinitionServices.Save(xamlFileName, definition);

                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        private static SharpUnitsDefinition CreateSharpUnitsDefinition(ConvertContext context)
        {
            var result = new SharpUnitsDefinition();

            foreach (var quantity in context.Data.Quantities.OfType<BaseQuantity>())
            {
                var baseDim = CreateBaseDimension(context, quantity);
                result.Dimensions.Add(baseDim);
            }

            foreach (var quantity in context.Data.Quantities.OfType<DerivedQuantity>())
            {
                var dim = CreateDerivedDimension(context, quantity);
                result.Dimensions.Add(dim);
            }
            
            var allUnits = context.Data.UnitSystems.Where(e => e.Name != "RSI").SelectMany(e => e.Quantities).SelectMany(e => e.Units);
            foreach (var unit in allUnits.OfType<UnitBase>())
            {
                var baseUn = CreateBaseUnit(context, unit);
                result.Units.Add(baseUn);
            }

            return result;
        }

        #region [ Dimensions ]

        private static BaseDimension CreateBaseDimension(ConvertContext context, BaseQuantity quantity)
        {
            var result = new BaseDimension();
            InitializeDimension(context, result, quantity);
            context.Dimensions.Add(quantity, result);
            return result;
        }

        private static DerivedDimension CreateDerivedDimension(ConvertContext context, DerivedQuantity quantity)
        {
            var result = new DerivedDimension();
            InitializeDimension(context, result, quantity);
            foreach (var item in quantity.BaseQuantities)
            {
                var dim = context.Dimensions[item.Quantity];
                var exp = new DimensionExponent(dim, item.Exponent);
                result.DerivedFrom.Add(exp);
            }
            context.Dimensions.Add(quantity, result);
            return result;
        }

        private static void InitializeDimension(ConvertContext context, Dimension result, QuantityBase quantity)
        {
            result.Name = quantity.Name;
            result.Symbol = quantity.Symbol;
            InitializeInformationItem(context, result, quantity.EnglishCaption, quantity.SpanishCaption,
                quantity.Reference);
        }

        #endregion


        #region [ Units ]

        private static BaseUnit CreateBaseUnit(ConvertContext context, UnitBase unit)
        {
            var result = new BaseUnit();
            if (context.LoadedUnits.ContainsKey(unit.Name))
                unit.Name = unit.Parent.Parent.Name + unit.Name;
            if (context.LoadedUnits.ContainsKey(unit.Name))
                unit.Name = unit.Parent.Name + unit.Name;
            if (context.LoadedUnits.ContainsKey(unit.Name))
                throw new DuplicateNameException(string.Format("Name {0} is already used for a unit", unit.Name));
            context.LoadedUnits.Add(unit.Name, unit);
            InitializeUnit(context, result, unit);
            context.Units.Add(unit, result);
            return result;
        }

        private static void InitializeUnit(ConvertContext context, SharpUnits.SourceData.Unit result, UnitBase unit)
        {
            result.Name = unit.Name;
            result.Symbol = unit.Symbol;
            result.Dimension = context.Dimensions[unit.Parent.Quantity];
            InitializeInformationItem(context, result, unit.EnglishSingular, unit.SpanishSingular, unit.Reference);
            if (!string.IsNullOrWhiteSpace(unit.EnglishPlural))
                result.PluralCaptions.Add(new LocalizedText(unit.EnglishPlural, "en"));
            if (!string.IsNullOrWhiteSpace(unit.SpanishPlural))
                result.PluralCaptions.Add(new LocalizedText(unit.SpanishPlural, "es"));
        }

        #endregion

        private static void InitializeInformationItem(ConvertContext context, InformationItem result, string englishCaption, string spanishCaption, string englishReference)
        {
            if (!string.IsNullOrWhiteSpace(englishCaption))
                result.Captions.Add(new LocalizedText(englishCaption, "en"));
            if (!string.IsNullOrWhiteSpace(spanishCaption))
                result.Captions.Add(new LocalizedText(spanishCaption, "es"));
            if (!string.IsNullOrWhiteSpace(englishReference))
                result.References.Add(new LocalizedText(englishReference, "en"));
        }
    }
}
