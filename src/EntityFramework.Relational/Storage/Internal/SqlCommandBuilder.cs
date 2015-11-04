// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqlCommandBuilder : ISqlCommandBuilder
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

        public SqlCommandBuilder(
            [NotNull] IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            [NotNull] ISqlGenerator sqlGenerator,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory)
        {
            Check.NotNull(relationalCommandBuilderFactory, nameof(relationalCommandBuilderFactory));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));

            _relationalCommandBuilderFactory = relationalCommandBuilderFactory;
            _sqlGenerator = sqlGenerator;
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

                    substitutions[i] = _sqlGenerator.GenerateParameterName(parameterName);

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
