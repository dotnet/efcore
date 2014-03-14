// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Key : MetadataBase, IKey
    {
        private readonly IReadOnlyList<Property> _properties;

        public Key([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");
            CheckSameEntityType(properties, "properties");

            _properties = properties;
        }
        
        public virtual IReadOnlyList<Property> Properties
        {
            get { return _properties; }
        }

        IReadOnlyList<IProperty> IKey.Properties
        {
            get { return Properties; }
        }

        internal static void CheckSameEntityType(IReadOnlyList<Property> properties, string argumentName)
        {
            if (properties.Count > 1)
            {
                var entityType = properties[0].EntityType;

                for (var i = 1; i < properties.Count; i++)
                {
                    if (properties[i].EntityType != entityType)
                    {
                        throw new ArgumentException(
                            Strings.FormatInconsistentEntityType(argumentName));
                    }
                }
            }
        }
    }
}
