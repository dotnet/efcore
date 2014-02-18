// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class PropertyEntry
    {
        private readonly ChangeTrackerEntry _entityEntry;
        private readonly string _name;

        public PropertyEntry([NotNull] EntityEntry entityEntry, [NotNull] string name)
        {
            Check.NotNull(entityEntry, "entityEntry");
            Check.NotEmpty(name, "name");

            _entityEntry = entityEntry.Entry;
            _name = name;
        }

        public virtual bool IsModified
        {
            get { return _entityEntry.IsPropertyModified(_name); }
            set { _entityEntry.SetPropertyModified(_name, value); }
        }

        public virtual string Name
        {
            get { return _name; }
        }
    }
}
