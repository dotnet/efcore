// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     <para>
    ///         Extension of <see cref="RelationalAnnotations"/> that allow <see cref="ConfigurationSource"/>
    ///         to be used with annotations.
    ///    </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalAnnotationsBuilder : RelationalAnnotations, IInfrastructure<InternalAnnotatableBuilder>
    {
        /// <summary>
        ///     <para>
        ///         Constructs a new builder instance.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="internalBuilder"> The internal builder to use. </param>
        /// <param name="configurationSource"> The configuration source. </param>
        public RelationalAnnotationsBuilder(
            [NotNull] InternalAnnotatableBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(Check.NotNull(internalBuilder, nameof(internalBuilder)).Metadata)

        {
            AnnotatableBuilder = internalBuilder;
            ConfigurationSource = configurationSource;
        }

        /// <summary>
        ///     <para>
        ///         Returns the <see cref="ConfigurationSource"/> being used.
        ///     </para>
        ///     <para>
        ///         This property is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        public virtual ConfigurationSource ConfigurationSource { get; }

        /// <summary>
        ///     <para>
        ///         Returns the <see cref="InternalAnnotatableBuilder"/> being used.
        ///     </para>
        ///     <para>
        ///         This property is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalAnnotatableBuilder AnnotatableBuilder { get; }

        /// <summary>
        ///     Attempts to set an annotation with the given name to the given value and
        ///     returns whether or not this was successful.
        /// </summary>
        /// <param name="annotationName"> The name of the annotation to set. </param>
        /// <param name="value"> The value to set. </param>
        /// <returns><c>true</c> if the annotation was set; <c>false</c> otherwise. </returns>
        public override bool SetAnnotation(
            string annotationName,
            object value)
            => AnnotatableBuilder.HasAnnotation(annotationName, value, ConfigurationSource) != null;

        /// <summary>
        ///     Checks whether or not the annotation with the given name can be set to the given value.
        /// </summary>
        /// <param name="annotationName"> The name of the annotation to set. </param>
        /// <param name="value"> The value to set. </param>
        /// <returns><c>true</c> if the annotation can be set; <c>false</c> otherwise. </returns>
        public override bool CanSetAnnotation(
            string annotationName,
            object value)
            => AnnotatableBuilder.CanSetAnnotation(annotationName, value, ConfigurationSource);

        /// <summary>
        ///     Attempts to remove an annotation with the given name and
        ///     returns whether or not this was successful.
        /// </summary>
        /// <param name="annotationName"> The name of the annotation to remove. </param>
        /// <returns><c>true</c> if the annotation was removed; <c>false</c> otherwise. </returns>
        public override bool RemoveAnnotation(string annotationName)
            => AnnotatableBuilder.RemoveAnnotation(annotationName, ConfigurationSource) != null;

        InternalAnnotatableBuilder IInfrastructure<InternalAnnotatableBuilder>.Instance => AnnotatableBuilder;
    }
}
