// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDatabaseBuilder : DatabaseBuilder
    {
        protected override Sequence BuildSequence(IProperty property)
        {
            Check.NotNull(property, "property");

            return property.BuildSequence();
        }
    }
}
