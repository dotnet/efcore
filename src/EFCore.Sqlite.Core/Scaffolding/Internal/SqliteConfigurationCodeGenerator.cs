// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteConfigurationCodeGenerator : ProviderConfigurationCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteConfigurationCodeGenerator" /> class.
        /// </summary>
        /// <param name="dependencies"> The dependencies. </param>
        public SqliteConfigurationCodeGenerator([NotNull] ProviderConfigurationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override MethodCallCodeFragment GenerateUseProvider(string connectionString)
            => new MethodCallCodeFragment(nameof(SqliteDbContextOptionsBuilderExtensions.UseSqlite), connectionString);
    }
}
