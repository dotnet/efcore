// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Specifies the configuration type for the entity type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntityTypeConfigurationAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityTypeConfigurationAttribute" /> class.
        /// </summary>
        /// <param name="entityConfigurationType"> The IEntityTypeConfiguration&lt;&gt; type to use. </param>
        public EntityTypeConfigurationAttribute(Type entityConfigurationType)
        {
            Check.NotNull(entityConfigurationType, nameof(entityConfigurationType));

            EntityTypeConfigurationType = entityConfigurationType;
        }

        /// <summary>
        ///     Type of the entity type configuration.
        /// </summary>
        public Type EntityTypeConfigurationType { get; }
    }
}
