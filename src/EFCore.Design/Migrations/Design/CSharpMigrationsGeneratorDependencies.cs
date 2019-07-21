// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="CSharpMigrationsGenerator" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    /// </summary>
    public sealed class CSharpMigrationsGeneratorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="CSharpMigrationsGenerator" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public CSharpMigrationsGeneratorDependencies(
            [NotNull] ICSharpHelper csharpHelper,
            [NotNull] ICSharpMigrationOperationGenerator csharpMigrationOperationGenerator,
            [NotNull] ICSharpSnapshotGenerator csharpSnapshotGenerator)
        {
            CSharpHelper = csharpHelper;
            CSharpMigrationOperationGenerator = csharpMigrationOperationGenerator;
            CSharpSnapshotGenerator = csharpSnapshotGenerator;
        }

        /// <summary>
        ///     The C# helper.
        /// </summary>
        public ICSharpHelper CSharpHelper { get; }

        /// <summary>
        ///     The C# migration operation generator.
        /// </summary>
        public ICSharpMigrationOperationGenerator CSharpMigrationOperationGenerator { get; }

        /// <summary>
        ///     The C# model snapshot generator.
        /// </summary>
        public ICSharpSnapshotGenerator CSharpSnapshotGenerator { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="csharpHelper"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CSharpMigrationsGeneratorDependencies With([NotNull] ICSharpHelper csharpHelper)
            => new CSharpMigrationsGeneratorDependencies(
                csharpHelper,
                CSharpMigrationOperationGenerator,
                CSharpSnapshotGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="csharpMigrationOperationGenerator"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CSharpMigrationsGeneratorDependencies With([NotNull] ICSharpMigrationOperationGenerator csharpMigrationOperationGenerator)
            => new CSharpMigrationsGeneratorDependencies(
                CSharpHelper,
                csharpMigrationOperationGenerator,
                CSharpSnapshotGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="csharpSnapshotGenerator"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CSharpMigrationsGeneratorDependencies With([NotNull] ICSharpSnapshotGenerator csharpSnapshotGenerator)
            => new CSharpMigrationsGeneratorDependencies(
                CSharpHelper,
                CSharpMigrationOperationGenerator,
                csharpSnapshotGenerator);
    }
}
