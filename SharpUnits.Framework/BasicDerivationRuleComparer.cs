namespace SharpUnits.Framework
{
    public class BasicDerivationRuleComparer :
        ReferenceComparerBase<IDimensionRuleBase>
    {

        private static BasicDerivationRuleComparer _default;
        public static BasicDerivationRuleComparer Default
        {
            get { return _default ?? (_default = new BasicDerivationRuleComparer()); }
        }


        protected override int CompareOverride(IDimensionRuleBase x, IDimensionRuleBase y)
        {
            return x.BasicDerivationRule.CompareTo(y.BasicDerivationRule);
        }

        protected override int GetHashCodeOverride(IDimensionRuleBase obj)
        {
            return obj.BasicDerivationRule.GetHashCode();
        }
    }
}