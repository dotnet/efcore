// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Index : Annotatable, IIndex
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected Index()
        {
        }

        public Index([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, "properties");
            MetadataHelper.CheckSameEntityType(properties, "properties");

            Properties = properties;
        }

        public virtual bool? IsUnique { get; set; }

        protected virtual bool DefaultIsUnique
        {
            get { return false; }
        }

        public virtual IReadOnlyList<Property> Properties { get; }

        public virtual EntityType EntityType
        {
            get { return Properties[0].EntityType; }
        }

        IReadOnlyList<IProperty> IIndex.Properties
        {
            get { return Properties; }
        }

        IEntityType IIndex.EntityType
        {
            get { return EntityType; }
        }

        bool IIndex.IsUnique
        {
            get { return IsUnique ?? DefaultIsUnique; }
        }
    }
}
