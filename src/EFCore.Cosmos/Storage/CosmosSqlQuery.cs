// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage
{
    public class CosmosSqlQuery
    {
        public CosmosSqlQuery(string query, IReadOnlyList<SqlParameter> parameters)
        {
            Query = query;
            Parameters = parameters;
        }

        [JsonProperty("query", Required = Required.Always)]
        public string Query { get; }

        [JsonProperty("parameters", Required = Required.Always)]
        public IReadOnlyList<SqlParameter> Parameters { get; }
    }
}
