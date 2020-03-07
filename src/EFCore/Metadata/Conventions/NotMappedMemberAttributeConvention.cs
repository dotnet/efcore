// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that ignores members on entity types that have the <see cref="NotMappedAttribute" />.
    /// </summary>
    public class NotMappedMemberAttributeConvention : IEntityTypeAddedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NotMappedMemberAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public NotMappedMemberAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
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
            IConventionEntityTypeBuilder entityTypeBuilder, IConventionContext<IConventionEntityTypeBuilder> context)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var entityType = entityTypeBuilder.Metadata;
            if (!entityType.HasClrType())
            {
                return;
            }

            var members = entityType.GetRuntimeProperties().Values.Cast<MemberInfo>()
                .Concat(entityType.GetRuntimeFields().Values);

            foreach (var member in members)
            {
                if (Attribute.IsDefined(member, typeof(NotMappedAttribute), inherit: true))
                {
                    entityTypeBuilder.Ignore(member.GetSimpleMemberName(), fromDataAnnotation: true);
                }
            }
        }
    }
}
