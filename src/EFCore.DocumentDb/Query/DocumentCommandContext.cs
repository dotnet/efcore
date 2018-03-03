// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DocumentCommandContext
    {
        private readonly DocumentClient _documentClient;
        private readonly string _collectionUri;
        private readonly Func<QuerySqlGenerator> _sqlGeneratorFunc;

        private IDictionary<string, string> _parametersToInclude;
        private SqlQuerySpec _sqlQuerySpec;


        public DocumentCommandContext(
            DocumentClient documentClient, string collectionUri, Func<QuerySqlGenerator> sqlGeneratorFunc)
        {
            _documentClient = documentClient;
            _collectionUri = collectionUri;
            _sqlGeneratorFunc = sqlGeneratorFunc;
        }

        private struct DocumentQuerySpec
        {
            private readonly string _sqlText;
            private readonly IDictionary<string, string> _parameters;
            private readonly ValueBufferFactory _valueBufferFactory;

            public DocumentQuerySpec(string sqlText, IDictionary<string, string> parameters, ValueBufferFactory valueBufferFactory)
            {
                _sqlText = sqlText;
                _parameters = parameters;
                _valueBufferFactory = valueBufferFactory;
            }

            public string SqlText => _sqlText;

            public ValueBufferFactory ValueBufferFactory => _valueBufferFactory;

            public IDictionary<string, string> Parameters => _parameters;
        }

        public ValueBufferFactory ValueBufferFactory { get; private set; }

        public string CollectionUri => _collectionUri;

        public QuerySqlGenerator GetSqlGenerator()
        {
            return _sqlGeneratorFunc();
        }

        public SqlQuerySpec GetSqlQuerySpec(IReadOnlyDictionary<string, object> parameterValues)
        {
            if (_sqlQuerySpec == null)
            {
                var generator = _sqlGeneratorFunc();
                ValueBufferFactory = generator.CreateValueBufferFactory();
                _sqlQuerySpec = new SqlQuerySpec(generator.GenerateSql(), new SqlParameterCollection());
                _parametersToInclude = generator.ParametersToInclude;
            }

            if (_parametersToInclude.Count == 0)
            {
                return _sqlQuerySpec;
            }

            _sqlQuerySpec.Parameters.Clear();

            foreach (var parameter in _parametersToInclude)
            {
                if (!parameterValues.TryGetValue(parameter.Key, out var value))
                {
                    throw new InvalidOperationException("parameter missing");
                }

                _sqlQuerySpec.Parameters.Add(new SqlParameter(parameter.Value, value));
            }

            return _sqlQuerySpec;
        }
    }
}
