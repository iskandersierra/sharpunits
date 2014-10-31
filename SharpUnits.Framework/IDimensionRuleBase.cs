using System;

namespace SharpUnits.Framework
{
    public interface IDimensionRuleBase
    {
        /// <summary>
        /// Reference the measurement framework defining the derivation rules
        /// </summary>
        MeasurementFramework Framework { get; }

        /// <summary>
        /// Gets the kind of rule this is
        /// </summary>
        DerivationRuleKind Kind { get; }

        bool IsBase { get; }
        
        bool IsMinimal { get; }
        
        bool IsNormalized { get; }
        
        bool IsExact { get; }

        /// <summary>
        /// Exact derivation rules can have any dimensions and they can even be repeated
        /// </summary>
        DerivationRule ExactDerivationRule { get; }

        /// <summary>
        /// Normalized derivation rules can have any dimensions and they can even be repeated, but 
        /// they are sorted by descending exponent order and then by ascending identified order
        /// </summary>
        DerivationRule NormalizedDerivationRule { get; }

        /// <summary>
        /// Minimal derivation rules contains only minimal dimensions with no repeated dimensions
        /// This is what allows distinguishing angle, solid angle and ratio, or torque and energy 
        /// because radius and length are minimally diferent although they are the dimensionally 
        /// the same 
        /// </summary>
        DerivationRule MinimalDerivationRule { get; }

        /// <summary>
        /// Basic derivation rules contains only basic dimensions with no repeated dimensions
        /// For example under this rule, energy and torque are the same (dimensionally or basically 
        /// equals) although they are not the same and do not even have the same units
        /// </summary>
        DerivationRule BasicDerivationRule { get; }
    }
}