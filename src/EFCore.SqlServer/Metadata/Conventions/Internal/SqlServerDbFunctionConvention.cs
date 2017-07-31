// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class SqlServerDbFunctionConvention : RelationalDbFunctionConvention
    {
        protected override void ApplyCustomizations(InternalModelBuilder modelBuilder, string name, Annotation annotation)
        {
            base.ApplyCustomizations(modelBuilder, name, annotation);

            ((DbFunction)annotation.Value).DefaultSchema = "dbo";
        }
    }
}
