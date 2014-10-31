namespace SharpUnits.Framework
{
    public abstract class MeasurementIdentifiedItem : 
        MeasurementLocalizableItem
    {
        internal int _identifier;

        public int Identifier
        {
            get { return _identifier; }
        }
    }
}