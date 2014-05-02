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
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{PropertyType.Name,nq} {Name,nq}")]
    public class Property : NamedMetadataBase, IProperty
    {
        private readonly Type _propertyType;
        private readonly bool _isConcurrencyToken;

        private bool _isNullable = true;
        private int _shadowIndex;
        private int _originalValueIndex = -1;
        private int _index;

        internal Property([NotNull] string name, [NotNull] Type propertyType)
            : this(name, propertyType, shadowProperty: false, concurrencyToken: false)
        {
        }

        internal Property([NotNull] string name, [NotNull] Type propertyType, bool shadowProperty, bool concurrencyToken)
            : base(Check.NotEmpty(name, "name"))
        {
            Check.NotNull(propertyType, "propertyType");

            _propertyType = propertyType;
            _shadowIndex = shadowProperty ? 0 : -1;
            _isNullable = propertyType.IsNullableType();
            _isConcurrencyToken = concurrencyToken;
        }

        public virtual Type PropertyType
        {
            get { return _propertyType; }
        }

        // TODO: Consider properties that are part of some complex/value type
        public virtual EntityType EntityType { get; internal set; }

        public virtual bool IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        public virtual ValueGenerationStrategy ValueGenerationStrategy { get; [param: NotNull] set; }

        public virtual bool IsClrProperty
        {
            get { return _shadowIndex < 0; }
        }

        public virtual bool IsShadowProperty
        {
            get { return !IsClrProperty; }
        }

        public virtual bool IsConcurrencyToken
        {
            get { return _isConcurrencyToken; }
        }

        public virtual int Index
        {
            get { return _index; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _index = value;
            }
        }

        public virtual int ShadowIndex
        {
            get { return _shadowIndex; }
            set
            {
                if (value < 0 || IsClrProperty)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _shadowIndex = value;
            }
        }

        public virtual int OriginalValueIndex
        {
            get { return _originalValueIndex; }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _originalValueIndex = value;
            }
        }

        IEntityType IPropertyBase.EntityType
        {
            get { return EntityType; }
        }
    }
}
