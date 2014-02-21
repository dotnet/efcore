// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Property : MetadataBase, IProperty
    {
        private readonly string _name;
        private string _storageName;
        private readonly Type _propertyType;
        private readonly Type _declaringType;

        /// <summary>
        ///     Creates a new metadata object representing a .NET property.
        /// </summary>
        /// <param name="propertyInfo">The .NET property that this metadata object represents.</param>
        public Property([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            _name = propertyInfo.Name;
            _propertyType = propertyInfo.PropertyType;
            _declaringType = propertyInfo.DeclaringType;
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

        public virtual Type DeclaringType
        {
            get { return _declaringType; }
        }

        public virtual ValueGenerationStrategy ValueGenerationStrategy { get; [param: NotNull] set; }

        public virtual void SetValue(object instance, object value)
        {
            Check.NotNull(instance, "instance");

            // TODO: Handle shadow state
            _declaringType.GetAnyProperty(Name).SetValue(instance, value);
        }

        public virtual object GetValue(object instance)
        {
            Check.NotNull(instance, "instance");

            // TODO: Handle shadow state
            return _declaringType.GetAnyProperty(Name).GetValue(instance);
        }
    }
}
