// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class PropertyEntry
    {
        private readonly StateEntry _stateEntry;
        private readonly IProperty _property;

        public PropertyEntry([NotNull] StateEntry stateEntry, [NotNull] string name)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotEmpty(name, "name");

            _stateEntry = stateEntry;
            _property = stateEntry.EntityType.GetProperty(name);
        }

        public virtual bool IsModified
        {
            get { return _stateEntry.IsPropertyModified(_property); }
            set { _stateEntry.SetPropertyModified(_property, value); }
        }

        public virtual string Name
        {
            get { return _property.Name; }
        }

        public virtual object CurrentValue
        {
            get { return _stateEntry[_property]; }
            [param: CanBeNull]
            set
            {
                _stateEntry[_property] = value;
            }
        }

        public virtual object OriginalValue
        {
            get { return _stateEntry.OriginalValues[_property]; }
            [param: CanBeNull]
            set
            {
                _stateEntry.OriginalValues[_property] = value;
            }
        }
    }
}
