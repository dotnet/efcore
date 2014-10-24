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
        private int _maxLength = -1;
        private ValueGeneration? _valueGeneration;

        public Property([NotNull] string name, [NotNull] Type propertyType, [NotNull] EntityType entityType, bool shadowProperty = false)
            : base(name)
        {
            Check.NotNull(propertyType, "propertyType");
            Check.NotNull(entityType, "entityType");

            _propertyType = propertyType;
            EntityType = entityType;
            _shadowIndex = shadowProperty ? 0 : -1;
        }

        public virtual Type PropertyType
        {
            get { return _propertyType; }
        }

        public virtual Type UnderlyingType
        {
            get { return _propertyType.UnwrapNullableType(); }
        }

        // TODO: Remove this once the model is readonly
        // Issue #868
        private bool _isNullableSet;

        public virtual bool? IsNullable
        {
            get
            {
                return _isNullableSet
                    ? (bool?)GetFlag(PropertyFlags.IsNullable)
                    : null;
            }
            set
            {
                if (!value.HasValue)
                {
                    _isNullableSet = false;
                }
                else
                {
                    _isNullableSet = true;
                    SetFlag(value.Value, PropertyFlags.IsNullable);
                }
            }
        }

        protected virtual bool DefaultIsNullable
        {
            get { return _propertyType.IsNullableType(); }
        }

        // TODO: Remove this once the model is readonly
        // Issue #868
        private bool _useStoreDefaultSet;

        public virtual bool? UseStoreDefault
        {
            get
            {
                return _useStoreDefaultSet
                    ? (bool?)GetFlag(PropertyFlags.UseStoreDefault)
                    : null;
            }
            set
            {
                if (!value.HasValue)
                {
                    _useStoreDefaultSet = false;
                }
                else
                {
                    _useStoreDefaultSet = true;
                    SetFlag(value.Value, PropertyFlags.UseStoreDefault);
                }
            }
        }

        protected virtual bool DefaultUseStoreDefault
        {
            get { return false; }
        }

        public virtual int? MaxLength
        {
            get
            {
                return _maxLength >= 0
                    ? (int?)_maxLength
                    : null;
            }
            set
            {
                if (!value.HasValue)
                {
                    _maxLength = -1;
                }
                else
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }

                    _maxLength = value.Value;
                }
            }
        }

        protected virtual int DefaultMaxLength
        {
            get { return 0; }
        }

        // TODO: Remove this once the model is readonly
        // Issue #868
        private bool _isReadOnlySet;

        public virtual bool? IsReadOnly
        {
            get
            {
                return _isReadOnlySet
                    ? (bool?)GetFlag(PropertyFlags.IsReadOnly)
                    : null;
            }
            set
            {
                if (!value.HasValue)
                {
                    _isReadOnlySet = false;
                }
                else
                {
                    if (!value.Value
                        && this.IsKey())
                    {
                        throw new NotSupportedException(Strings.FormatKeyPropertyMustBeReadOnly(Name, EntityType.Name));
                    }
                    _isReadOnlySet = true;
                    SetFlag(value.Value, PropertyFlags.IsReadOnly);
                }
            }
        }

        protected virtual bool DefaultIsReadOnly
        {
            get { return this.IsKey(); }
        }

        public virtual ValueGeneration? ValueGeneration
        {
            get { return _valueGeneration; }
            set
            {
                if (value.HasValue)
                {
                    Check.IsDefined(value.Value, "value");
                }

                _valueGeneration = value;
            }
        }

        protected virtual ValueGeneration DefaultValueGeneration
        {
            get { return Metadata.ValueGeneration.None; }
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

        // TODO: Remove this once the model is readonly
        // Issue #868
        private bool _isConcurrencyTokenSet;

        public virtual bool? IsConcurrencyToken
        {
            get
            {
                return _isConcurrencyTokenSet ?
                    (bool?)GetFlag(PropertyFlags.IsConcurrencyToken)
                    : null;
            }
            set
            {
                if (!value.HasValue)
                {
                    _isConcurrencyTokenSet = false;
                }
                else
                {
                    _isConcurrencyTokenSet = true;
                    if (IsConcurrencyToken != value)
                    {
                        SetFlag(value.Value, PropertyFlags.IsConcurrencyToken);

                        if (EntityType != null)
                        {
                            EntityType.PropertyMetadataChanged(this);
                        }
                    }
                }
            }
        }

        protected virtual bool DefaultIsConcurrencyToken
        {
            get { return false; }
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

        bool IProperty.IsNullable
        {
            get { return IsNullable ?? DefaultIsNullable; }
        }

        bool IProperty.UseStoreDefault
        {
            get { return UseStoreDefault ?? DefaultUseStoreDefault; }
        }

        int IProperty.MaxLength
        {
            get { return MaxLength ?? DefaultMaxLength; }
        }

        bool IProperty.IsReadOnly
        {
            get { return IsReadOnly ?? DefaultIsReadOnly; }
        }

        ValueGeneration IProperty.ValueGeneration
        {
            get { return ValueGeneration ?? DefaultValueGeneration; }
        }

        bool IProperty.IsConcurrencyToken
        {
            get { return IsConcurrencyToken ?? DefaultIsConcurrencyToken; }
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
