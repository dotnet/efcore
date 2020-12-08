// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///  Specifies an entity configuration to the entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntityConfigurationAttribute : Attribute
    {
        private readonly Type _entityConfigurationType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityConfigurationAttribute" /> class.
        /// </summary>
        /// <param name="entityConfigurationType"> The entity configuration type created for the entity. </param>
        public EntityConfigurationAttribute(Type entityConfigurationType)
        {
            _entityConfigurationType = entityConfigurationType;
        }   

        /// <summary>
        /// Type of entity type configuration
        /// </summary>
        public Type EntityConfigurationType
        {
            get
            {
                return _entityConfigurationType;
            }
        }        
    }
}
