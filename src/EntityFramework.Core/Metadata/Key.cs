// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Key : MetadataBase, IKey
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected Key()
        {
        }

        public Key([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");
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
