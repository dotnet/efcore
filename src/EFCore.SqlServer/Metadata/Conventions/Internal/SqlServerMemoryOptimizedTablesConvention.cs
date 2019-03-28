// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerMemoryOptimizedTablesConvention : IEntityTypeAnnotationChangedConvention, IKeyAddedConvention,
        IIndexAddedConvention
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerMemoryOptimizedTablesConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     Called after an annotation is changed on an entity type.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAnnotationChanged(
            IConventionEntityTypeBuilder entityTypeBuilder, string name, IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation, IConventionContext<IConventionAnnotation> context)
        {
            if (name == SqlServerAnnotationNames.MemoryOptimized)
            {
                var memoryOptimized = annotation?.Value as bool? == true;
                foreach (var key in entityTypeBuilder.Metadata.GetDeclaredKeys())
                {
                    key.Builder.ForSqlServerIsClustered(memoryOptimized ? false : (bool?)null);
                }

                foreach (var index in
                    entityTypeBuilder.Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredIndexes()))
                {
                    index.Builder.ForSqlServerIsClustered(memoryOptimized ? false : (bool?)null);
                }
            }
        }

        /// <summary>
        ///     Called after a key is added to the entity type.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessKeyAdded(IConventionKeyBuilder keyBuilder, IConventionContext<IConventionKeyBuilder> context)
        {
            if (keyBuilder.Metadata.DeclaringEntityType.GetSqlServerIsMemoryOptimized())
            {
                keyBuilder.ForSqlServerIsClustered(false);
            }
        }

        /// <summary>
        ///     Called after an index is added to the entity type.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessIndexAdded(IConventionIndexBuilder indexBuilder, IConventionContext<IConventionIndexBuilder> context)
        {
            if (indexBuilder.Metadata.DeclaringEntityType.GetAllBaseTypesInclusive().Any(et => et.GetSqlServerIsMemoryOptimized()))
            {
                indexBuilder.ForSqlServerIsClustered(false);
            }
        }
    }
}
