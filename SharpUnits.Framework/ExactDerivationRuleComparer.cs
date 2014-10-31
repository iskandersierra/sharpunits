namespace SharpUnits.Framework
{
    public class ExactDerivationRuleComparer :
        ReferenceComparerBase<IDimensionRuleBase>
    {

        private static ExactDerivationRuleComparer _default;
        public static ExactDerivationRuleComparer Default
        {
            get { return _default ?? (_default = new ExactDerivationRuleComparer()); }
        }

        protected override int CompareOverride(IDimensionRuleBase x, IDimensionRuleBase y)
        {
            return x.ExactDerivationRule.CompareTo(y.ExactDerivationRule);
        }

        protected override int GetHashCodeOverride(IDimensionRuleBase obj)
        {
            return obj.ExactDerivationRule.GetHashCode();
        }
    }
}