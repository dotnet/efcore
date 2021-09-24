// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that ignores members on entity types that have the <see cref="NotMappedAttribute" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information.
    /// </remarks>
    public class NotMappedMemberAttributeConvention : IEntityTypeAddedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NotMappedMemberAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
        public NotMappedMemberAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type.</param>
        /// <param name="context">Additional information associated with convention execution.</param>
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var entityType = entityTypeBuilder.Metadata;
            var members = entityType.GetRuntimeProperties().Values.Cast<MemberInfo>()
                .Concat(entityType.GetRuntimeFields().Values);

            foreach (var member in members)
            {
                if (Attribute.IsDefined(member, typeof(NotMappedAttribute), inherit: true)
                    && ShouldIgnore(member))
                {
                    entityTypeBuilder.Ignore(member.GetSimpleMemberName(), fromDataAnnotation: true);
                }
            }
        }

        /// <summary>
        ///     Returns a value indicating whether the given CLR member should be ignored.
        /// </summary>
        /// <param name="memberInfo">The member.</param>
        /// <returns><see langword="true" /> if the member should be ignored.</returns>
        protected virtual bool ShouldIgnore(MemberInfo memberInfo)
            => true;
    }
}
