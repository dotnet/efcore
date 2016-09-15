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
    public class ComplexType : StructuralType, IMutableComplexType
    {
        private readonly SortedDictionary<string, ComplexPropertyDefinition> _properties
            = new SortedDictionary<string, ComplexPropertyDefinition>(StringComparer.Ordinal);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexType([NotNull] string name, [NotNull] Model model, ConfigurationSource configurationSource)
            : base(name, model, configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(model, nameof(model));

            // TODO: Builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexType([NotNull] Type clrType, [NotNull] Model model, ConfigurationSource configurationSource)
            : base(clrType, model, configurationSource)
        {
            Check.ValidEntityType(clrType, nameof(clrType));
            Check.NotNull(model, nameof(model));

            // TODO: Builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

        // TODO: public virtual InternalEntityTypeBuilder Builder { [DebuggerStepThrough] get; [DebuggerStepThrough] [param: CanBeNull] set; }

        // TODO: public override string ToString() => this.ToDebugString();

        // TODO: protected override Annotation OnAnnotationSet(string name, Annotation annotation, Annotation oldAnnotation)
        //=> Model.ConventionDispatcher.OnEntityTypeAnnotationSet(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition AddProperty(
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

                    return AddProperty(clrProperty, configurationSource, runConventions);
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

            return AddProperty(property, runConventions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition AddProperty(
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

            return AddProperty(new ComplexPropertyDefinition(propertyInfo, this, configurationSource), runConventions);
        }

        private void ValidateCanAddProperty(string name)
        {
            var duplicateProperty = FindProperty(name);
            if (duplicateProperty != null)
            {
                // TODO: Message
                throw new InvalidOperationException(CoreStrings.DuplicateProperty(
                    name, this.DisplayName(), duplicateProperty.DeclaringType.DisplayName()));
            }
        }

        private ComplexPropertyDefinition AddProperty(ComplexPropertyDefinition property, bool runConventions)
        {
            _properties.Add(property.Name, property);

            PropertyMetadataChanged();

            if (runConventions)
            {
                // TODO: property = Model.ConventionDispatcher.OnPropertyAdded(property.Builder)?.Metadata;
            }

            return property;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition GetOrAddProperty([NotNull] PropertyInfo propertyInfo)
            => FindProperty(propertyInfo) ?? AddProperty(propertyInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition GetOrAddProperty([NotNull] string name, [NotNull] Type propertyType, bool shadow)
            => FindProperty(name) ?? AddProperty(name, propertyType, shadow);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition FindProperty([NotNull] PropertyInfo propertyInfo)
            => FindProperty(propertyInfo.Name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ComplexPropertyDefinition FindProperty([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            ComplexPropertyDefinition property;
            return _properties.TryGetValue(name, out property)
                ? property
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ComplexPropertyDefinition RemoveProperty([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = FindProperty(name);
            return property == null
                ? null
                : RemoveProperty(property);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void OnEntityTypeMemberIgnored(string name)
        {
        }

        // TODO: => Model.ConventionDispatcher.OnEntityTypeMemberIgnored(Builder, name);

        private ComplexPropertyDefinition RemoveProperty(ComplexPropertyDefinition property)
        {
            CheckPropertyNotInUse(property);

            _properties.Remove(property.Name);
            // TODO: property.Builder = null;

            PropertyMetadataChanged();

            return property;
        }

        private void CheckPropertyNotInUse(ComplexPropertyDefinition property)
        {
            // TODO: Check not in any keys, etc.
            //if (entityType.GetDeclaredKeys().Any(k => k.Properties.Contains(property))
            //    || entityType.GetDeclaredForeignKeys().Any(k => k.Properties.Contains(property))
            //    || entityType.GetDeclaredIndexes().Any(i => i.Properties.Contains(property)))
            //{
            //    throw new InvalidOperationException(CoreStrings.PropertyInUse(property.Name, this.DisplayName()));
            //}
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual IEnumerable<ComplexPropertyDefinition> GetProperties()
            => _properties.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void PropertyMetadataChanged()
        {
            // TODO:
            //foreach (var indexedProperty in this.GetPropertiesAndNavigations())
            //{
            //    indexedProperty.TrySetIndexes(null);
            //}
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int PropertyCount => _properties.Count;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ComplexTypeUsage> GetComplexTypeUsages()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeUsage FindComplexTypeUsage(string name)
        {
            throw new NotImplementedException();
        }

        IModel IStructuralType.Model => Model;
        IMutableModel IMutableStructuralType.Model => Model;

        IMutableStructuralProperty IMutableStructuralType.AddProperty(string name, Type propertyType, bool shadow)
            => AddProperty(name, propertyType, shadow);

        IMutableComplexPropertyDefinition IMutableComplexType.AddProperty(string name, Type propertyType, bool shadow)
            => AddProperty(name, propertyType, shadow);

        IStructuralProperty IStructuralType.FindProperty(string name) => FindProperty(name);
        IComplexPropertyDefinition IComplexType.FindProperty(string name) => FindProperty(name);
        IMutableStructuralProperty IMutableStructuralType.FindProperty(string name) => FindProperty(name);
        IMutableComplexPropertyDefinition IMutableComplexType.FindProperty(string name) => FindProperty(name);

        IEnumerable<IStructuralProperty> IStructuralType.GetProperties() => GetProperties();
        IEnumerable<IComplexPropertyDefinition> IComplexType.GetProperties() => GetProperties();
        IEnumerable<IMutableStructuralProperty> IMutableStructuralType.GetProperties() => GetProperties();
        IEnumerable<IMutableComplexPropertyDefinition> IMutableComplexType.GetProperties() => GetProperties();

        IMutableStructuralProperty IMutableStructuralType.RemoveProperty(string name) => RemoveProperty(name);
        IMutableComplexPropertyDefinition IMutableComplexType.RemoveProperty(string name) => RemoveProperty(name);

        IComplexTypeReference IStructuralType.FindComplexTypeReference(string name) => FindComplexTypeReference(name);
        IEnumerable<IComplexTypeReference> IStructuralType.GetComplexTypeReferences() => GetComplexTypeReferences();

        // TODO: public virtual DebugView<EntityType> DebugView => new DebugView<EntityType>(this, m => m.ToDebugString(false));
    }
}
