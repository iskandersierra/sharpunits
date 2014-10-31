using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework.Builders
{
    public abstract class MeasurementLocalizableItemBuilder<T>
        where T : MeasurementLocalizableItemBuilder<T>
    {
        private const string BuilderTypeSuffix = "Builder";
        internal readonly string _name;
        internal readonly List<LocaleBuilder> _locales;

// ReSharper disable once StaticFieldInGenericType
        private static readonly string TypeName;

        static MeasurementLocalizableItemBuilder()
        {
            var typeName = typeof (T).Name;
            if (typeName.Length > BuilderTypeSuffix.Length && typeName.EndsWith(BuilderTypeSuffix))
                typeName = typeName.Substring(0, typeName.Length - BuilderTypeSuffix.Length);
            TypeName = typeName;
        }

        internal MeasurementLocalizableItemBuilder(string name)
        {
            if (!StringExtensions.IsValidCSharpIdentifier(name))
                throw new ArgumentException(string.Format(Resources.ElementNameMustBeValidIdentifier, name), "name");
            _name = name;
            _locales = new List<LocaleBuilder>();

        }

        public T WithLocale(CultureInfo culture, Action<LocaleBuilder> localeActions)
        {
            if (culture == null) throw new ArgumentNullException("culture");
            if (localeActions == null) throw new ArgumentNullException("localeActions");

            return WithLocaleInternal(culture, false, localeActions);
        }

        public T WithLocale(string cultureName, Action<LocaleBuilder> localeActions)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            return WithLocale(culture, localeActions);
        }

        public T WithDefaultLocale(CultureInfo culture, Action<LocaleBuilder> localeActions)
        {
            if (culture == null) throw new ArgumentNullException("culture");
            if (localeActions == null) throw new ArgumentNullException("localeActions");

            return WithLocaleInternal(culture, true, localeActions);
        }

        public T WithDefaultLocale(string cultureName, Action<LocaleBuilder> localeActions)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            return WithDefaultLocale(culture, localeActions);
        }

        private T WithLocaleInternal(CultureInfo culture, bool isDefaultLocale, Action<LocaleBuilder> localeActions)
        {
            if (_locales.Any(e => Equals(e.Culture, culture)))
                throw new ArgumentException(string.Format(Resources.CultureIsAlreadySet, culture, _name));
            if (isDefaultLocale && _locales.Any(e => e.IsDefault))
                throw new ArgumentException(string.Format(Resources.ElementAlreadyHaveDefaultCulture, culture, _locales.First(e => e.IsDefault).Culture, _name));

            var localeBuilder = new LocaleBuilder(culture, isDefaultLocale);
            localeActions(localeBuilder);
            _locales.Add(localeBuilder);

            return (T) this;
        }

        public override string ToString()
        {
            return string.Format("{1}: {0}", _name, TypeName);
        }

        internal void InitLocalizableItem(MeasurementLocalizableItem item)
        {
            item._name = this._name;

            var captions = this._locales.Where(e => e._caption != null).ToDictionary(e => e.Culture.Name, e => e._caption);
            var symbols = this._locales.Where(e => e._symbol != null).ToDictionary(e => e.Culture.Name, e => e._symbol);
            var descriptions = this._locales.Where(e => e._description != null).ToDictionary(e => e.Culture.Name, e => e._description);
            var references = this._locales.Where(e => e._reference != null).ToDictionary(e => e.Culture.Name, e => e._reference);

            item._captions = new ReadOnlyDictionary<string, string>(captions);
            item._symbols = new ReadOnlyDictionary<string, string>(symbols);
            item._descriptions = new ReadOnlyDictionary<string, string>(descriptions);
            item._references = new ReadOnlyDictionary<string, string>(references);
        }
    }
}