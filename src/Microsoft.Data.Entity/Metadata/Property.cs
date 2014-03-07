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
        private readonly PropertyInfo _propertyInfo;

        private string _storageName;
        private bool _isNullable = true;

        // Intended only for creation of test doubles
        internal Property()
        {
        }

        /// <summary>
        ///     Creates a new metadata object representing a .NET property.
        /// </summary>
        /// <param name="propertyInfo">The .NET property that this metadata object represents.</param>
        public Property([NotNull] PropertyInfo propertyInfo)
            : this(Check.NotNull(propertyInfo, "propertyInfo").Name, propertyInfo.PropertyType)
        {
            _propertyInfo = propertyInfo;
        }

        /// <summary>
        ///     Creates a new metadata object representing an property that will participate in shadow-state
        ///     such that there is no underlying .NET property corresponding to this metadata object.
        /// </summary>
        /// <param name="name">The name of the shadow-state property.</param>
        /// <param name="propertyType">The type of the shadow-state property.</param>
        public Property([NotNull] string name, [NotNull] Type propertyType)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(propertyType, "propertyType");

            _name = name;
            _propertyType = propertyType;
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

        public virtual PropertyInfo PropertyInfo
        {
            get { return _propertyInfo; }
        }

        // TODO: Consider properties that are part of some complex/value type
        public virtual EntityType EntityType { get; [param: CanBeNull] internal set; }

        public virtual bool IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        public virtual ValueGenerationStrategy ValueGenerationStrategy { get; [param: NotNull] set; }

        public virtual void SetValue(object instance, object value)
        {
            Check.NotNull(instance, "instance");

            // TODO: Handle shadow state
            _propertyInfo.SetValue(instance, value);
        }

        public virtual object GetValue(object instance)
        {
            Check.NotNull(instance, "instance");

            // TODO: Handle shadow state
            return _propertyInfo.GetValue(instance);
        }

        IEntityType IProperty.EntityType
        {
            get { return EntityType; }
        }
    }
}
