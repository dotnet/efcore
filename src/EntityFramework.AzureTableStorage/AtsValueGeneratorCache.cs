// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsValueGeneratorCache : ValueGeneratorCache
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected AtsValueGeneratorCache()
        {
        }

        public AtsValueGeneratorCache(
            [NotNull] ValueGeneratorSelector selector, [NotNull] ForeignKeyValueGenerator foreignKeyValueGenerator)
            : base(selector, foreignKeyValueGenerator)
        {
        }

        public override IValueGenerator GetGenerator(IProperty property)
        {
            //TODO Timestamp? ETag?
            return null;
        }
    }
}
