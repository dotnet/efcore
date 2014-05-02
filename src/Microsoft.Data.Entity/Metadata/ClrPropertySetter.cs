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
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ClrPropertySetter<TEntity, TValue> : IClrPropertySetter
    {
        private readonly Action<TEntity, TValue> _setter;

        public ClrPropertySetter([NotNull] Action<TEntity, TValue> setter)
        {
            Check.NotNull(setter, "setter");

            _setter = setter;
        }

        public void SetClrValue(object instance, object value)
        {
            Check.NotNull(instance, "instance");

            _setter((TEntity)instance, (TValue)value);
        }
    }
}
