// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Names for well-known model annotations. Applications should not use these names
    ///     directly, but should instead use API surface to access these features.
    ///     They are exposed here for use by database providers and conventions.
    /// </summary>
    public static class CoreAnnotationNames
    {
        /// <summary>
        ///     Indicates the maximum length of the annotated item.
        /// </summary>
        public const string MaxLength = "MaxLength";

        /// <summary>
        ///     Indicates that the annotated item supports Unicode.
        /// </summary>
        public const string Unicode = "Unicode";

        /// <summary>
        ///     Indicates the product version used to build the model.
        /// </summary>
        public const string ProductVersion = "ProductVersion";

        /// <summary>
        ///     The name of annotations that specify a <see cref="ValueGeneration.ValueGeneratorFactory" /> to use.
        /// </summary>
        public const string ValueGeneratorFactory = "ValueGeneratorFactory";

        /// <summary>
        ///     Indicates the <see cref="EntityFrameworkCore.PropertyAccessMode" /> for the annotated item.
        /// </summary>
        public const string PropertyAccessMode = "PropertyAccessMode";

        /// <summary>
        ///     Indicates the special <see cref="EntityFrameworkCore.PropertyAccessMode" /> for annotated navigation properties.
        /// </summary>
        public const string NavigationAccessMode = "NavigationAccessMode";

        /// <summary>
        ///     Indicates the <see cref="EntityFrameworkCore.ChangeTrackingStrategy" /> used for entities in the model.
        /// </summary>
        public const string ChangeTrackingStrategy = "ChangeTrackingStrategy";

        /// <summary>
        ///     Used while model building to keep a reference to owned types.
        /// </summary>
        public const string OwnedTypes = "OwnedTypes";

        /// <summary>
        ///     Indicates the <see cref="Metadata.ConstructorBinding" /> to use for the annotated item.
        /// </summary>
        public const string ConstructorBinding = "ConstructorBinding";

        /// <summary>
        ///     Indicates the <see cref="Storage.CoreTypeMapping" /> to use for the annotated item.
        /// </summary>
        public const string TypeMapping = "TypeMapping";

        /// <summary>
        ///     Indicates the <see cref="Storage.CoreTypeMapping" /> to use for the annotated item.
        /// </summary>
        public const string ValueConverter = "ValueConverter";

        /// <summary>
        ///     Indicates the <see cref="ChangeTracking.ValueComparer" /> to use for the annotated item.
        /// </summary>
        public const string ValueComparer = "ValueComparer";

        /// <summary>
        ///     Indicates the <see cref="ChangeTracking.ValueComparer" /> to use for the annotated item when used as a key.
        /// </summary>
        public const string KeyValueComparer = "KeyValueComparer";

        /// <summary>
        ///     Indicates the <see cref="ChangeTracking.ValueComparer" /> when structural, as opposed to reference, comparison is required.
        /// </summary>
        public const string StructuralValueComparer = "StructuralValueComparer";

        /// <summary>
        ///     Indicates the <see cref="PropertySaveBehavior" /> for a property after the entity is saved to the database.
        /// </summary>
        public const string AfterSaveBehavior = "AfterSaveBehavior";

        /// <summary>
        ///     Indicates the <see cref="PropertySaveBehavior" /> for a property before the entity is saved to the database.
        /// </summary>
        public const string BeforeSaveBehavior = "BeforeSaveBehavior";

        /// <summary>
        ///     Indicates the LINQ expression filter automatically applied to queries for this entity type.
        /// </summary>
        public const string QueryFilter = "QueryFilter";

        /// <summary>
        ///     Indicates the LINQ query used as the default source for queries of this type.
        /// </summary>
        public const string DefiningQuery = "DefiningQuery";

        /// <summary>
        ///     Indicates whether the navigation should be eager loaded by default.
        /// </summary>
        public const string EagerLoaded = "EagerLoaded";

        /// <summary>
        ///     Indicates the <see cref="System.Type" /> used by the provider for the annotated item.
        /// </summary>
        public const string ProviderClrType = "ProviderClrType";
    }
}
