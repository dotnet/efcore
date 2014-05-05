// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
