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

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected Key()
        {
        }

        internal Key([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");
            CheckSameEntityType(properties, "properties");

            _properties = properties;
        }

        public virtual string StorageName { get; [param: CanBeNull] set; }

        public virtual IReadOnlyList<Property> Properties
        {
            get { return _properties; }
        }

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
