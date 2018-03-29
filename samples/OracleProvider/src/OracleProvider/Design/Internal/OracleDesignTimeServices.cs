// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Oracle.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Storage.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Oracle.Design.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OracleDesignTimeServices : IDesignTimeServices
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
            => serviceCollection
                .AddSingleton<IRelationalTypeMappingSource, OracleTypeMappingSource>()
                .AddSingleton<IDatabaseModelFactory, OracleDatabaseModelFactory>()
                .AddSingleton<IProviderConfigurationCodeGenerator, OracleCodeGenerator>()
                .AddSingleton<IAnnotationCodeGenerator, OracleAnnotationCodeGenerator>();
    }
}
