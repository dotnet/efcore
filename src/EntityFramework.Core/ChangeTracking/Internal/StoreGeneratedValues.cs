// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class StoreGeneratedValues : DictionarySidecar
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StoreGeneratedValues()
        {
        }

        public StoreGeneratedValues([NotNull] InternalEntityEntry entry, [NotNull] IEnumerable<IProperty> properties)
            : base(entry, properties)
        {
        }

        public override string Name => WellKnownNames.StoreGeneratedValues;

        public override bool TransparentRead => true;

        public override bool TransparentWrite => true;

        public override bool AutoCommit => true;
    }
}
