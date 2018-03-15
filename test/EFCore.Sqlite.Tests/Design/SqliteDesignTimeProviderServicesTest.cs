// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class SqliteDesignTimeProviderServicesTest : DesignTimeProviderServicesTest
    {
        protected override Assembly GetRuntimeAssembly()
            => typeof(SqliteRelationalConnection).GetTypeInfo().Assembly;

        protected override Type GetDesignTimeServicesType()
            => typeof(SqliteDesignTimeServices);
    }
}
