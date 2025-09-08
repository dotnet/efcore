// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGCodeGenerator : ProviderCodeGenerator
    {
        private static readonly MethodInfo _useXGMethodInfo = typeof(XGDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(XGDbContextOptionsBuilderExtensions.UseXG),
            typeof(DbContextOptionsBuilder),
            typeof(string),
            typeof(ServerVersion),
            typeof(Action<XGDbContextOptionsBuilder>));

        private readonly IXGOptions _options;

        public XGCodeGenerator(
            [NotNull] ProviderCodeGeneratorDependencies dependencies,
            IXGOptions options)
            : base(dependencies)
        {
            _options = options;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override MethodCallCodeFragment GenerateUseProvider(
            string connectionString,
            MethodCallCodeFragment providerOptions)
        {
            // Strip scaffolding specific connection string options first.
            connectionString = new XGScaffoldingConnectionSettings(connectionString).GetProviderCompatibleConnectionString();

            return new MethodCallCodeFragment(
                _useXGMethodInfo,
                providerOptions == null
                    ? new object[] { connectionString, new XGCodeGenerationServerVersionCreation(_options.ServerVersion) }
                    : new object[] { connectionString, new XGCodeGenerationServerVersionCreation(_options.ServerVersion), new NestedClosureCodeFragment("x", providerOptions) });
        }
    }
}
