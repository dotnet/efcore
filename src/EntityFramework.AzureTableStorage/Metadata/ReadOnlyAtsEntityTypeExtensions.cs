// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class ReadOnlyAtsEntityTypeExtensions : IAtsEntityTypeExtensions
    {
        protected const string AtsTableAnnotation = AtsAnnotationNames.Prefix + AtsAnnotationNames.TableName;

        private readonly IEntityType _entityType;

        public ReadOnlyAtsEntityTypeExtensions([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entityType = entityType;
        }

        public virtual string Table
        {
            get { return _entityType[AtsTableAnnotation] ?? _entityType.SimpleName; }
        }

        protected virtual IEntityType EntityType
        {
            get { return _entityType; }
        }
    }
}
