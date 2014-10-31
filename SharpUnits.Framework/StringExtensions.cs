using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharpUnits.Framework
{
    internal static class StringExtensions
    {
        #region [ IsValidCSharpIdentifier ]
        // http://msdn.microsoft.com/en-us/library/aa664670(v=vs.71).aspx
        public const string CSharpLetterCharPattern = @"\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}";
        public const string CSharpCombiningCharPattern = @"\p{Mn}\p{Mc}";
        public const string CSharpDecimalDigitCharPattern = @"\p{Nd}";
        public const string CSharpConnectingCharPattern = @"\p{Pc}";
        public const string CSharpFormattingCharPattern = @"\p{Cf}";
        public const string CSharpIdentifierStartCharPattern = "[" + CSharpLetterCharPattern + "_" + "]";
        public const string CSharpIdentifierPartCharPattern = "[" + CSharpLetterCharPattern + CSharpDecimalDigitCharPattern + CSharpConnectingCharPattern + CSharpCombiningCharPattern + CSharpFormattingCharPattern + "]";
        public const string PartCSharpIdentifierPattern = CSharpIdentifierStartCharPattern + CSharpIdentifierPartCharPattern + @"*";

        public static readonly Regex PartCSharpIdentifierRegex = new Regex(PartCSharpIdentifierPattern, RegexOptions.Compiled);
        public static readonly Regex AloneCSharpIdentifierRegex = new Regex("^" + PartCSharpIdentifierPattern + "$", RegexOptions.Compiled);

        public static bool IsValidCSharpIdentifier(string text)
        {
            return text != null && AloneCSharpIdentifierRegex.IsMatch(text);
        }
        #endregion

        #region [ IsValidSymbol ]
        public const string SymbolCharPattern = @"[\p{L}\p{M}\p{S}\p{N}]";
        public static readonly Regex PartSymbolRegex = new Regex(SymbolCharPattern + "+", RegexOptions.Compiled);
        public static readonly Regex AloneSymbolRegex = new Regex("^" + SymbolCharPattern + "+$", RegexOptions.Compiled);
        public static bool IsValidSymbol(string text)
        {
            return text != null && AloneSymbolRegex.IsMatch(text);
        }
        #endregion
    }
}
