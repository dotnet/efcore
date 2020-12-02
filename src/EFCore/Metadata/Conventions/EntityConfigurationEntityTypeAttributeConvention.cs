// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    /// A convention that apply specified enity type configuration <see cref="EntityConfigurationAttribute" />.
    /// </summary>
    public class EntityConfigurationEntityTypeAttributeConvention : EntityTypeAttributeConventionBase<EntityConfigurationAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="EntityConfigurationEntityTypeAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public EntityConfigurationEntityTypeAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called after an entity type is added to the model if it has an attribute.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        protected override void ProcessEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder,
            EntityConfigurationAttribute attribute,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            var entityTypeConfiguration = attribute.EntityConfigurationType;

            var entityTypeBuilderInstance = GetEntityBuilderInstance(entityTypeBuilder.Metadata, entityTypeConfiguration);

            var instance = Activator.CreateInstance(entityTypeConfiguration);

            MethodInfo method = entityTypeConfiguration.GetMethod("Configure");
            method.Invoke(instance, new object[] { entityTypeBuilderInstance });
        }

        private Type[] GetEntityTypeArgs(Type entityTypeConfiguration)
        {
            var interfaces = entityTypeConfiguration.GetInterfaces()
                .ToList()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));

            var types = interfaces
                .Select(x => x.GetGenericArguments().First())
                .ToArray();

            return types;
        }

        private object GetEntityBuilderInstance(IConventionEntityType conventionEntityType, Type entityTypeConfiguration)
        {
            Type[] entityTypeArgs = GetEntityTypeArgs(entityTypeConfiguration);

            var entityTypeBuilder = typeof(EntityTypeBuilder<>);

            Type constructed = entityTypeBuilder.MakeGenericType(entityTypeArgs);

            var entityTypeBuilderInstance = Activator.CreateInstance(constructed, new object[] { conventionEntityType });

            return entityTypeBuilderInstance;
        }
    }
}
