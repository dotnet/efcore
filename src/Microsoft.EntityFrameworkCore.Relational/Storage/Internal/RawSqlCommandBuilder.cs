// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RawSqlCommandBuilder : IRawSqlCommandBuilder
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IRelationalCommand Build(string sql)
            => _relationalCommandBuilderFactory
                .Create()
                .Append(Check.NotEmpty(sql, nameof(sql)))
                .Build();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual RawSqlCommand Build(string sql, IReadOnlyList<object> parameters)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            var relationalCommandBuilder = _relationalCommandBuilderFactory.Create();

            var substitutions = new string[parameters.Count];

            var parameterNameGenerator = _parameterNameGeneratorFactory.Create();

            var parameterValues = new Dictionary<string, object>();

            for (var i = 0; i < substitutions.Length; i++)
            {
                var parameterName = parameterNameGenerator.GenerateNext();

                substitutions[i] = _sqlGenerationHelper.GenerateParameterName(parameterName);

                relationalCommandBuilder.AddParameter(
                    parameterName,
                    substitutions[i]);

                parameterValues.Add(parameterName, parameters[i]);
            }

            // ReSharper disable once CoVariantArrayConversion
            sql = string.Format(sql, substitutions);

            return new RawSqlCommand(
                relationalCommandBuilder.Append(sql).Build(),
                parameterValues);
        }
    }
}
