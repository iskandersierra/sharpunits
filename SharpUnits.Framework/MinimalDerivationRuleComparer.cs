namespace SharpUnits.Framework
{
    public class MinimalDerivationRuleComparer :
        ReferenceComparerBase<IDimensionRuleBase>
    {

        private static MinimalDerivationRuleComparer _default;
        public static MinimalDerivationRuleComparer Default
        {
            get { return _default ?? (_default = new MinimalDerivationRuleComparer()); }
        }

        protected override int CompareOverride(IDimensionRuleBase x, IDimensionRuleBase y)
        {
            return x.MinimalDerivationRule.CompareTo(y.MinimalDerivationRule);
        }

        protected override int GetHashCodeOverride(IDimensionRuleBase obj)
        {
            return obj.MinimalDerivationRule.GetHashCode();
        }
    }
}