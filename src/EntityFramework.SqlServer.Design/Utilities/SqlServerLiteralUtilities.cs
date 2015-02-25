// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.Design.Utilities
{
    public class SqlServerLiteralUtilities
    {
        public SqlServerLiteralUtilities([NotNull] ILogger logger)
        {
            Check.NotNull(logger, nameof(logger));

            Logger = logger;
        }

        public virtual ILogger Logger { get; }

        /// <summary>
        /// Converts a string of the form 'There''s a double single quote in here'
        /// (including the outer single quotes) to the string literal
        /// "There's a double single quote in here" (not including the double quotes).
        /// </summary>
        /// <param name="sqlServerStringLiteral"> the string to convert </param>
        /// <returns> the converted string, or null if it cannot convert </returns>
        public virtual string ConvertSqlServerStringLiteral([NotNull] string sqlServerStringLiteral)
        {
            Check.NotEmpty(sqlServerStringLiteral, nameof(sqlServerStringLiteral));

            var sqlServerStringLiteralLength = sqlServerStringLiteral.Length;
            if (sqlServerStringLiteralLength < 2)
            {
                Logger.WriteWarning(Strings.CannotInterpretSqlServerStringLiteral(sqlServerStringLiteral));
                return null;
            }

            if (sqlServerStringLiteral[0] != '\'' ||
                sqlServerStringLiteral[sqlServerStringLiteralLength-1] != '\'')
            {
                Logger.WriteWarning(Strings.CannotInterpretSqlServerStringLiteral(sqlServerStringLiteral));
                return null;
            }

            return sqlServerStringLiteral.Substring(1, sqlServerStringLiteralLength - 2)
                .Replace("''", "'");
        }

        /// <summary>
        /// SQL Server stores the values 0 or 1 in bit columns. Interpret these
        /// as false and true respectively.
        /// </summary>
        /// <param name="sqlServerStringLiteral"> the string to convert </param>
        /// <returns>
        /// false if the string can be interpreted as 0, true if it can be
        /// intrepreted as 1, otherwise null
        /// </returns>
        public virtual bool? ConvertSqlServerBitLiteral([NotNull] string sqlServerStringLiteral)
        {
            Check.NotEmpty(sqlServerStringLiteral, nameof(sqlServerStringLiteral));

            int result;
            if (int.TryParse(sqlServerStringLiteral, out result))
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

        public virtual object ConvertSqlServerDefaultValue(
            [NotNull] Type propertyType , [NotNull] string sqlServerDefaultValue)
        {
            Check.NotNull(propertyType, nameof(propertyType));
            Check.NotEmpty(sqlServerDefaultValue, nameof(sqlServerDefaultValue));

            if (sqlServerDefaultValue.Length < 2)
            {
                return null;
            }

            while (sqlServerDefaultValue[0] == '('
                && sqlServerDefaultValue[sqlServerDefaultValue.Length - 1] == ')')
            {
                sqlServerDefaultValue = sqlServerDefaultValue.Substring(1, sqlServerDefaultValue.Length - 2);
            }

            if (string.IsNullOrEmpty(sqlServerDefaultValue))
            {
                return null;
            }

            propertyType = propertyType.IsNullableType()
                ? Nullable.GetUnderlyingType(propertyType)
                : propertyType;

            if (typeof(string) == propertyType)
            {
                return ConvertSqlServerStringLiteral(sqlServerDefaultValue);
            }

            if (typeof(bool) == propertyType)
            {
                return ConvertSqlServerBitLiteral(sqlServerDefaultValue);
            }

            //TODO: decide what to do about byte[] default values and the values
            // newid() and getdate() for Guid and DateTime respectively

            try
            {
                return Convert.ChangeType(sqlServerDefaultValue, propertyType);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}