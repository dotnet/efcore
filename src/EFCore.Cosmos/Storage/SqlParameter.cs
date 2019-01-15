// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage
{
    public class SqlParameter
    {
        public SqlParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; }
    }
}
