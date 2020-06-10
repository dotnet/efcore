// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="CSharpSnapshotGenerator" />
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
    public sealed class CSharpSnapshotGeneratorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="CSharpSnapshotGenerator" />.
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
        public CSharpSnapshotGeneratorDependencies(
            [NotNull] ICSharpHelper csharpHelper,
            [NotNull] IRelationalTypeMappingSource relationalTypeMappingSource,
            [NotNull] IAnnotationCodeGenerator annotationCodeGenerator)
        {
            Check.NotNull(csharpHelper, nameof(csharpHelper));

            CSharpHelper = csharpHelper;
            RelationalTypeMappingSource = relationalTypeMappingSource;
            AnnotationCodeGenerator = annotationCodeGenerator;
        }

        /// <summary>
        ///     The C# helper.
        /// </summary>
        public ICSharpHelper CSharpHelper { get; }

        /// <summary>
        ///     The type mapper.
        /// </summary>
        public IRelationalTypeMappingSource RelationalTypeMappingSource { get; }

        /// <summary>
        ///     The annotation code generator.
        /// </summary>
        public IAnnotationCodeGenerator AnnotationCodeGenerator { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="csharpHelper"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CSharpSnapshotGeneratorDependencies With([NotNull] ICSharpHelper csharpHelper)
            => new CSharpSnapshotGeneratorDependencies(csharpHelper, RelationalTypeMappingSource, AnnotationCodeGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="relationalTypeMappingSource"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CSharpSnapshotGeneratorDependencies With([NotNull] IRelationalTypeMappingSource relationalTypeMappingSource)
            => new CSharpSnapshotGeneratorDependencies(CSharpHelper, relationalTypeMappingSource, AnnotationCodeGenerator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="annotationCodeGenerator"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CSharpSnapshotGeneratorDependencies With([NotNull] IAnnotationCodeGenerator annotationCodeGenerator)
            => new CSharpSnapshotGeneratorDependencies(CSharpHelper, RelationalTypeMappingSource, annotationCodeGenerator);
    }
}
