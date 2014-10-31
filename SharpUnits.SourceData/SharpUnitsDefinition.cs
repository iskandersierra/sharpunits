using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xaml;

namespace SharpUnits.SourceData
{
    public class SharpUnitsDefinition
    {
        List<Dimension> _Dimensions;
        public List<Dimension> Dimensions
        {
            get
            {
                return _Dimensions ?? (_Dimensions = new List<Dimension>());
            }
        }


        List<Unit> _Units;
        public List<Unit> Units
        {
            get
            {
                return _Units ?? (_Units = new List<Unit>());
            }
        }

    }

    #region [ Information item ]
    [RuntimeNameProperty("Name")]
    [DebuggerDisplay("{Name}")]
    public abstract class InformationItem
    {
        public string Name { get; set; }

        List<LocalizedText> _Captions;
        public List<LocalizedText> Captions
        {
            get
            {
                return _Captions ?? (_Captions = new List<LocalizedText>());
            }
        }

        List<LocalizedText> _Comments;
        public List<LocalizedText> Comments
        {
            get
            {
                return _Comments ?? (_Comments = new List<LocalizedText>());
            }
        }

        List<LocalizedText> _References;
        public List<LocalizedText> References
        {
            get
            {
                return _References ?? (_References = new List<LocalizedText>());
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", TypeName, Name);
        }

        protected abstract string TypeName { get; }
    }

    [ContentProperty("Text")]
    [DebuggerDisplay("{Culture}: {Text}")]
    public class LocalizedText
    {
        public LocalizedText()
        {

        }
        public LocalizedText(string text, string culture)
        {
            Text = text;
            Culture = culture;
        }
        public string Text { get; set; }
        public string Culture { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Culture, Text);
        }
    }
    #endregion [ Information item ]

    #region [ Dimensions ]
    public abstract class Dimension : InformationItem
    {
        [DefaultValue(null)]
        public string Symbol { get; set; }

        protected override string TypeName
        {
            get { return "Dimension"; }
        }
    }

    public class BaseDimension : Dimension
    {
    }

    [ContentProperty("DerivedFrom")]
    public class DerivedDimension : Dimension
    {
        List<DimensionExponent> _DerivedFrom;
        public List<DimensionExponent> DerivedFrom
        {
            get
            {
                return _DerivedFrom ?? (_DerivedFrom = new List<DimensionExponent>());
            }
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", base.ToString(), DerivedFrom != null ? string.Join(" X ", DerivedFrom.Select(e => e.ToString())) : "???");
        }
    }

    [DebuggerDisplay("{Dimension}^{Exponent}")]
    public class DimensionExponent
    {
        public DimensionExponent()
        {

        }
        public DimensionExponent(Dimension dimension, int exponent)
        {
            this.Dimension = dimension;
            this.Exponent = exponent;
        }
        public Dimension Dimension { get; set; }
        
        public int Exponent { get; set; }

        public override string ToString()
        {
            return string.Format("{0}^{1}", Dimension != null ? Dimension.Name : "???", Exponent);
        }
    }
    #endregion [ Dimensions ]

    #region [ Units ]
    public abstract class Unit : InformationItem
    {
        [DefaultValue(null)]
        public string Symbol { get; set; }

        public Dimension Dimension { get; set; }

        List<LocalizedText> _PluralCaptions;
        public List<LocalizedText> PluralCaptions
        {
            get
            {
                return _PluralCaptions ?? (_PluralCaptions = new List<LocalizedText>());
            }
        }

        protected override string TypeName
        {
            get { return "Unit"; }
        }
    }

    public class BaseUnit : Unit
    {

    }
    #endregion [ Units ]

    #region [ Services ]
    public static class SharpUnitsDefinitionServices
    {
        public static SharpUnitsDefinition Load(string fileName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(fileName), "fileName not empty");
            Contract.Requires(File.Exists(fileName), "fileName do not exist");

            return (SharpUnitsDefinition)XamlServices.Load(fileName);
        }

        public static void Save(string fileName, SharpUnitsDefinition definition)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(fileName), "fileName not empty");
            Contract.Requires(definition != null, "definition not null");

            XamlServices.Save(fileName, definition);
        }
    }
    #endregion [ Services ]
}
