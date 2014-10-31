using SharpUnits.SourceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvertOldXmlApp
{
    public class ConvertContext
    {
        public PhysicalQuantities Data { get; private set; }
        public Dictionary<QuantityBase, Dimension> Dimensions { get; private set; }
        public Dictionary<UnitBase, SharpUnits.SourceData.Unit> Units { get; private set; }
        public Dictionary<string, UnitBase> LoadedUnits { get; private set; }

        public ConvertContext(PhysicalQuantities data)
        {
            // TODO: Complete member initialization
            this.Data = data;
            Dimensions = new Dictionary<QuantityBase, Dimension>();
            Units = new Dictionary<UnitBase, SharpUnits.SourceData.Unit>();
            LoadedUnits = new Dictionary<string, UnitBase>();
        }


    }
}
