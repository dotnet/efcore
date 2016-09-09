// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ComplexTypeDefinition : TypeBase, IMutableComplexTypeDefinition
    {
        private readonly SortedDictionary<string, ComplexPropertyDefinition> _propertyDefinitions
            = new SortedDictionary<string, ComplexPropertyDefinition>(StringComparer.Ordinal);

        private readonly SortedDictionary<string, ComplexTypeReferenceDefinition> _referenceDefinitions
            = new SortedDictionary<string, ComplexTypeReferenceDefinition>(StringComparer.Ordinal);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeDefinition([NotNull] string name, [NotNull] Model model, ConfigurationSource configurationSource)
            : base(name, model, configurationSource)
        {
            // TODO: ComplexType builders
            // Builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeDefinition([NotNull] Type clrType, [NotNull] Model model, ConfigurationSource configurationSource)
            : base(clrType, model, configurationSource)
        {
            // TODO: ComplexType builders
            // Builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

        // TODO: ComplexType builders
        // public virtual InternalEntityTypeBuilder Builder { [DebuggerStepThrough] get; [DebuggerStepThrough] [param: CanBeNull] set; }

        // TODO: ComplexType debug strings 
        // public override string ToString() => this.ToDebugString();

        // TODO: ComplexType builders
        // protected override Annotation OnAnnotationSet(string name, Annotation annotation, Annotation oldAnnotation)
        //    => Model.ConventionDispatcher.OnEntityTypeAnnotationSet(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition AddPropertyDefinition(
            [NotNull] string name,
            [CanBeNull] Type propertyType = null,
            bool? shadow = null,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(name, nameof(name));

            ValidateCanAddProperty(name);

            FieldInfo fieldInfo = null;

            if (shadow != true)
            {
                var clrProperty = ClrType?.GetPropertiesInHierarchy(name).FirstOrDefault();
                if (clrProperty != null)
                {
                    if (propertyType != null
                        && propertyType != clrProperty.PropertyType)
                    {
                        throw new InvalidOperationException(CoreStrings.PropertyWrongClrType(
                            name,
                            this.DisplayName(),
                            clrProperty.PropertyType.ShortDisplayName(),
                            propertyType.ShortDisplayName()));
                    }

                    return AddPropertyDefinition(clrProperty, configurationSource, runConventions);
                }

                fieldInfo = ClrType?.GetTypesInHierarchy()
                    .SelectMany(e => e.GetRuntimeFields()
                        .Where(f => f.Name == name
                                    && (propertyType == null
                                        || f.FieldType.GetTypeInfo().IsAssignableFrom(propertyType.GetTypeInfo()))))
                    .FirstOrDefault();

                if (fieldInfo != null
                    && propertyType == null)
                {
                    propertyType = fieldInfo.FieldType;
                }

                if (shadow == false)
                {
                    if (ClrType == null)
                    {
                        throw new InvalidOperationException(CoreStrings.ClrPropertyOnShadowEntity(name, this.DisplayName()));
                    }

                    throw new InvalidOperationException(CoreStrings.NoClrProperty(name, this.DisplayName()));
                }
            }

            if (propertyType == null)
            {
                throw new InvalidOperationException(CoreStrings.NoPropertyType(name, this.DisplayName()));
            }

            var property = new ComplexPropertyDefinition(name, propertyType, this, configurationSource);

            if (fieldInfo != null)
            {
                property.SetFieldInfo(fieldInfo, ConfigurationSource.Convention, runConventions: false);
            }

            return AddPropertyDefinition(property, runConventions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition AddPropertyDefinition(
            [NotNull] PropertyInfo propertyInfo,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            ValidateCanAddProperty(propertyInfo.Name);

            if (ClrType == null)
            {
                throw new InvalidOperationException(CoreStrings.ClrPropertyOnShadowEntity(propertyInfo.Name, this.DisplayName()));
            }

            if (propertyInfo.DeclaringType == null
                || !propertyInfo.DeclaringType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
            {
                throw new ArgumentException(CoreStrings.PropertyWrongEntityClrType(
                    propertyInfo.Name, this.DisplayName(), propertyInfo.DeclaringType?.ShortDisplayName()));
            }

            return AddPropertyDefinition(new ComplexPropertyDefinition(propertyInfo, this, configurationSource), runConventions);
        }

        private void ValidateCanAddProperty(string name)
        {
            var duplicateProperty = FindPropertyDefinition(name);
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateProperty(
                    name, this.DisplayName(), duplicateProperty.DeclaringType.DisplayName()));
            }
        }

        private ComplexPropertyDefinition AddPropertyDefinition(ComplexPropertyDefinition property, bool runConventions)
        {
            _propertyDefinitions.Add(property.Name, property);

            PropertyMetadataChanged();

            if (runConventions)
            {
                // TODO: ComplexType builders
                //property = Model.ConventionDispatcher.OnPropertyAdded(property.Builder)?.Metadata;
            }

            return property;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition GetOrAddPropertyDefinition([NotNull] PropertyInfo propertyInfo)
            => FindPropertyDefinition(propertyInfo) ?? AddPropertyDefinition(propertyInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition GetOrAddPropertyDefinition([NotNull] string name, [NotNull] Type propertyType, bool shadow)
            => FindPropertyDefinition(name) ?? AddPropertyDefinition(name, propertyType, shadow);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition FindPropertyDefinition([NotNull] PropertyInfo propertyInfo)
            => FindPropertyDefinition(propertyInfo.Name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition FindPropertyDefinition([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            ComplexPropertyDefinition property;
            return _propertyDefinitions.TryGetValue(name, out property)
                ? property
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ComplexPropertyDefinition> GetDeclaredPropertyDefinitions() => _propertyDefinitions.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition RemovePropertyDefinition([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = FindPropertyDefinition(name);
            return property == null
                ? null
                : RemovePropertyDefinition(property);
        }

        private ComplexPropertyDefinition RemovePropertyDefinition(ComplexPropertyDefinition property)
        {
            // TODO: ComplexType Check if property usage is in use in any entity.

            _propertyDefinitions.Remove(property.Name);

            // TODO: ComplexType builders
            //property.Builder = null;

            PropertyMetadataChanged();

            return property;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ComplexPropertyDefinition> GetPropertyDefinitions() => _propertyDefinitions.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void PropertyMetadataChanged()
        {
            // TODO: ComplexType metdata changed
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void OnTypeMemberIgnored(string name)
        {
        }
        // TODO: ComplexType builders
        //    => Model.ConventionDispatcher.OnEntityTypeMemberIgnored(Builder, name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeReferenceDefinition AddComplexTypeReferenceDefinition(
            string name, ComplexTypeDefinition referencedType)
        {
            // TODO: ComplexType validate
            return new ComplexTypeReferenceDefinition(name, referencedType, this, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeReferenceDefinition FindComplexTypeReferenceDefinition([NotNull] PropertyInfo propertyInfo)
            => FindComplexTypeReferenceDefinition(propertyInfo.Name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeReferenceDefinition FindComplexTypeReferenceDefinition(string name)
        {
            Check.NotEmpty(name, nameof(name));

            ComplexTypeReferenceDefinition property;
            return _referenceDefinitions.TryGetValue(name, out property)
                ? property
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ComplexTypeReferenceDefinition> GetComplexTypeReferenceDefinitions() 
            => _referenceDefinitions.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeReferenceDefinition RemoveComplexTypeReferenceDefinition(string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = FindComplexTypeReferenceDefinition(name);
            return property == null
                ? null
                : RemoveComplexTypeReferenceDefinition(property);
        }

        private ComplexTypeReferenceDefinition RemoveComplexTypeReferenceDefinition(ComplexTypeReferenceDefinition property)
        {
            // TODO: ComplexType Check if property usage is in use in any entity.

            _referenceDefinitions.Remove(property.Name);

            // TODO: ComplexType builders
            //property.Builder = null;

            PropertyMetadataChanged();

            return property;
        }

        IModel ITypeBase.Model => Model;
        IMutableModel IMutableTypeBase.Model => Model;

        IComplexPropertyDefinition IComplexTypeDefinition.FindPropertyDefinition(string name) => FindPropertyDefinition(name);
        IMutableComplexPropertyDefinition IMutableComplexTypeDefinition.FindPropertyDefinition(string name) => FindPropertyDefinition(name);

        IEnumerable<IComplexPropertyDefinition> IComplexTypeDefinition.GetPropertyDefinitions() => GetPropertyDefinitions();
        IEnumerable<IMutableComplexPropertyDefinition> IMutableComplexTypeDefinition.GetPropertyDefinitions() => GetPropertyDefinitions();

        IMutableComplexPropertyDefinition IMutableComplexTypeDefinition.AddPropertyDefinition(string name, Type propertyType, bool shadow) => AddPropertyDefinition(name, propertyType, shadow);
        IMutableComplexPropertyDefinition IMutableComplexTypeDefinition.RemovePropertyDefinition(string name) => RemovePropertyDefinition(name);

        IComplexTypeReferenceDefinition IComplexTypeDefinition.FindComplexTypeReferenceDefinition(string name) => FindComplexTypeReferenceDefinition(name);
        IMutableComplexTypeReferenceDefinition IMutableComplexTypeDefinition.FindComplexTypeReferenceDefinition(string name) => FindComplexTypeReferenceDefinition(name);

        IEnumerable<IComplexTypeReferenceDefinition> IComplexTypeDefinition.GetComplexTypeReferenceDefinitions() => GetComplexTypeReferenceDefinitions();
        IEnumerable<IMutableComplexTypeReferenceDefinition> IMutableComplexTypeDefinition.GetComplexTypeReferenceDefinitions() => GetComplexTypeReferenceDefinitions();

        IMutableComplexTypeReferenceDefinition IMutableComplexTypeDefinition.AddComplexTypeReferenceDefinition(string name, IMutableComplexTypeDefinition referencedType) => AddComplexTypeReferenceDefinition(name, (ComplexTypeDefinition)referencedType);
        IMutableComplexTypeReferenceDefinition IMutableComplexTypeDefinition.RemoveComplexTypeReferenceDefinition(string name) => RemoveComplexTypeReferenceDefinition(name);

        // TODO: ComplexType debug strings
        //public virtual DebugView<EntityType> DebugView => new DebugView<EntityType>(this, m => m.ToDebugString(false));
    }
}
