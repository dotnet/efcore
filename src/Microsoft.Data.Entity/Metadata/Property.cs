// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Property : MetadataBase, IProperty
    {
        private readonly MetadataName _name;
        private readonly Type _propertyType;
        private readonly Type _declaringType;

        /// <summary>
        /// Creates a new metadata object representing a .NET property.
        /// </summary>
        /// <param name="propertyInfo">The .NET property that this metadata object represents.</param>
        public Property([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            _name = new MetadataName(propertyInfo.Name);
            _propertyType = propertyInfo.PropertyType;
            _declaringType = propertyInfo.DeclaringType;
        }

        /// <summary>
        /// Creates a new metadata object representing an property that will participate in shadow-state
        /// such that there is no underlying .NET property corresponding to this metadata object.
        /// </summary>
        /// <param name="name">The name of the shadow-state property.</param>
        /// <param name="type">The type of the shadow-state property.</param>
        public Property([NotNull] string name, [NotNull] Type type)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(type, "type");

            _name = new MetadataName(name);
            _propertyType = type;
        }

        public virtual string Name
        {
            get { return _name.Name; }
        }

        public virtual string StorageName
        {
            get { return _name.StorageName; }
            [param: NotNull]
            set
            {
                Check.NotEmpty(value, "value");

                _name.StorageName = value;
            }
        }

        public virtual Type Type
        {
            get { return _propertyType; }
        }

        public virtual Type DeclaringType
        {
            get { return _declaringType; }
        }

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
