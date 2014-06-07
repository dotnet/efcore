// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    // TODO: Consider using ArraySidecar with pre-defined indexes
    public class ForeignKeysSnapshot : DictionarySidecar
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ForeignKeysSnapshot()
        {
        }

        public ForeignKeysSnapshot([NotNull] StateEntry stateEntry)
            : base(stateEntry, Check.NotNull(stateEntry, "stateEntry").EntityType.ForeignKeys.SelectMany(fk => fk.Properties).Distinct())
        {
        }

        public override string Name
        {
            get { return WellKnownNames.ForeignKeysSnapshot; }
        }

        public override bool TransparentRead
        {
            get { return false; }
        }

        public override bool TransparentWrite
        {
            get { return false; }
        }

        public override bool AutoCommit
        {
            get { return false; }
        }
    }
}
