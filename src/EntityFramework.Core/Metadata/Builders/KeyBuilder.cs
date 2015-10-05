// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a <see cref="Key" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class KeyBuilder : IAccessor<Model>, IAccessor<InternalKeyBuilder>
    {
        private readonly InternalKeyBuilder _builder;

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="KeyBuilder" /> class to configure a given key.
        ///     </para>
        ///     <para>
        ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
        ///         and it is not designed to be directly constructed in your application code.
        ///     </para>
        /// </summary>
        /// <param name="builder"> Internal builder for the key being configured. </param>
        public KeyBuilder([NotNull] InternalKeyBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            _builder = builder;
        }

        /// <summary>
        ///     The internal builder being used to configure the key.
        /// </summary>
        InternalKeyBuilder IAccessor<InternalKeyBuilder>.Service => _builder;

        /// <summary>
        ///     The key being configured.
        /// </summary>
        public virtual Key Metadata => Builder.Metadata;

        /// <summary>
        ///     The model that the key belongs to.
        /// </summary>
        Model IAccessor<Model>.Service => Builder.ModelBuilder.Metadata;

        /// <summary>
        ///     Adds or updates an annotation on the key. If an annotation with the key specified in
        ///     <paramref name="annotation" />
        ///     already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual KeyBuilder HasAnnotation([NotNull] string annotation, [NotNull] object value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotNull(value, nameof(value));

            Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        private InternalKeyBuilder Builder => this.GetService<InternalKeyBuilder>();
    }
}
