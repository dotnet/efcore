// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Key : Annotatable, IKey
    {
        public Key([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, "properties");
            MetadataHelper.CheckSameEntityType(properties, "properties");

            Properties = properties;
        }

        [NotNull]
        public virtual IReadOnlyList<Property> Properties { get; }

        public virtual EntityType EntityType
        {
            get { return Properties[0].EntityType; }
        }

        IReadOnlyList<IProperty> IKey.Properties
        {
            get { return Properties; }
        }

        IEntityType IKey.EntityType
        {
            get { return EntityType; }
        }
    }
}
