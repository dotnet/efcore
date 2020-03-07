// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A base type for conventions that perform configuration based on an attribute specified on an entity type.
    /// </summary>
    /// <typeparam name="TAttribute"> The attribute type to look for. </typeparam>
    public abstract class EntityTypeAttributeConventionBase<TAttribute> : IEntityTypeAddedConvention
        where TAttribute : Attribute
    {
        /// <summary>
        ///     Creates a new instance of <see cref="EntityTypeAttributeConventionBase{TAttribute}" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        protected EntityTypeAttributeConventionBase([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var type = entityTypeBuilder.Metadata.ClrType;
            if (type == null
                || !Attribute.IsDefined(type, typeof(TAttribute), inherit: true))
            {
                return;
            }

            var attributes = type.GetTypeInfo().GetCustomAttributes<TAttribute>(true);

            foreach (var attribute in attributes)
            {
                ProcessEntityTypeAdded(entityTypeBuilder, attribute, context);
                if (((IReadableConventionContext)context).ShouldStopProcessing())
                {
                    return;
                }
            }
        }

        /// <summary>
        ///     Called after an entity type is added to the model if it has an attribute.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        protected abstract void ProcessEntityTypeAdded(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<IConventionEntityTypeBuilder> context);
    }
}
