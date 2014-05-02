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
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class OriginalValues : ArraySidecar
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected OriginalValues()
        {
        }

        public OriginalValues([NotNull] StateEntry stateEntry)
            : base(stateEntry, Check.NotNull(stateEntry, "stateEntry").EntityType.OriginalValueCount)
        {
        }

        protected override int Index(IProperty property)
        {
            Check.NotNull(property, "property");

            return property.OriginalValueIndex;
        }

        protected override void ThrowInvalidIndexException(IProperty property)
        {
            Check.NotNull(property, "property");

            throw new InvalidOperationException(Strings.FormatOriginalValueNotTracked(property.Name, StateEntry.EntityType.Name));
        }

        public override string Name
        {
            get { return WellKnownNames.OriginalValues; }
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
