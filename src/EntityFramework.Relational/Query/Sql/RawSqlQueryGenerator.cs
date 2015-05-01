// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public class RawSqlQueryGenerator : ISqlQueryGenerator
    {
        private readonly List<CommandParameter> _commandParameters;
        private readonly string _sql;
        private readonly object[] _inputParameters;

        public RawSqlQueryGenerator([NotNull] string sql, [NotNull] object[] parameters)
        {
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            _sql = sql;
            _inputParameters = parameters;

            _commandParameters = new List<CommandParameter>();
        }

        public virtual IReadOnlyList<CommandParameter> Parameters => _commandParameters;

        protected virtual string ParameterPrefix => "@";

        public virtual string GenerateSql([NotNull] IDictionary<string, object> parameterValues)
        {
            Check.NotNull(parameterValues, nameof(parameterValues));

            _commandParameters.Clear();

            var substitutions = new string[_inputParameters.Length];

            for (var index = 0; index < _inputParameters.Length; index++)
            {
                var parameterName = "p" + index;

                _commandParameters.Add(new CommandParameter(parameterName, _inputParameters[index]));
                substitutions[index] = ParameterPrefix + parameterName;
            }

            return string.Format(_sql, substitutions);
        }
    }
}
