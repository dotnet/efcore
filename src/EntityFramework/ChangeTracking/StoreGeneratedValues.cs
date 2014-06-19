// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking
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

        public StoreGeneratedValues([NotNull] StateEntry stateEntry, [NotNull] IEnumerable<IProperty> properties)
            : base(stateEntry, properties)
        {
        }

        public override string Name
        {
            get { return WellKnownNames.StoreGeneratedValues; }
        }

        public override bool TransparentRead
        {
            get { return true; }
        }

        public override bool TransparentWrite
        {
            get { return true; }
        }

        public override bool AutoCommit
        {
            get { return true; }
        }
    }
}
