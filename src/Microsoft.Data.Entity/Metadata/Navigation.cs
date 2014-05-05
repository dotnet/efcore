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
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Navigation : NamedMetadataBase, INavigation
    {
        private readonly ForeignKey _foreignKey;

        public Navigation([NotNull] ForeignKey foreignKey, [NotNull] string name)
            : base(Check.NotEmpty(name, "name"))
        {
            Check.NotNull(foreignKey, "foreignKey");

            _foreignKey = foreignKey;
        }

        public virtual EntityType EntityType { get; [param: CanBeNull] set; }

        public virtual ForeignKey ForeignKey
        {
            get { return _foreignKey; }
        }

        IEntityType IPropertyBase.EntityType
        {
            get { return EntityType; }
        }

        IForeignKey INavigation.ForeignKey
        {
            get { return ForeignKey; }
        }
    }
}
