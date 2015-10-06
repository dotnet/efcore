// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqlCommandBuilder : ISqlCommandBuilder
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

        public SqlCommandBuilder(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerator sqlGenerator,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));

            _commandBuilderFactory = commandBuilderFactory;
            _sqlGenerator = sqlGenerator;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
        }

        public virtual IRelationalCommand Build(
            [NotNull] string sql,
            [CanBeNull] IReadOnlyList<object> parameters = null)
        {
            Check.NotEmpty(sql, nameof(sql));

            var builder = _commandBuilderFactory.Create();

            if (parameters != null)
            {
                var substitutions = new string[parameters.Count];

                var parameterNameGenerator = _parameterNameGeneratorFactory.Create();

                for (var index = 0; index < substitutions.Length; index++)
                {
                    substitutions[index] =
                        _sqlGenerator.GenerateParameterName(
                            parameterNameGenerator.GenerateNext());

                    builder.AddParameter(
                        substitutions[index],
                        parameters[index]);
                }

                sql = string.Format(sql, substitutions);
            }

            return builder.Append(sql).BuildRelationalCommand();
        }
    }
}
