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

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class CompiledPropertyNoAnnotations<TProperty> : NoAnnotations
    {
        private readonly IEntityType _entityType;

        protected CompiledPropertyNoAnnotations(IEntityType entityType)
        {
            _entityType = entityType;
        }

        public Type PropertyType
        {
            get { return typeof(TProperty); }
        }

        public ValueGenerationStrategy ValueGenerationStrategy
        {
            get { return ValueGenerationStrategy.None; }
        }

        public bool IsNullable
        {
            get { return typeof(TProperty).IsNullableType(); }
        }

        public IEntityType EntityType
        {
            get { return _entityType; }
        }

        public bool IsClrProperty
        {
            get { return true; }
        }

        public bool IsConcurrencyToken
        {
            // TODO:
            get { return true; }
        }

        public int OriginalValueIndex
        {
            // TODO:
            get { return -1; }
        }
    }
}
