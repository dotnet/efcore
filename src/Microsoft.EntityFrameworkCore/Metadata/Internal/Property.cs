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
    public class Property : StructuralProperty, IMutableProperty
    {
        // Warning: Never access these fields directly as access needs to be thread-safe
        private PropertyIndexes _indexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Property(
            [NotNull] string name,
            [NotNull] Type clrType,
            [NotNull] EntityType declaringEntityType,
            ConfigurationSource configurationSource)
            : base(name, clrType, configurationSource)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            DeclaringEntityType = declaringEntityType;
            Initialize(declaringEntityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Property(
            [NotNull] PropertyInfo propertyInfo,
            [NotNull] EntityType declaringEntityType,
            ConfigurationSource configurationSource)
            : base(propertyInfo, configurationSource)
        {
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            DeclaringEntityType = declaringEntityType;
            Initialize(declaringEntityType);
        }

        private void Initialize(EntityType declaringEntityType)
        {
            Builder = new InternalPropertyBuilder(this, declaringEntityType.Model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual EntityType DeclaringType => DeclaringEntityType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Builder { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void OnPropertyNullableChanged() 
            => DeclaringEntityType.Model.ConventionDispatcher.OnPropertyNullableChanged(Builder);

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
        protected override void OnFieldInfoSet(FieldInfo oldFieldInfo)
            => DeclaringEntityType.Model.ConventionDispatcher.OnPropertyFieldChanged(Builder, oldFieldInfo);

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
        protected override bool DefaultRequiresValueGenerator
            => this.IsKey()
               && !this.IsForeignKey()
               && ValueGenerated == ValueGenerated.OnAdd;

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

        IMutableEntityType IMutableProperty.DeclaringType => DeclaringEntityType;
        IEntityType IProperty.DeclaringType => DeclaringEntityType;
        IEntityType IPropertyBase.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableProperty.DeclaringEntityType => DeclaringEntityType;

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString() => this.ToDebugString();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DebugView<Property> DebugView
            => new DebugView<Property>(this, m => m.ToDebugString(false));
    }
}
