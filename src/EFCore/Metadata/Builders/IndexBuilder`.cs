// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring an <see cref="IMutableIndex" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    // ReSharper disable once UnusedTypeParameter
    public class IndexBuilder<T> : IndexBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public IndexBuilder([NotNull] IMutableIndex index)
            : base(index)
        {
        }

        /// <summary>
        ///     Adds or updates an annotation on the index. If an annotation with the key specified in
        ///     <paramref name="annotation" />
        ///     already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual IndexBuilder<T> HasAnnotation([NotNull] string annotation, [NotNull] object value)
            => (IndexBuilder<T>)base.HasAnnotation(annotation, value);

        /// <summary>
        ///     Configures whether this index is unique (i.e. the value(s) for each instance must be unique).
        /// </summary>
        /// <param name="unique"> A value indicating whether this index is unique. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual IndexBuilder<T> IsUnique(bool unique = true)
            => (IndexBuilder<T>)base.IsUnique(unique);
    }
}
