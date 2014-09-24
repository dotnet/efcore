// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{EntityType.Name,nq}.{Name,nq} ({PropertyType.Name,nq})")]
    public class Property : PropertyBase, IProperty
    {
        private readonly Type _propertyType;
        private PropertyFlags _flags;
        private int _shadowIndex;
        private int _originalValueIndex = -1;
        private int _index;
        private int _maxLength;
        private ValueGeneration _valueGeneration;

        public Property([NotNull] string name, [NotNull] Type propertyType, [NotNull] EntityType entityType, bool shadowProperty = false)
            : base(name)
        {
            Check.NotNull(propertyType, "propertyType");
            Check.NotNull(entityType, "entityType");

            _propertyType = propertyType;
            EntityType = entityType;
            _shadowIndex = shadowProperty ? 0 : -1;
            IsNullable = propertyType.IsNullableType();
        }

        public virtual Type PropertyType
        {
            get { return _propertyType; }
        }

        public virtual Type UnderlyingType
        {
            get { return Nullable.GetUnderlyingType(_propertyType) ?? _propertyType; }
        }

        public virtual bool IsNullable
        {
            get { return GetFlag(PropertyFlags.IsNullable); }
            set { SetFlag(value, PropertyFlags.IsNullable); }
        }

        public virtual bool UseStoreDefault
        {
            get { return GetFlag(PropertyFlags.UseStoreDefault); }
            set { SetFlag(value, PropertyFlags.UseStoreDefault); }
        }

        public virtual int MaxLength
        {
            get { return _maxLength; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _maxLength = value;
            }
        }

        public virtual bool IsReadOnly
        {
            get { return this.IsKey() || GetFlag(PropertyFlags.IsReadOnly); }
            set
            {
                if (!value
                    && this.IsKey())
                {
                    throw new NotSupportedException(Strings.FormatKeyPropertyMustBeReadOnly(Name, EntityType.Name));
                }
                SetFlag(value, PropertyFlags.IsReadOnly);
            }
        }

        public virtual ValueGeneration ValueGeneration
        {
            get { return _valueGeneration; }
            set
            {
                Check.IsDefined(value, "value");

                _valueGeneration = value;
            }
        }

        public virtual bool IsShadowProperty
        {
            get { return _shadowIndex >= 0; }
            set
            {
                if (IsShadowProperty != value)
                {
                    _shadowIndex = value ? 0 : -1;

                    if (EntityType != null)
                    {
                        EntityType.PropertyMetadataChanged(this);
                    }
                }
            }
        }

        public virtual bool IsConcurrencyToken
        {
            get { return GetFlag(PropertyFlags.IsConcurrencyToken); }
            set
            {
                if (IsConcurrencyToken != value)
                {
                    SetFlag(value, PropertyFlags.IsConcurrencyToken);

                    if (EntityType != null)
                    {
                        EntityType.PropertyMetadataChanged(this);
                    }
                }
            }
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
                if (value < 0
                    || !IsShadowProperty)
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

        private bool GetFlag(PropertyFlags flag)
        {
            return (_flags & flag) != 0;
        }

        private void SetFlag(bool value, PropertyFlags flag)
        {
            _flags = value ? (_flags | flag) : (_flags & ~flag);
        }

        internal static string Format(IEnumerable<Property> properties)
        {
            return string.Join(", ", properties.Select(p => "'" + p.Name + "'"));
        }

        [Flags]
        private enum PropertyFlags
        {
            IsConcurrencyToken = 1,
            IsNullable = 2,
            IsReadOnly = 4,
            UseStoreDefault = 8
        }
    }
}
