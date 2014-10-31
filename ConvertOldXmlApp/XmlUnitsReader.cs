using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ConvertOldXmlApp
{
    public class LoadContext
    {
        public List<string> Errors = new List<string>();
        public List<Tuple<int, Action<PhysicalQuantities>, XObject>> PostActions = new List<Tuple<int, Action<PhysicalQuantities>, XObject>>();

        public bool HasErrors { get { return Errors.Count > 0; } }

        public void NotifyError(string message, XObject node = null)
        {
            var lineInfo = node as IXmlLineInfo;
            if (lineInfo != null && lineInfo.HasLineInfo())
                message = message + string.Format(": on line {0} col {1}", lineInfo.LineNumber, lineInfo.LinePosition);
            Errors.Add(message);
        }

        public void AddAction(Action<PhysicalQuantities> action, XObject node = null, int priority = 0)
        {
            PostActions.Add(new Tuple<int, Action<PhysicalQuantities>, XObject>(priority, action, node));
        }

        public void RunActions(PhysicalQuantities root)
        {
            foreach (var tup in PostActions.OrderBy(p => p.Item1))
            {
                try
                {
                    tup.Item2(root);
                }
                catch (Exception ex)
                {
                    NotifyError(ex.ToString(), tup.Item3);
                }
                if (HasErrors) break;
            }
        }

        public static T GetSingle<T>(IEnumerable<T> elements, Func<T, bool> predicate, string searchingElementName, string searchingName, string referenceElementName, string referenceName)
        {
            var found = elements.Where(predicate).ToArray();
            if (found.Length == 0) throw new InvalidOperationException(searchingElementName + " not found " + searchingName + " on " + referenceElementName + " " + referenceName);
            if (found.Length > 1) throw new InvalidOperationException("Ambiguous " + searchingElementName + " " + searchingName + " on " + referenceElementName + " " + referenceName);
            return found[0];
        }
    }
    public class PhysicalQuantities
    {
        public LoadContext Context = new LoadContext();

        public PhysicalQuantities(string inputFile)
            : this(XDocument.Load(inputFile, LoadOptions.SetLineInfo))
        {
        }
        public PhysicalQuantities(XDocument doc)
            : this(doc.Root)
        {
        }
        public PhysicalQuantities(XElement element)
        {
            Load(element, Context);
        }

        public string Reference { get; set; }
        public List<QuantityBase> Quantities { get; set; }
        public List<PrefixTable> PrefixTables { get; set; }
        public List<UnitSystem> UnitSystems { get; set; }
        public List<UnitConversion> UnitConversions { get; set; }

        private void Load(XElement element, LoadContext ctx)
        {
            Reference = (string)element.Attribute("Reference");

            Quantities = new List<QuantityBase>();
            var xQuantities = element.Element("Quantities");
            foreach (var xQuantity in xQuantities.Elements())
            {
                Quantities.Add(QuantityBase.Load(this, xQuantity, ctx));
                if (ctx.HasErrors) return;
            }
            PrefixTables = new List<PrefixTable>();
            var xPrefixTables = element.Element("PrefixTables");
            foreach (var xPrefixTable in xPrefixTables.Elements("PrefixTable"))
            {
                PrefixTables.Add(new PrefixTable(this, xPrefixTable, ctx));
                if (ctx.HasErrors) return;
            }
            UnitSystems = new List<UnitSystem>();
            var xUnitSystems = element.Element("UnitSystems");
            foreach (var xUnitSystem in xUnitSystems.Elements("UnitSystem"))
            {
                UnitSystems.Add(new UnitSystem(this, xUnitSystem, ctx));
                if (ctx.HasErrors) return;
            }
            UnitConversions = new List<UnitConversion>();
            var xUnitConversions = element.Element("UnitConversions");
            foreach (var xUnitConversion in xUnitConversions.Elements("UnitConversion"))
            {
                UnitConversions.Add(new UnitConversion(this, xUnitConversion, ctx));
                if (ctx.HasErrors) return;
            }
            ctx.RunActions(this);
        } // void Load(XElement element, LoadContext ctx)
    } // public class PhysicalQuantities

    public abstract class QuantityBase
    {
        public QuantityBase(PhysicalQuantities parent, XElement element, LoadContext ctx)
        {
            Parent = parent;
            if (element.Attribute("Name") == null)
                ctx.NotifyError("QuantityBase missing Name attribute", element);
            if (ctx.HasErrors) return;

            Name = (string)element.Attribute("Name");
            EnglishCaption = (string)element.Attribute("EnglishName");
            SpanishCaption = (string)element.Attribute("SpanishName");
            Reference = (string)element.Attribute("Reference");
            if (element.Attribute("Symbol") != null)
                Symbol = (string)element.Attribute("Symbol");
            if (ctx.HasErrors) return;
        }

        public PhysicalQuantities Parent { get; set; }
        public string Reference { get; set; }
        public string Name { get; set; }
        public string EnglishCaption { get; set; }
        public string SpanishCaption { get; set; }
        public string Symbol { get; set; }

        public static QuantityBase Load(PhysicalQuantities parent, XElement element, LoadContext ctx)
        {
            if (element.Name == "BaseQuantity")
                return new BaseQuantity(parent, element, ctx);
            if (element.Name == "DerivedQuantity")
                return new DerivedQuantity(parent, element, ctx);
            ctx.NotifyError("Unknown quantity type", element);
            return null;
        }
    } // public abstract class QuantityBase

    public class BaseQuantity : QuantityBase
    {
        public BaseQuantity(PhysicalQuantities parent, XElement element, LoadContext ctx)
            : base(parent, element, ctx)
        {
        }
    } // public class BaseQuantity

    public class DerivedQuantity : QuantityBase
    {
        public DerivedQuantity(PhysicalQuantities parent, XElement element, LoadContext ctx)
            : base(parent, element, ctx)
        {
            if (ctx.HasErrors) return;

            BaseQuantities = new List<QuantityExp>();
            foreach (var xQuantityExp in element.Elements("QuantityExp"))
            {
                BaseQuantities.Add(new QuantityExp(this, xQuantityExp, ctx));
                if (ctx.HasErrors) return;
            }
        }

        public List<QuantityExp> BaseQuantities { get; set; }
    } // public class DerivedQuantity

    public class QuantityExp
    {
        public QuantityExp(DerivedQuantity parent, XElement element, LoadContext ctx)
        {
            Parent = parent;
            if (element.Attribute("Name") == null)
                ctx.NotifyError("QuantityExp missing Name attribute", element);
            if (element.Attribute("Exponent") == null)
                ctx.NotifyError("QuantityExp missing Exponent attribute", element);
            if (ctx.HasErrors) return;

            QuantityName = (string)element.Attribute("Name");
            Exponent = (int)element.Attribute("Exponent");

            ctx.AddAction(r => Update(r));
        }

        internal void Update(PhysicalQuantities r)
        {
            this.Quantity = LoadContext.GetSingle(r.Quantities, q => q.Name == this.QuantityName, "parent quantity", QuantityName, "derived quantity", Parent.Name);
        }

        public DerivedQuantity Parent { get; set; }
        public string QuantityName { get; set; }
        public QuantityBase Quantity { get; set; }

        public int Exponent { get; set; }
    } // public class QuantityExp

    public class PrefixTable
    {
        public PrefixTable(PhysicalQuantities parent, XElement element, LoadContext ctx)
        {
            Parent = parent;
            if (element.Attribute("Name") == null)
                ctx.NotifyError("PrefixTable missing Name attribute", element);
            if (ctx.HasErrors) return;

            Name = (string)element.Attribute("Name");
            EnglishCaption = (string)element.Attribute("EnglishName");
            SpanishCaption = (string)element.Attribute("SpanishName");
            Reference = (string)element.Attribute("Reference");

            Prefixes = new List<Prefix>();
            foreach (var xPrefix in element.Elements("Prefix"))
            {
                Prefixes.Add(new Prefix(this, xPrefix, ctx));
                if (ctx.HasErrors) return;
            }
        }

        public PhysicalQuantities Parent { get; set; }
        public string Reference { get; set; }
        public string Name { get; set; }
        public string EnglishCaption { get; set; }
        public string SpanishCaption { get; set; }
        public List<Prefix> Prefixes { get; set; }
    } // public class PrefixTable

    public class Prefix
    {
        public Prefix(PrefixTable parent, XElement element, LoadContext ctx)
        {
            Parent = parent;
            if (element.Attribute("Name") == null)
                ctx.NotifyError("Prefix missing Name attribute", element);
            if (element.Attribute("Factor") == null)
                ctx.NotifyError("Prefix missing Factor attribute", element);
            if (ctx.HasErrors) return;

            Name = (string)element.Attribute("Name");
            EnglishCaption = (string)element.Attribute("EnglishName");
            SpanishCaption = (string)element.Attribute("SpanishName");
            Symbol = (string)element.Attribute("Symbol");
            Factor = (double)element.Attribute("Factor");
        }

        public PrefixTable Parent { get; set; }
        public string Name { get; set; }
        public string EnglishCaption { get; set; }
        public string SpanishCaption { get; set; }
        public string Symbol { get; set; }
        public double Factor { get; set; }
    } // public class Prefix

    public class UnitSystem
    {
        public UnitSystem(PhysicalQuantities parent, XElement element, LoadContext ctx)
        {
            Parent = parent;
            if (element.Attribute("Name") == null)
                ctx.NotifyError("UnitSystem missing Name attribute", element);
            if (ctx.HasErrors) return;

            Name = (string)element.Attribute("Name");
            EnglishCaption = (string)element.Attribute("EnglishName");
            SpanishCaption = (string)element.Attribute("SpanishName");
            Reference = (string)element.Attribute("Reference");

            Quantities = new List<UnitSystemQuantity>();
            foreach (var xElem in element.Elements("Quantity"))
            {
                Quantities.Add(new UnitSystemQuantity(this, xElem, ctx));
                if (ctx.HasErrors) return;
            }
        }

        public PhysicalQuantities Parent { get; set; }
        public string Reference { get; set; }
        public string EnglishCaption { get; set; }
        public string SpanishCaption { get; set; }
        public string Name { get; set; }
        public List<UnitSystemQuantity> Quantities { get; set; }
    } // public class UnitSystem

    public class UnitSystemQuantity
    {
        public UnitSystemQuantity(UnitSystem parent, XElement element, LoadContext ctx)
        {
            Parent = parent;
            if (element.Attribute("Name") == null)
                ctx.NotifyError("UnitSystemQuantity missing Name attribute", element);
            if (ctx.HasErrors) return;

            Name = (string)element.Attribute("Name");

            Units = new List<UnitBase>();
            foreach (var xElem in element.Elements())
            {
                Units.Add(UnitBase.Load(this, xElem, ctx));
                if (ctx.HasErrors) return;
            }

            ctx.AddAction(r => Update(r), element);
        }

        internal void Update(PhysicalQuantities r)
        {
            this.Quantity = LoadContext.GetSingle(r.Quantities, t => t.Name == this.Name, "unit system quantity", Name, "unit system", Parent.Name);
        }

        public UnitSystem Parent { get; set; }
        public string Name { get; set; }
        public QuantityBase Quantity { get; set; }
        public List<UnitBase> Units { get; set; }
    } // public class UnitSystemQuantity

    public abstract class UnitBase
    {
        public UnitBase(UnitSystemQuantity parent, XElement element, LoadContext ctx)
        {
            Parent = parent;
            if (element.Attribute("Name") == null)
                ctx.NotifyError("Unit missing Name attribute", element);
            if (ctx.HasErrors) return;

            Name = (string)element.Attribute("Name");
            EnglishSingular = (string)element.Attribute("EnglishSingular");
            EnglishPlural = (string)element.Attribute("EnglishPlural");
            SpanishSingular = (string)element.Attribute("SpanishSingular");
            SpanishPlural = (string)element.Attribute("SpanishPlural");
            Symbol = (string)element.Attribute("Symbol");
            Reference = (string)element.Attribute("Reference");
        }
        internal UnitBase(UnitSystemQuantity parent, string name, string symbol, string reference)
        {
            Parent = parent;
            Name = name;
            Symbol = symbol;
            Reference = reference;
        }

        public string Reference { get; set; }
        public string SpanishSingular { get; set; }
        public string SpanishPlural { get; set; }
        public string EnglishSingular { get; set; }
        public string EnglishPlural { get; set; }
       public string Name { get; set; }
        public string Symbol { get; set; }
        public UnitSystemQuantity Parent { get; set; }

        public virtual IEnumerable<UnitBase> GetAllUnits()
        {
            yield return this;
        }

        public static UnitBase Load(UnitSystemQuantity parent, XElement element, LoadContext ctx)
        {
            if (element.Name == "Unit")
                return new Unit(parent, element, ctx);
            if (element.Name == "ScaledUnit")
                return new ScaledUnit(parent, element, ctx);
            ctx.NotifyError("Unknown unit type", element);
            return null;
        }
    } // public abstract class UnitBase

    public class Unit : UnitBase
    {
        public Unit(UnitSystemQuantity parent, XElement element, LoadContext ctx)
            : base(parent, element, ctx)
        {
            if (ctx.HasErrors) return;

            PrefixedUnits = new List<PrefixedUnit>();
            foreach (var xElem in element.Elements("PrefixedUnits"))
            {
                PrefixedUnits.Add(new PrefixedUnit(this, xElem, ctx));
                if (ctx.HasErrors) return;
            }
            ctx.AddAction(r => Update(r), element);
        }

        internal void Update(PhysicalQuantities r)
        {
            var list = new List<UnitBase>();
            list.Add(this);
            foreach (var prefixedUnit in PrefixedUnits)
                foreach (Prefix prefix in prefixedUnit.Table.Prefixes)
                    if (prefixedUnit.ExceptPrefixes == null || !prefixedUnit.ExceptPrefixes.Contains(prefix.Name))
                    {
                        var scaledUnit = new ScaledUnit(Parent, prefix.Name + this.Name, this.Symbol == null ? null : prefix.Symbol + this.Symbol, this.Reference, prefix.Factor, 0.0, this);
                        list.Add(scaledUnit);
                    }
            AllUnits = list;
        }

        public List<PrefixedUnit> PrefixedUnits { get; set; }
        public IEnumerable<UnitBase> AllUnits { get; set; }

        public override IEnumerable<UnitBase> GetAllUnits()
        {
            return AllUnits;
        }
    } // public class Unit

    public class PrefixedUnit
    {
        public PrefixedUnit(Unit parent, XElement element, LoadContext ctx)
        {
            Parent = parent;
            if (element.Attribute("TableName") == null)
                ctx.NotifyError("PrefixedUnit missing TableName attribute", element);
            if (ctx.HasErrors) return;

            TableName = (string)element.Attribute("TableName");

            if (element.Attribute("Except") == null)
                this.ExceptPrefixes = null;
            else
                this.ExceptPrefixes = ((string)element.Attribute("Except")).Split(',').ToList();

            ctx.AddAction(r => Update(r), element);
        }

        internal void Update(PhysicalQuantities r)
        {
            this.Table = LoadContext.GetSingle(r.PrefixTables, t => t.Name == TableName, "prefix table", TableName, "unit", Parent.Name);
        }

        public string TableName { get; set; }
        public PrefixTable Table { get; set; }
        public Unit Parent { get; set; }
        public List<string> ExceptPrefixes { get; set; }
    } // public class PrefixedUnit

    public class ScaledUnit : UnitBase
    {
        public ScaledUnit(UnitSystemQuantity parent, XElement element, LoadContext ctx)
            : base(parent, element, ctx)
        {
            if (ctx.HasErrors) return;

            if (element.Attribute("Factor") == null)
                ctx.NotifyError("ScaledUnit missing Factor attribute", element);
            if (element.Attribute("FactorRelativeTo") == null)
                ctx.NotifyError("ScaledUnit missing FactorRelativeTo attribute", element);

            if (ctx.HasErrors) return;

            Factor = (double)element.Attribute("Factor");
            if (element.Attribute("Offset") != null)
                Offset = (double)element.Attribute("Offset");
            RelativeUnitName = (string)element.Attribute("FactorRelativeTo");

            ctx.AddAction(r => Update(r), element);
        }

        internal ScaledUnit(UnitSystemQuantity parent, string name, string symbol, string reference, double factor, double offset, UnitBase relativeUnit)
            : base(parent, name, symbol, reference)
        {
            Factor = factor;
            Offset = offset;
            RelativeUnit = relativeUnit;
            RelativeUnitName = relativeUnit.Name;
        }

        internal void Update(PhysicalQuantities r)
        {
            this.RelativeUnit = LoadContext.GetSingle(Parent.Units, t => t.Name == RelativeUnitName, "relative unit", RelativeUnitName, "unit", Name);
        }

        public double Factor { get; set; }
        public double Offset { get; set; }
        public string RelativeUnitName { get; set; }
        public UnitBase RelativeUnit { get; set; }
    } // public class ScaledUnit

    public class UnitConversion
    {
        public UnitConversion(PhysicalQuantities parent, XElement element, LoadContext ctx)
        {
            Parent = parent;
            if (element.Attribute("Factor") == null)
                ctx.NotifyError("UnitConversion missing Factor attribute", element);
            if (element.Attribute("Quantity") == null)
                ctx.NotifyError("UnitConversion missing Quantity attribute", element);
            if (element.Attribute("SourceUnitSystem") == null)
                ctx.NotifyError("UnitConversion missing SourceUnitSystem attribute", element);
            if (element.Attribute("TargetUnitSystem") == null)
                ctx.NotifyError("UnitConversion missing TargetUnitSystem attribute", element);
            if (element.Attribute("SourceUnit") == null)
                ctx.NotifyError("UnitConversion missing SourceUnit attribute", element);
            if (element.Attribute("TargetUnit") == null)
                ctx.NotifyError("UnitConversion missing TargetUnit attribute", element);
            if (ctx.HasErrors) return;

            Reference = (string)element.Attribute("Reference");
            Factor = (double)element.Attribute("Factor");
            if (element.Attribute("Offset") != null)
                Offset = (double)element.Attribute("Offset");
            QuantityName = (string)element.Attribute("Quantity");
            SourceUnitSystemName = (string)element.Attribute("SourceUnitSystem");
            TargetUnitSystemName = (string)element.Attribute("TargetUnitSystem");
            SourceUnitName = (string)element.Attribute("SourceUnit");
            TargetUnitName = (string)element.Attribute("TargetUnit");

            ctx.AddAction(r => Update(r), element);
        }

        internal void Update(PhysicalQuantities r)
        {
            this.Quantity = LoadContext.GetSingle(r.Quantities, t => t.Name == this.QuantityName, "quantity", QuantityName, "unit conversion", QuantityName + " from " + SourceUnitSystemName + " to " + TargetUnitSystemName);
            this.SourceUnitSystem = LoadContext.GetSingle(r.UnitSystems, t => t.Name == this.SourceUnitSystemName, "source unit system", SourceUnitSystemName, "unit conversion", QuantityName + " from " + SourceUnitSystemName + " to " + TargetUnitSystemName);
            this.TargetUnitSystem = LoadContext.GetSingle(r.UnitSystems, t => t.Name == this.TargetUnitSystemName, "target unit system", TargetUnitSystemName, "unit conversion", QuantityName + " from " + SourceUnitSystemName + " to " + TargetUnitSystemName);

            this.SourceUnit = LoadContext.GetSingle(
                LoadContext.GetSingle(this.SourceUnitSystem.Quantities, t => t.Name == this.QuantityName, "quantity", QuantityName, "unit conversion", QuantityName + " from " + SourceUnitSystemName + " to " + TargetUnitSystemName).Units,
                u => u.Name == this.SourceUnitName, "source unit", SourceUnitName, "unit conversion", QuantityName + " from " + SourceUnitSystemName + " to " + TargetUnitSystemName);

            this.TargetUnit = LoadContext.GetSingle(
                LoadContext.GetSingle(this.TargetUnitSystem.Quantities, t => t.Name == this.QuantityName, "quantity", QuantityName, "unit conversion", QuantityName + " from " + SourceUnitSystemName + " to " + TargetUnitSystemName).Units,
                u => u.Name == this.TargetUnitName, "target unit", TargetUnitName, "unit conversion", QuantityName + " from " + SourceUnitSystemName + " to " + TargetUnitSystemName);
        }

        public PhysicalQuantities Parent { get; set; }
        public string Reference { get; set; }
        public double Factor { get; set; }
        public double Offset { get; set; }
        public string QuantityName { get; set; }
        public string SourceUnitSystemName { get; set; }
        public string TargetUnitSystemName { get; set; }
        public string SourceUnitName { get; set; }
        public string TargetUnitName { get; set; }
        public QuantityBase Quantity { get; set; }
        public UnitSystem SourceUnitSystem { get; set; }
        public UnitSystem TargetUnitSystem { get; set; }
        public UnitBase SourceUnit { get; set; }
        public UnitBase TargetUnit { get; set; }
    } // public class UnitConversion
}
