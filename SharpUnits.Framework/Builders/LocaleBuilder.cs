using System;
using System.Globalization;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework.Builders
{
    public class LocaleBuilder
    {
        internal CultureInfo _culture;
        internal string _caption;
        internal string _symbol;
        internal string _description;
        internal string _reference;
        internal bool _isDefault;

        internal LocaleBuilder(CultureInfo culture, bool isDefault)
        {
            if (culture == null) throw new ArgumentNullException("culture");
            _culture = culture;
            _isDefault = isDefault;
        }

        public CultureInfo Culture
        {
            get { return _culture; }
        }

        public bool IsDefault
        {
            get { return _isDefault; }
        }

        public LocaleBuilder Symbol(string symbol)
        {
            if (!StringExtensions.IsValidSymbol(symbol))
                throw new ArgumentException(string.Format(Resources.SymbolOfElementIsInvalid, symbol), "symbol");
            _symbol = symbol;
            return this;
        }

        public LocaleBuilder Caption(string caption)
        {
            _caption = caption;
            return this;
        }

        public LocaleBuilder Description(string description)
        {
            _description = description;
            return this;
        }

        public LocaleBuilder Reference(string reference)
        {
            _reference = reference;
            return this;
        }
    }
}