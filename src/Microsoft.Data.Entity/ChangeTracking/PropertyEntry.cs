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
        }
    }
}
