using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework
{

    public abstract class MeasurementLocalizableItem : IFormattable
    {
        internal string _name;
        internal IReadOnlyDictionary<string, string> _captions;
        internal IReadOnlyDictionary<string, string> _symbols;
        internal IReadOnlyDictionary<string, string> _descriptions;
        internal IReadOnlyDictionary<string, string> _references;
        internal CultureInfo _defaultCulture;

        public string Name
        {
            get { return _name; }
        }

        public CultureInfo DefaultCulture
        {
            get { return _defaultCulture; }
        }

        public string Caption { get { return GetLocalizedValue(_captions, null); } }
        public string Symbol { get { return GetLocalizedValue(_symbols, null); } }
        public string Description { get { return GetLocalizedValue(_descriptions, null); } }
        public string Reference { get { return GetLocalizedValue(_references, null); } }

        public string GetCaption(CultureInfo culture)
        {
            return GetLocalizedValue(_captions, culture);
        }
        public string GetSymbol(CultureInfo culture)
        {
            return GetLocalizedValue(_symbols, culture);
        }
        public string GetDescription(CultureInfo culture)
        {
            return GetLocalizedValue(_descriptions, culture);
        }
        public string GetReference(CultureInfo culture)
        {
            return GetLocalizedValue(_references, culture);
        }

        private string GetLocalizedValue(IReadOnlyDictionary<string, string> dictionary, CultureInfo culture)
        {
            if (culture == null) culture = CultureInfo.CurrentCulture;

            string value;
            if (dictionary.TryGetValue(culture.Name, out value))
                return value;

            if (culture == CultureInfo.InvariantCulture)
                return null; //TODO: throw exception?

            return GetLocalizedValue(dictionary, culture.Parent);
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        /// <summary>
        /// Returns a textual representation of the element
        /// </summary>
        /// <param name="format">Can be one of the following formats:<br/>
        /// "N" or "n" print the name of the element<br/>
        /// "S" or "s" print the symbol of the element<br/>
        /// "C" or "c" print the caption of the element<br/>
        /// "D" or "d" print the description of the element<br/>
        /// "R" or "r" print the reference of the element<br/>
        /// None print the default representation of the element, which is whichever of the following are non-empty: Symbol, Caption or Name<br/>
        /// You can optionally append one following symbols to the previous case:<br/>
        /// "T" or "t" to indicate title case<br/>
        /// "U" or "u" to indicate upper case<br/>
        /// "L" or "l" to indicate lower case<br/>
        /// None to indicate no case change
        /// The order or casing of modifiers is not important, so Ts, St, st and TS all mean Title case symbol
        /// Casing modifiers do not affect Name and Reference modifiers
        /// </param>
        /// <param name="formatProvider">Default is CurrentCulture</param>
        /// <returns>a textual representation of the element</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) format = string.Empty;
            var contentFormat = DefaultContentFormat;
            var casingFormat = DefaultCasingFormat;
            var bracketFormat = DefaultBracketsFormat;
            bool contentFormatFound = false;
            bool casingFormatFound = false;
            bool bracketFormatFound = false;

            if (format.Length > 3) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));

            format = CultureInfo.InvariantCulture.TextInfo.ToLower(format);

            for (int i = 0; i < format.Length; i++)
            {
                switch (format[i])
                {
                    #region [ Content ]
                    case 'n':
                        if (contentFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        contentFormat = LocalizableContentFormat.Name;
                        contentFormatFound = true;
                        break;
                    case 's':
                        if (contentFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        contentFormat = LocalizableContentFormat.Symbol;
                        contentFormatFound = true;
                        break;
                    case 'c':
                        if (contentFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        contentFormat = LocalizableContentFormat.Caption;
                        contentFormatFound = true;
                        break;
                    case 'd':
                        if (contentFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        contentFormat = LocalizableContentFormat.Description;
                        contentFormatFound = true;
                        break;
                    case 'r':
                        if (contentFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        contentFormat = LocalizableContentFormat.Reference;
                        contentFormatFound = true;
                        break;
                    #endregion [ Content ]
                    #region [ Casing ]
                    case 't':
                        if (casingFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        casingFormat = LocalizableCasingFormat.TitleCase;
                        casingFormatFound = true;
                        break;
                    case 'u':
                        if (casingFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        casingFormat = LocalizableCasingFormat.UpperCase;
                        casingFormatFound = true;
                        break;
                    case 'l':
                        if (casingFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        casingFormat = LocalizableCasingFormat.LowerCase;
                        casingFormatFound = true;
                        break;
                    #endregion [ Casing ]
                    #region [ Brackets ]
                    case '(':
                    case ')':
                        if (bracketFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        bracketFormat = BracketsFormat.Parentheses;
                        bracketFormatFound = true;
                        break;
                    case '[':
                    case ']':
                        if (bracketFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        bracketFormat = BracketsFormat.SquareBrackets;
                        bracketFormatFound = true;
                        break;
                    case '{':
                    case '}':
                        if (bracketFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        bracketFormat = BracketsFormat.CurlyBraces;
                        bracketFormatFound = true;
                        break;
                    case '<':
                    case '>':
                        if (bracketFormatFound) throw new FormatException(string.Format(Resources.InvalidFormatSpecifier, format));
                        bracketFormat = BracketsFormat.AngularParentheses;
                        bracketFormatFound = true;
                        break;
                    #endregion [ Brackets ]
                }
            }

            return ToString(contentFormat, casingFormat, bracketFormat, formatProvider);
        }

        public string ToString(
            LocalizableContentFormat contentFormat, 
            LocalizableCasingFormat casingFormat,
            BracketsFormat bracketsFormat,
            IFormatProvider formatProvider)
        {
            formatProvider = formatProvider ?? CultureInfo.CurrentCulture;
            var culture = (CultureInfo)formatProvider.GetFormat(typeof(CultureInfo)) ?? formatProvider as CultureInfo ?? CultureInfo.CurrentCulture;
            var textInfo = (TextInfo)formatProvider.GetFormat(typeof(TextInfo)) ?? culture.TextInfo;

            string text = null;

            #region [ Content ]

            switch (contentFormat)
            {
                case LocalizableContentFormat.Default:
                    text = GetSymbol(culture);
                    if (string.IsNullOrWhiteSpace(text))
                        text = GetCaption(culture);
                    if (string.IsNullOrWhiteSpace(text))
                        text = Name;
                    break;
                case LocalizableContentFormat.Name:
                    text = Name;
                    casingFormat = LocalizableCasingFormat.Unchanged;
                    break;
                case LocalizableContentFormat.Symbol:
                    text = GetSymbol(culture);
                    break;
                case LocalizableContentFormat.Caption:
                    text = GetCaption(culture);
                    break;
                case LocalizableContentFormat.Description:
                    text = GetDescription(culture);
                    bracketsFormat = BracketsFormat.None;
                    break;
                case LocalizableContentFormat.Reference:
                    text = GetReference(culture);
                    casingFormat = LocalizableCasingFormat.Unchanged;
                    bracketsFormat = BracketsFormat.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("contentFormat");
            }

            #endregion

            #region [ Casing ]

            switch (casingFormat)
            {
                case LocalizableCasingFormat.Unchanged:
                    break;
                case LocalizableCasingFormat.UpperCase:
                    text = textInfo.ToUpper(text);
                    break;
                case LocalizableCasingFormat.LowerCase:
                    text = textInfo.ToLower(text);
                    break;
                case LocalizableCasingFormat.TitleCase:
                    text = textInfo.ToTitleCase(text);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("casingFormat");
            }

            #endregion

            #region [ Brackets ]

            switch (bracketsFormat)
            {
                case BracketsFormat.None:
                    break;
                case BracketsFormat.Parentheses:
                    text = '(' + text + ')';
                    break;
                case BracketsFormat.SquareBrackets:
                    text = '[' + text + ']';
                    break;
                case BracketsFormat.CurlyBraces:
                    text = '{' + text + '}';
                    break;
                case BracketsFormat.AngularParentheses:
                    text = '<' + text + '>';
                    break;
                default:
                    throw new ArgumentOutOfRangeException("bracketsFormat");
            }

            #endregion

            return text;
        }

        protected virtual LocalizableContentFormat DefaultContentFormat
        {
            get
            {
                return LocalizableContentFormat.Default;
            }
        }

        protected virtual LocalizableCasingFormat DefaultCasingFormat
        {
            get
            {
                return LocalizableCasingFormat.Unchanged;
            }
        }

        protected virtual BracketsFormat DefaultBracketsFormat
        {
            get
            {
                return BracketsFormat.None;
            }
        }
    }

    public enum LocalizableContentFormat
    {
        Default,
        Name,
        Symbol,
        Caption,
        Description,
        Reference,
    }

    public enum LocalizableCasingFormat
    {
        Unchanged,
        UpperCase,
        LowerCase,
        TitleCase,
    }

    public enum BracketsFormat
    {
        None,
        Parentheses,
        SquareBrackets,
        CurlyBraces,
        AngularParentheses,
    }
}