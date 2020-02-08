// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class RawSqlCommandBuilder : IRawSqlCommandBuilder
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RawSqlCommandBuilder(
            [NotNull] IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory)
        {
            Check.NotNull(relationalCommandBuilderFactory, nameof(relationalCommandBuilderFactory));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));

            _relationalCommandBuilderFactory = relationalCommandBuilderFactory;
            _sqlGenerationHelper = sqlGenerationHelper;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IRelationalCommand Build(string sql)
            => _relationalCommandBuilderFactory
                .Create()
                .Append(Check.NotEmpty(sql, nameof(sql)))
                .Build();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual RawSqlCommand Build(string sql, IEnumerable<object> parameters)
        {
            var relationalCommandBuilder = _relationalCommandBuilderFactory.Create();

            var substitutions = new List<string>();

            var parameterNameGenerator = _parameterNameGeneratorFactory.Create();

            var parameterValues = new Dictionary<string, object>();

            foreach (var parameter in parameters)
            {
                if (parameter is DbParameter dbParameter)
                {
                    if (string.IsNullOrEmpty(dbParameter.ParameterName))
                    {
                        dbParameter.ParameterName = _sqlGenerationHelper.GenerateParameterName(parameterNameGenerator.GenerateNext());
                    }

                    substitutions.Add(_sqlGenerationHelper.GenerateParameterName(dbParameter.ParameterName));
                    relationalCommandBuilder.AddRawParameter(dbParameter.ParameterName, dbParameter);
                }
                else
                {
                    var parameterName = parameterNameGenerator.GenerateNext();
                    var substitutedName = _sqlGenerationHelper.GenerateParameterName(parameterName);

                    substitutions.Add(substitutedName);
                    relationalCommandBuilder.AddParameter(parameterName, substitutedName);
                    parameterValues.Add(parameterName, parameter);
                }
            }

            // ReSharper disable once CoVariantArrayConversion
            sql = string.Format(sql, substitutions.ToArray());

            return new RawSqlCommand(
                relationalCommandBuilder.Append(sql).Build(),
                parameterValues);
        }
    }
}
