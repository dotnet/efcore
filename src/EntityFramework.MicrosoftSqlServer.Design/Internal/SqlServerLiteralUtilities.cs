// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class SqlServerLiteralUtilities
    {
        public static readonly Regex _defaultValueIsExpression =
            new Regex(@"^[@\$\w]+\(.*\)$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(1000.0));

        public SqlServerLiteralUtilities(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] TSqlConversionUtilities tSqlConversionUtilities)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            Logger = loggerFactory.CreateCommandsLogger();
            TSqlConversionUtilities = tSqlConversionUtilities;
        }

        public virtual ILogger Logger { get; }
        public virtual TSqlConversionUtilities TSqlConversionUtilities { get; }

        public virtual DefaultExpressionOrValue ConvertSqlServerDefaultValue(
            [NotNull] Type propertyType, [NotNull] string sqlServerDefaultValue)
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

            if (_defaultValueIsExpression.IsMatch(sqlServerDefaultValue))
            {
                return new DefaultExpressionOrValue
                {
                    DefaultExpression = sqlServerDefaultValue
                };
            }

            if (propertyType.IsNullableType()
                && TSqlConversionUtilities.IsLiteralNull(sqlServerDefaultValue))
            {
                return new DefaultExpressionOrValue()
                {
                    DefaultValue = null
                };
            }

            propertyType = propertyType.UnwrapNullableType();

            if (typeof(DateTime) == propertyType)
            {
                return new DefaultExpressionOrValue
                {
                    DefaultExpression = sqlServerDefaultValue
                };
            }

            if (typeof(string) == propertyType)
            {
                var defaultValue = TSqlConversionUtilities
                        .ConvertStringLiteral(sqlServerDefaultValue);
                if (defaultValue == null)
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.CannotInterpretSqlServerStringLiteral(sqlServerDefaultValue));
                }

                return new DefaultExpressionOrValue
                {
                    DefaultValue = defaultValue
                };
            }

            if (typeof(bool) == propertyType)
            {
                return new DefaultExpressionOrValue
                {
                    DefaultValue = TSqlConversionUtilities
                        .ConvertBitLiteral(sqlServerDefaultValue)
                };
            }

            if (typeof(Guid) == propertyType)
            {
                var defaultValue = TSqlConversionUtilities
                        .ConvertStringLiteral(sqlServerDefaultValue);
                if (defaultValue == null)
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.CannotInterpretSqlServerStringLiteral(sqlServerDefaultValue));
                }

                return new DefaultExpressionOrValue
                {
                    DefaultValue = new Guid(defaultValue)
                };
            }

            //TODO: decide what to do about byte[] default values

            try
            {
                return new DefaultExpressionOrValue
                {
                    DefaultValue = Convert.ChangeType(sqlServerDefaultValue, propertyType)
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class DefaultExpressionOrValue
    {
        public virtual string DefaultExpression { get;[param: NotNull] set; }
        public virtual object DefaultValue { get;[param: NotNull] set; }
    }
}
