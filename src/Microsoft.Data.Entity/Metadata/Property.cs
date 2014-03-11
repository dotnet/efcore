// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{PropertyType.Name,nq} {Name,nq}")]
    public class Property : MetadataBase, IProperty
    {
        private readonly string _name;
        private readonly Type _propertyType;

        private string _storageName;
        private bool _isNullable = true;
        private int _shadowIndex;
        private int _index;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected Property()
        {
        }

        /// <summary>
        ///     Creates a new metadata object representing a .NET property.
        /// </summary>
        /// <param name="propertyInfo">The .NET property that this metadata object represents.</param>
        public Property([NotNull] PropertyInfo propertyInfo)
            : this(Check.NotNull(propertyInfo, "propertyInfo").Name, propertyInfo.PropertyType, hasClrProperty: true)
        {
        }

        public Property([NotNull] string name, [NotNull] Type propertyType, bool hasClrProperty)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(propertyType, "propertyType");

            _name = name;
            _propertyType = propertyType;
            _shadowIndex = hasClrProperty ? -1 : 0;
            _isNullable = propertyType.IsNullableType();
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual string StorageName
        {
            get { return _storageName ?? _name; }
            [param: NotNull]
            set
            {
                Check.NotEmpty(value, "value");

                _storageName = value;
            }
        }

        public virtual Type PropertyType
        {
            get { return _propertyType; }
        }

        // TODO: Consider properties that are part of some complex/value type
        public virtual EntityType EntityType { get; [param: CanBeNull] set; }

        public virtual bool IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        public virtual ValueGenerationStrategy ValueGenerationStrategy { get; [param: NotNull] set; }

        public virtual bool HasClrProperty
        {
            get { return _shadowIndex < 0; }
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
                if (value < 0 || HasClrProperty)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _shadowIndex = value;
            }
        }

        // TODO: Move these off IProperty
        public virtual void SetValue(object instance, object value)
        {
            Check.NotNull(instance, "instance");

            EntityType.Type.GetAnyProperty(Name).SetValue(instance, value);
        }

        // TODO: Move these off IProperty
        public virtual object GetValue(object instance)
        {
            Check.NotNull(instance, "instance");

            return EntityType.Type.GetAnyProperty(Name).GetValue(instance);
        }

        IEntityType IProperty.EntityType
        {
            get { return EntityType; }
        }
    }
}
