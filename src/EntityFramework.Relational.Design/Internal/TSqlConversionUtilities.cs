// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class TSqlConversionUtilities
    {
        /// <summary>
        ///     Converts a string of the form 'There''s a double single quote in here'
        ///     or, for unicode strings, N'There''s a double single quote in here'
        ///     (including the optional N and the outer single quotes) to the string literal
        ///     "There's a double single quote in here" (not including the double quotes).
        /// </summary>
        /// <param name="stringLiteral"> the string to convert </param>
        /// <returns> the converted string, or null if it cannot convert </returns>
        public virtual string ConvertStringLiteral([NotNull] string stringLiteral)
        {
            Check.NotEmpty(stringLiteral, nameof(stringLiteral));

            if (stringLiteral[0] == 'N')
            {
                stringLiteral = stringLiteral.Substring(1);
            }

            var sqlServerStringLiteralLength = stringLiteral.Length;
            if (sqlServerStringLiteralLength < 2)
            {
                return null;
            }

            if (stringLiteral[0] != '\''
                || stringLiteral[sqlServerStringLiteralLength - 1] != '\'')
            {
                return null;
            }

            return stringLiteral.Substring(1, sqlServerStringLiteralLength - 2)
                .Replace("''", "'");
        }

        /// <summary>
        ///     T-SQL does not have a direct bit representation instead the
        ///     values are stored as 0 or 1. Interpret these as false and
        ///     true respectively.
        /// </summary>
        /// <param name="stringLiteral"> the string to convert </param>
        /// <returns>
        ///     false if the string can be interpreted as 0, true if it can be
        ///     interpreted as 1, otherwise null
        /// </returns>
        public virtual bool? ConvertBitLiteral([NotNull] string stringLiteral)
        {
            Check.NotEmpty(stringLiteral, nameof(stringLiteral));

            int result;
            if (int.TryParse(stringLiteral, out result))
            {
                if (result == 0)
                {
                    return false;
                }

                if (result == 1)
                {
                    return true;
                }
            }

            return null;
        }

        /// <summary>
        ///     For metadata T-SQL can return the value "NULL" e.g. for default values.
        ///     Test for that value;
        /// </summary>
        /// <param name="stringLiteral"> the string to convert </param>
        /// <returns>
        ///     true if the string is literally "NULL", false otherwise
        /// </returns>
        public virtual bool IsLiteralNull([NotNull] string stringLiteral)
        {
            Check.NotEmpty(stringLiteral, nameof(stringLiteral));

            return stringLiteral == "NULL";
        }
    }
}
