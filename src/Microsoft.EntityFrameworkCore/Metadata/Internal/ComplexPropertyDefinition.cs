// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ComplexPropertyDefinition : StructuralProperty, IMutableComplexPropertyDefinition
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexPropertyDefinition(
            [NotNull] string name,
            [NotNull] Type clrType,
            [NotNull] ComplexType declaringType,
            ConfigurationSource configurationSource)
            : base(name, clrType, configurationSource)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(declaringType, nameof(declaringType));

            DeclaringType = declaringType;

            // TODO: ComplexType builders
            //Initialize(declaringType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexPropertyDefinition(
            [NotNull] PropertyInfo propertyInfo,
            [NotNull] ComplexType declaringType,
            ConfigurationSource configurationSource)
            : base(propertyInfo, configurationSource)
        {
            Check.NotNull(declaringType, nameof(declaringType));

            DeclaringType = declaringType;

            // TODO: ComplexType builders
            //Initialize(declaringType);
        }

        // TODO: ComplexType builders
        //private void Initialize(ComplexType declaringType)
        //{
        //    Builder = new InternalPropertyBuilder(this, declaringType.Model.Builder);
        //}

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ComplexType DeclaringType { get; }

        // TODO: ComplexType builders
        //public virtual InternalPropertyBuilder Builder { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void OnPropertyNullableChanged()
        {
        }

        // TODO: ComplexType builders
        //    => DeclaringEntityType.Model.ConventionDispatcher.OnPropertyNullableChanged(Builder);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void SetIsNullable(bool nullable, ConfigurationSource configurationSource)
        {
            // TODO: Throw if definition is being used somewhere as a key
            base.SetIsNullable(nullable, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void OnFieldInfoSet(FieldInfo oldFieldInfo)
        {
        }

        // TODO: ComplexType builders
        //    => DeclaringEntityType.Model.ConventionDispatcher.OnPropertyFieldChanged(Builder, oldFieldInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void SetIsReadOnlyAfterSave(bool readOnlyAfterSave, ConfigurationSource configurationSource)
        {
            // TODO: Check if property is used in any key
            base.SetIsReadOnlyAfterSave(readOnlyAfterSave, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool DefaultIsReadOnlyAfterSave
            // TODO: Check is used in any key
            => (ValueGenerated == ValueGenerated.OnAddOrUpdate)
               && !IsStoreGeneratedAlways;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool DefaultRequiresValueGenerator
            => ValueGenerated == ValueGenerated.OnAdd; // TODO: Find if this is being used as a key/fk somewhere

        IMutableComplexType IMutableComplexPropertyDefinition.DeclaringType => DeclaringType;
        IComplexType IComplexPropertyDefinition.DeclaringType => DeclaringType;

        // TODO:
        //public override string ToString() => this.ToDebugString();

        // TODO:
        //public virtual DebugView<Property> DebugView
        //    => new DebugView<Property>(this, m => m.ToDebugString(false));
    }
}
