// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Scaffolding.Internal
{
    public class XGJsonNewtonsoftCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
    {
        private static readonly MethodInfo _useNewtonsoftJsonMethodInfo =
            typeof(XGJsonNewtonsoftDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGJsonNewtonsoftDbContextOptionsBuilderExtensions.UseNewtonsoftJson),
                typeof(XGDbContextOptionsBuilder),
                typeof(XGCommonJsonChangeTrackingOptions));

        public override MethodCallCodeFragment GenerateProviderOptions()
            => new MethodCallCodeFragment(_useNewtonsoftJsonMethodInfo);
    }
}
