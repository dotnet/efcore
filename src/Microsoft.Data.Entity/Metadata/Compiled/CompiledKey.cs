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

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledKey : NoAnnotations
    {
        private readonly IModel _model;

        protected CompiledKey(IModel model)
        {
            _model = model;
        }

        protected abstract KeyDefinition Definition { get; }

        public IEntityType EntityType
        {
            get { return _model.EntityTypes[Definition.EntityTypeIndex]; }
        }

        public IReadOnlyList<IProperty> Properties
        {
            get { return Definition.PropertyIndexes.Select(i => EntityType.Properties[i]).ToArray(); }
        }

        protected struct KeyDefinition
        {
            public short EntityTypeIndex;
            public short[] PropertyIndexes;

            public KeyDefinition(short entityTypeIndex, short[] propertyIndexes)
            {
                EntityTypeIndex = entityTypeIndex;
                PropertyIndexes = propertyIndexes;
            }
        }
    }
}
