namespace SharpUnits.Framework
{
    public class NormalizedDerivationRuleComparer :
        ReferenceComparerBase<IDimensionRuleBase>
    {

        private static NormalizedDerivationRuleComparer _default;
        public static NormalizedDerivationRuleComparer Default
        {
            get { return _default ?? (_default = new NormalizedDerivationRuleComparer()); }
        }

        protected override int CompareOverride(IDimensionRuleBase x, IDimensionRuleBase y)
        {
            return x.NormalizedDerivationRule.CompareTo(y.NormalizedDerivationRule);
        }

        protected override int GetHashCodeOverride(IDimensionRuleBase obj)
        {
            return obj.NormalizedDerivationRule.GetHashCode();
        }
    }
}