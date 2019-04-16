// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    public class SqlParameter
    {
        public SqlParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; }

        [JsonProperty("value", Required = Required.AllowNull)]
        public object Value { get; }
    }
}
