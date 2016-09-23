// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ComplexProperty : Property, IMutableComplexProperty
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexProperty(
            [NotNull] ComplexTypeUsage complexTypeUsage,
            [NotNull] ComplexPropertyDefinition propertyDefinition,
            ConfigurationSource configurationSource)
            : base(
                propertyDefinition.Name,
                propertyDefinition.ClrType,
                propertyDefinition.PropertyInfo,
                propertyDefinition.FieldInfo,
                complexTypeUsage.DeclaringEntityType,
                configurationSource)
        {
            DeclaringType = complexTypeUsage;
            Definition = propertyDefinition;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ComplexTypeUsage DeclaringType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition Definition { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool DefaultIsNullable
            => base.DefaultIsNullable
               && Keys == null
               && Definition.IsNullableDefault != false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool DefaultIsConcurrencyToken
            => Definition.IsConcurrencyTokenDefault == true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool DefaultIsReadOnlyAfterSave
            => base.DefaultIsReadOnlyAfterSave || Definition.IsReadOnlyAfterSaveDefault == true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool DefaultIsReadOnlyBeforeSave
            => base.DefaultIsReadOnlyBeforeSave || Definition.IsReadOnlyBeforeSaveDefault == true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool DefaultRequiresValueGenerator
            => base.DefaultRequiresValueGenerator || Definition.RequiresValueGeneratorDefault == true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool DefaultIsStoreGeneratedAlways
            => base.DefaultIsStoreGeneratedAlways || Definition.IsStoreGeneratedAlwaysDefault == true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ValueGenerated DefaultValueGenerated
            => Definition.ValueGeneratedDefault ?? ValueGenerated.Never;

        // TODO: ComplexType debug strings
        // public override string ToString() => this.ToDebugString();

        // TODO: ComplexType debug strings
        // public virtual DebugView<Property> DebugView => new DebugView<Property>(this, m => m.ToDebugString(false));

        ITypeBase IPropertyBase.DeclaringType => DeclaringType;
        IMutableTypeBase IMutablePropertyBase.DeclaringType => DeclaringType;
        IComplexTypeUsage IComplexProperty.DeclaringType => DeclaringType;
        IMutableComplexTypeUsage IMutableComplexProperty.DeclaringType => DeclaringType;

        IComplexPropertyDefinition IComplexProperty.Definition => Definition;
        IMutableComplexPropertyDefinition IMutableComplexProperty.Definition => Definition;
    }
}
