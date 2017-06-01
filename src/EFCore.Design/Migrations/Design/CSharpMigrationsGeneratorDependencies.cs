// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;

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
        /// </summary>
        /// <param name="csharpHelper"> The C# helper. </param>
        /// <param name="csharpMigrationOperationGenerator"> The C# migration operation generator. </param>
        /// <param name="csharpSnapshotGenerator"> The C# model snapshot generator. </param>
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
