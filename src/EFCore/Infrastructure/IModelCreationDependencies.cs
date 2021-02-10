// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IModelCreationDependencies
    {
        /// <summary>
        ///     The model source.
        /// </summary>
        public IModelSource ModelSource { get; }

        /// <summary>
        ///     The convention set to use when creating the model.
        /// </summary>
        public IConventionSetBuilder ConventionSetBuilder { get; }

        /// <summary>
        ///     The dependencies object for the model.
        /// </summary>
        public ModelDependencies ModelDependencies { get; }

        /// <summary>
        ///     The model runtime initializer that will be used after the model building is finished.
        /// </summary>
        public IModelRuntimeInitializer ModelRuntimeInitializer { get; }

        /// <summary>
        ///     The validation logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Model.Validation> ValidationLogger { get; }
    }
}
