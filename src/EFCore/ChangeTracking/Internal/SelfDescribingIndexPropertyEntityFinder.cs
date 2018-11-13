// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SelfDescribingIndexPropertyEntityFinder : ISharedTypeEntityFinder
    {
        public const string DefaultEntityTypeNamePropertyName = "__EntityTypeName__";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SelfDescribingIndexPropertyEntityFinder([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            Model = model;
            EntityTypeNamePropertyName = DefaultEntityTypeNamePropertyName;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string EntityTypeNamePropertyName { get; [NotNull]set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IModel Model { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// <returns> the shared-type EntityType corresponding to this instance if found in the model, or null if not. </returns>
        public virtual IEntityType FindSharedTypeEntityType(object entityInstance)
        {
            Check.NotNull(entityInstance, nameof(entityInstance));

            var efIndexerPropInfo = entityInstance.GetType()
                .GetRuntimeProperties().FirstOrDefault(p => p.IsEFIndexerProperty());
            if (efIndexerPropInfo == null)
            {
                return null;
            }

            string entityTypeName = null;
            try
            {
                entityTypeName = efIndexerPropInfo
                    .GetValue(entityInstance, new[] { EntityTypeNamePropertyName }) as string;
            }
            catch
            {
                // exceptions indicate the indexer does not have the
                // EntityTypeNamePropertyName key etc.
            }

            var entityType = entityTypeName == null
                ? null
                : Model.FindEntityType(entityTypeName);

            return entityType != null && entityType.IsSharedType ? entityType : null;
        }
    }
}
