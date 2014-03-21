// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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

        private bool _isNullable = true;
        private int _shadowIndex;
        private int _index;

        internal Property([NotNull] string name, [NotNull] Type propertyType, bool shadowProperty)
            : base(Check.NotEmpty(name, "name"))
        {
            Check.NotNull(propertyType, "propertyType");

            _propertyType = propertyType;
            _shadowIndex = shadowProperty ? 0 : -1;
            _isNullable = propertyType.IsNullableType();
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

        IEntityType IPropertyBase.EntityType
        {
            get { return EntityType; }
        }
    }
}
