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
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(name, nameof(name));

            return AddPropertyDefinition(name, propertyType, ClrType?.GetMembersInHierarchy(name).FirstOrDefault(), configurationSource, runConventions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition AddPropertyDefinition(
            [NotNull] MemberInfo memberInfo,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(memberInfo, nameof(memberInfo));

            if (ClrType == null)
            {
                throw new InvalidOperationException(CoreStrings.ClrPropertyOnShadowEntity(memberInfo.Name, this.DisplayName()));
            }

            if (memberInfo.DeclaringType == null
                || !memberInfo.DeclaringType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
            {
                throw new ArgumentException(CoreStrings.PropertyWrongEntityClrType(
                    memberInfo.Name, this.DisplayName(), memberInfo.DeclaringType?.ShortDisplayName()));
            }

            return AddPropertyDefinition(memberInfo.Name, memberInfo.GetMemberType(), memberInfo, configurationSource, runConventions);
        }

        private ComplexPropertyDefinition AddPropertyDefinition(
            string name,
            Type propertyType,
            MemberInfo memberInfo,
            ConfigurationSource configurationSource,
            bool runConventions)
        {
            var duplicateProperty = FindPropertyDefinition(name);
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateProperty(
                    name, this.DisplayName(), duplicateProperty.DeclaringType.DisplayName()));
            }

            var duplicateReference = FindComplexTypeReferenceDefinition(name);
            if (duplicateReference != null)
            {
                throw new InvalidOperationException(CoreStrings.ConflictingComplexReference(name, this.DisplayName(),
                    duplicateReference.DeclaringType.DisplayName()));
            }

            if (propertyType == null)
            {
                if (memberInfo == null)
                {
                    throw new InvalidOperationException(CoreStrings.NoPropertyType(name, this.DisplayName()));
                }

                propertyType = memberInfo.GetMemberType();
            }
            else
            {
                if (memberInfo != null
                    && propertyType != memberInfo.GetMemberType())
                {
                    throw new InvalidOperationException(CoreStrings.PropertyWrongClrType(
                        name,
                        this.DisplayName(),
                        memberInfo.GetMemberType().ShortDisplayName(),
                        propertyType.ShortDisplayName()));
                }
            }

            var property = new ComplexPropertyDefinition(name, propertyType, memberInfo as PropertyInfo, memberInfo as FieldInfo, this, configurationSource);

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
            [NotNull] string name,
            [NotNull] ComplexTypeDefinition referencedType,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(name, nameof(name));
            Check.NotNull(referencedType, nameof(referencedType));

            return AddComplexTypeReferenceDefinition(
                name, ClrType?.GetMembersInHierarchy(name).FirstOrDefault(), referencedType, configurationSource, runConventions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeReferenceDefinition AddComplexTypeReferenceDefinition(
            [NotNull] MemberInfo memberInfo,
            [NotNull] ComplexTypeDefinition referencedType,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(memberInfo, nameof(memberInfo));
            Check.NotNull(referencedType, nameof(referencedType));

            if (ClrType == null)
            {
                throw new InvalidOperationException(CoreStrings.ClrPropertyOnShadowEntity(memberInfo.Name, this.DisplayName()));
            }

            if (memberInfo.DeclaringType == null
                || !memberInfo.DeclaringType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
            {
                throw new ArgumentException(CoreStrings.PropertyWrongEntityClrType(
                    memberInfo.Name, this.DisplayName(), memberInfo.DeclaringType?.ShortDisplayName()));
            }

            return AddComplexTypeReferenceDefinition(memberInfo.Name, memberInfo, referencedType, configurationSource, runConventions);
        }

        private ComplexTypeReferenceDefinition AddComplexTypeReferenceDefinition(
            string name,
            MemberInfo memberInfo,
            ComplexTypeDefinition referencedType,
            ConfigurationSource configurationSource,
            bool runConventions)
        {
            Check.NotNull(name, nameof(name));

            var duplicateReference = FindComplexTypeReferenceDefinition(name);
            if (duplicateReference != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateComplexReference(name, this.DisplayName(),
                    duplicateReference.DeclaringType.DisplayName()));
            }

            var duplicateProperty = FindPropertyDefinition(name);
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(CoreStrings.ConflictingPropertyToReference(
                    name, this.DisplayName(), duplicateProperty.DeclaringType.DisplayName()));
            }


            if (memberInfo != null
                && !memberInfo.GetMemberType().GetTypeInfo().IsAssignableFrom(referencedType.ClrType.GetTypeInfo()))
            {
                throw new InvalidOperationException(CoreStrings.PropertyWrongClrType(
                    name,
                    this.DisplayName(),
                    memberInfo.GetMemberType().ShortDisplayName(),
                    referencedType.ClrType.ShortDisplayName()));
            }

            var property = new ComplexTypeReferenceDefinition(
                name, memberInfo as PropertyInfo, memberInfo as FieldInfo, referencedType, this, configurationSource);

            _referenceDefinitions.Add(property.Name, property);

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
        public virtual ComplexTypeReferenceDefinition FindComplexTypeReferenceDefinition([NotNull] string name)
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
        public virtual ComplexTypeReferenceDefinition RemoveComplexTypeReferenceDefinition([NotNull] string name)
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

        IMutableComplexPropertyDefinition IMutableComplexTypeDefinition.AddPropertyDefinition(string name, Type propertyType) => AddPropertyDefinition(name, propertyType);
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
