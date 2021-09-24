// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that adds service properties to entity types.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information.
    /// </remarks>
    public class ServicePropertyDiscoveryConvention :
        IEntityTypeAddedConvention,
        IEntityTypeBaseTypeChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ServicePropertyDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
        public ServicePropertyDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
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
            => Process(entityTypeBuilder);

        /// <summary>
        ///     Called after the base type of an entity type changes.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type.</param>
        /// <param name="newBaseType">The new base entity type.</param>
        /// <param name="oldBaseType">The old base entity type.</param>
        /// <param name="context">Additional information associated with convention execution.</param>
        public virtual void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType? newBaseType,
            IConventionEntityType? oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            if (entityTypeBuilder.Metadata.BaseType == newBaseType)
            {
                Process(entityTypeBuilder);
            }
        }

        private void Process(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var model = entityType.Model;
            foreach (var propertyInfo in entityType.GetRuntimeProperties().Values)
            {
                if (!entityTypeBuilder.CanHaveServiceProperty(propertyInfo))
                {
                    continue;
                }

                var factory = Dependencies.MemberClassifier.FindServicePropertyCandidateBindingFactory(propertyInfo, model);
                if (factory == null)
                {
                    continue;
                }

                entityTypeBuilder.ServiceProperty(propertyInfo)?.HasParameterBinding(
                    (ServiceParameterBinding)factory.Bind(entityType, propertyInfo.PropertyType, propertyInfo.GetSimpleMemberName()));
            }
        }
    }
}
