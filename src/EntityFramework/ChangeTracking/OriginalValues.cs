// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        protected override int Index(IPropertyBase property)
        {
            Check.NotNull(property, "property");

            var asProperty = property as IProperty;

            return asProperty != null ? asProperty.OriginalValueIndex : -1;
        }

        protected override void ThrowInvalidIndexException(IPropertyBase property)
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
