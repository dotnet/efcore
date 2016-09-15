// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ComplexPropertyUsage : StructuralProperty, IMutableComplexPropertyUsage
    {
        // Warning: Never access these fields directly as access needs to be thread-safe
        private PropertyIndexes _indexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexPropertyUsage(
            [NotNull] ComplexPropertyDefinition definition,
            [NotNull] ComplexTypeUsage complexTypeUsage,
            ConfigurationSource configurationSource)
            : base(definition.Name, definition.ClrType, configurationSource)
        {
            DefiningProperty = definition;
            ComplexTypeUsage = complexTypeUsage;

            // TODO: Builders
            //Initialize(declaringEntityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexPropertyDefinition DefiningProperty { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeUsage ComplexTypeUsage { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType DeclaringEntityType => ComplexTypeUsage.DeclaringEntityType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ComplexType DeclaringType => DefiningProperty.DeclaringType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void OnPropertyNullableChanged()
        {
        }

        // TODO: => DeclaringEntityType.Model.ConventionDispatcher.OnPropertyNullableChanged(Builder);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void SetIsNullable(bool nullable, ConfigurationSource configurationSource)
        {
            if (nullable
                && Keys != null)
            {
                throw new InvalidOperationException(CoreStrings.CannotBeNullablePK(Name, DeclaringEntityType.DisplayName()));
            }

            base.SetIsNullable(nullable, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void SetIsReadOnlyAfterSave(bool readOnlyAfterSave, ConfigurationSource configurationSource)
        {
            if (!readOnlyAfterSave
                && Keys != null)
            {
                throw new InvalidOperationException(CoreStrings.KeyPropertyMustBeReadOnly(Name, DeclaringEntityType.DisplayName()));
            }

            base.SetIsReadOnlyAfterSave(readOnlyAfterSave, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool DefaultIsReadOnlyAfterSave
            => ((ValueGenerated == ValueGenerated.OnAddOrUpdate)
                && !IsStoreGeneratedAlways)
               || Keys != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool DefaultRequiresValueGenerator => false;
        // TODO:
            //=> this.IsKey()
            //   && !this.IsForeignKey()
            //   && ValueGenerated == ValueGenerated.OnAdd;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetContainingForeignKeys()
            => ((IProperty)this).GetContainingForeignKeys().Cast<ForeignKey>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Key> GetContainingKeys()
            => ((IProperty)this).GetContainingKeys().Cast<Key>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetContainingIndexes()
            => ((IProperty)this).GetContainingIndexes().Cast<Index>();

        IMutableComplexType IMutableComplexPropertyUsage.DeclaringType => DeclaringType;
        IComplexType IComplexPropertyUsage.DeclaringType => DeclaringType;

        IEntityType IPropertyBase.DeclaringEntityType => DeclaringEntityType;

        IMutableComplexPropertyDefinition IMutableComplexPropertyUsage.DefiningProperty => DefiningProperty;
        IComplexPropertyDefinition IComplexPropertyUsage.DefiningProperty => DefiningProperty;

        IMutableComplexTypeUsage IMutableComplexPropertyUsage.ComplexTypeUsage => ComplexTypeUsage;
        IComplexTypeUsage IComplexPropertyUsage.ComplexTypeUsage => ComplexTypeUsage;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyIndexes PropertyIndexes
        {
            get
            {
                return NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, this,
                    property => property.DeclaringEntityType.CalculateIndexes(property));
            }

            [param: CanBeNull]
            set
            {
                if (value == null)
                {
                    // This path should only kick in when the model is still mutable and therefore access does not need
                    // to be thread-safe.
                    _indexes = null;
                }
                else
                {
                    NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, value);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IKey PrimaryKey { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IKey> Keys { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IForeignKey> ForeignKeys { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IIndex> Indexes { get; [param: CanBeNull] set; }

        // TODO: public override string ToString() => this.ToDebugString();

        // TODO: public virtual DebugView<Property> DebugView => new DebugView<Property>(this, m => m.ToDebugString(false));
    }
}
