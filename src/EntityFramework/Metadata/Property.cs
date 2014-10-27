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
        private readonly EntityType _entityType;
        private PropertyFlags _flags;
        // TODO: Remove this once the model is readonly Issue #868
        private PropertyFlags _setFlags;
        private int _shadowIndex;
        private int _originalValueIndex = -1;
        private int _index;
        private int _maxLength = -1;

        public Property([NotNull] string name, [NotNull] Type propertyType, [NotNull] EntityType entityType, bool shadowProperty = false)
            : base(name)
        {
            Check.NotNull(propertyType, "propertyType");
            Check.NotNull(entityType, "entityType");

            _propertyType = propertyType;
            _entityType = entityType;
            _shadowIndex = shadowProperty ? 0 : -1;
        }

        public virtual Type PropertyType
        {
            get { return _propertyType; }
        }

        public override EntityType EntityType
        {
            get { return _entityType; }
        }

        public virtual Type UnderlyingType
        {
            get { return _propertyType.UnwrapNullableType(); }
        }

        public virtual bool? IsNullable
        {
            get { return GetFlag(PropertyFlags.IsNullable); }
            set
            {
                if (value.HasValue
                    && value.Value
                    && !_propertyType.IsNullableType())
                {
                    throw new InvalidOperationException(Strings.FormatCannotBeNullable(Name, EntityType.SimpleName, _propertyType.Name));
                }

                SetFlag(value, PropertyFlags.IsNullable);
            }
        }

        protected virtual bool DefaultIsNullable
        {
            get { return _propertyType.IsNullableType(); }
        }

        public virtual bool? UseStoreDefault
        {
            get { return GetFlag(PropertyFlags.UseStoreDefault); }
            set { SetFlag(value, PropertyFlags.UseStoreDefault); }
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

        public virtual bool? IsReadOnly
        {
            get { return GetFlag(PropertyFlags.IsReadOnly); }
            set
            {
                if (value.HasValue
                    && !value.Value
                    && this.IsKey())
                {
                    throw new NotSupportedException(Strings.FormatKeyPropertyMustBeReadOnly(Name, EntityType.Name));
                }
                SetFlag(value, PropertyFlags.IsReadOnly);
            }
        }

        protected virtual bool DefaultIsReadOnly
        {
            get { return this.IsKey(); }
        }

        public virtual bool? IsStoreComputed
        {
            get { return GetFlag(PropertyFlags.IsStoreComputed); }
            set { SetFlag(value, PropertyFlags.IsStoreComputed); }
        }

        protected virtual bool DefaultIsStoreComputed
        {
            get { return false; }
        }

        public virtual bool? GenerateValueOnAdd
        {
            get { return GetFlag(PropertyFlags.GenerateValueOnAdd); }
            set { SetFlag(value, PropertyFlags.GenerateValueOnAdd); }
        }

        protected virtual bool DefaultGenerateValueOnAdd
        {
            get { return false; }
        }

        public virtual bool IsShadowProperty
        {
            get { return _shadowIndex >= 0; }
            set
            {
                if (IsShadowProperty != value)
                {
                    _shadowIndex = value ? 0 : -1;

                    EntityType.PropertyMetadataChanged(this);
                }
            }
        }

        public virtual bool? IsConcurrencyToken
        {
            get { return GetFlag(PropertyFlags.IsConcurrencyToken); }
            set
            {
                if (IsConcurrencyToken != value)
                {
                    SetFlag(value, PropertyFlags.IsConcurrencyToken);

                    EntityType.PropertyMetadataChanged(this);
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

        private bool? GetFlag(PropertyFlags flag)
        {
            return (_setFlags & flag) != 0 ? (_flags & flag) != 0 : (bool?)null;
        }

        private void SetFlag(bool? value, PropertyFlags flag)
        {
            _setFlags = value.HasValue ? (_setFlags | flag) : (_setFlags & ~flag);
            _flags = value.HasValue && value.Value ? (_flags | flag) : (_flags & ~flag);
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

        bool IProperty.IsStoreComputed
        {
            get { return IsStoreComputed ?? DefaultIsStoreComputed; }
        }

        bool IProperty.GenerateValueOnAdd
        {
            get { return GenerateValueOnAdd ?? DefaultGenerateValueOnAdd; }
        }

        bool IProperty.IsConcurrencyToken
        {
            get { return IsConcurrencyToken ?? DefaultIsConcurrencyToken; }
        }

        [Flags]
        private enum PropertyFlags : ushort
        {
            IsConcurrencyToken = 1,
            IsNullable = 2,
            IsReadOnly = 4,
            UseStoreDefault = 8,
            IsStoreComputed = 16,
            GenerateValueOnAdd = 32
        }
    }
}
