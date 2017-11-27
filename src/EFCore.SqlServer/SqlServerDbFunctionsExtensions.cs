using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
    ///     The methods on this class are accessed via <see cref="EF.Functions" />.
    /// </summary>
    public static class SqlServerDbFunctionsExtensions
    {
        /// <summary>
        /// <para>
        ///     An implementation of the T-SQL FREEETEXT method. It is used to perform full text searches on full text indexed columns
        /// </para>
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="columnName">the column where the search will be performed</param>
        /// <param name="freeText">the text that is to be searched</param>
        /// <param name="languageTerm">language ID from sys.syslanguages table</param>
        public static bool FreeText(
            [CanBeNull] this DbFunctions _,
            [NotNull] string columnName,
            [NotNull] string freeText,
            int languageTerm)
            => FreeTextCore(columnName, freeText, languageTerm);

        /// <summary>
        /// <para>
        ///     An implementation of the T-SQL FREEETEXT method. It is used to perform full text searches on full text indexed columns
        /// </para>
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="columnName">the column where the search will be performed</param>
        /// <param name="freeText">the text that is to be searched</param>
        public static bool FreeText(
            [CanBeNull] this DbFunctions _,
            [NotNull] string columnName,
            [NotNull] string freeText)
            => FreeTextCore(columnName, freeText, null);

        private static bool FreeTextCore(string columnName, string freeText, int? languageTerm)
        {
            throw new NotImplementedException();
        }
    }
}
