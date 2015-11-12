// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class RawSqlCommandBuilder : IRawSqlCommandBuilder
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

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

        public virtual IRelationalCommand Build(string sql, IReadOnlyList<object> parameters = null)
        {
            Check.NotEmpty(sql, nameof(sql));

            var relationalCommandBuilder = _relationalCommandBuilderFactory.Create();

            if (parameters != null)
            {
                var substitutions = new string[parameters.Count];

                var parameterNameGenerator = _parameterNameGeneratorFactory.Create();

                for (var i = 0; i < substitutions.Length; i++)
                {
                    var parameterName = parameterNameGenerator.GenerateNext();

                    substitutions[i] = _sqlGenerationHelper.GenerateParameterName(parameterName);

                    relationalCommandBuilder.AddParameter(
                        substitutions[i],
                        parameters[i],
                        parameterName);
                }

                // ReSharper disable once CoVariantArrayConversion
                sql = string.Format(sql, substitutions);
            }

            return relationalCommandBuilder.Append(sql).Build();
        }
    }
}
