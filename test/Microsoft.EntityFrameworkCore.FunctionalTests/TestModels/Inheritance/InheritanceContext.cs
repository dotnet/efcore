// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.Inheritance
{
    public class InheritanceContext : DbContext
    {
        public static readonly string StoreName = "Inheritance";

        public InheritanceContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }
    }
}
