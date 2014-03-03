// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class PropertyEntry
    {
        private readonly StateEntry _stateEntry;
        private readonly string _name;

        public PropertyEntry([NotNull] StateEntry stateEntry, [NotNull] string name)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotEmpty(name, "name");

            _stateEntry = stateEntry;
            _name = name;
        }

        public virtual bool IsModified
        {
            get { return _stateEntry.IsPropertyModified(_name); }
            set { _stateEntry.SetPropertyModified(_name, value); }
        }

        public virtual string Name
        {
            get { return _name; }
        }
    }
}
