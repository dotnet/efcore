// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that adds service properties to entity types.
    /// </summary>
    public class ServicePropertyDiscoveryConvention :
        IEntityTypeAddedConvention,
        IEntityTypeBaseTypeChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ServicePropertyDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ServicePropertyDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
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
            => Process(entityTypeBuilder);

        /// <summary>
        ///     Called after the base type of an entity type changes.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newBaseType"> The new base entity type. </param>
        /// <param name="oldBaseType"> The old base entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
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
            var candidates = entityType.GetRuntimeProperties().Values;

            foreach (var propertyInfo in candidates)
            {
                var name = propertyInfo.GetSimpleMemberName();
                if (entityTypeBuilder.IsIgnored(name)
                    || entityType.FindProperty(propertyInfo) != null
                    || entityType.FindNavigation(propertyInfo) != null
                    || !propertyInfo.IsCandidateProperty(publicOnly: false)
                    || (propertyInfo.IsCandidateProperty()
                        && Dependencies.TypeMappingSource.FindMapping(propertyInfo) != null))
                {
                    continue;
                }

                var factory = Dependencies.ParameterBindingFactories.FindFactory(propertyInfo.PropertyType, name);
                if (factory == null)
                {
                    continue;
                }

                entityTypeBuilder.ServiceProperty(propertyInfo)?.HasParameterBinding(
                    (ServiceParameterBinding)factory.Bind(entityType, propertyInfo.PropertyType, name));
            }
        }
    }
}
