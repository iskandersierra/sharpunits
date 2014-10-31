using System;
using System.Collections.Generic;
using System.Linq;
using SharpUnits.Framework.Properties;

namespace SharpUnits.Framework.Builders
{
    public abstract class MeasurementIdentifiedItemBuilder<T> :
        MeasurementLocalizableItemBuilder<T>
        where T : MeasurementIdentifiedItemBuilder<T>
    {
        private int? _identifier;

        internal int? Identifier
        {
            get { return _identifier; }
        }

        protected MeasurementIdentifiedItemBuilder(string name) : base(name)
        {
        }

        public T WithIdentifier(int identifier)
        {
            if (_identifier.HasValue)
                throw new ArgumentException(string.Format(Resources.CannotSetIdentifierTwice, _name, identifier), "identifier");
            var dimWithSameId = GetIdentifierSpace().FirstOrDefault(d => d.Identifier != null && d.Identifier == identifier);
            if (dimWithSameId != null)
                throw new ArgumentException(string.Format(Resources.ElementAlreadyUseIdentifier, dimWithSameId._name, identifier), "identifier");

            _identifier = identifier;

            return (T)this;
        }

        protected abstract IEnumerable<T> GetIdentifierSpace();

        public override string ToString()
        {
            return string.Format("{0} [{1}]", base.ToString(), _identifier);
        }

        internal int InitIdentifiedItem(
            MeasurementIdentifiedItem item,
            HashSet<int> identifiers,
            int nextId)
        {
            this.InitLocalizableItem(item);

            if (this.Identifier.HasValue)
                item._identifier = this.Identifier.Value;
            else
            {
                while (identifiers.Contains(nextId))
                    nextId++;
                item._identifier = nextId;
                identifiers.Add(nextId);
            }
            return nextId;
        }
    }
}