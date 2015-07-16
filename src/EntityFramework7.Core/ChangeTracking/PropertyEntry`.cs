// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;

namespace Microsoft.Data.Entity.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Provides access to change tracking information and operations for a given property.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
    ///         not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TEntity"> The type of the entity the property belongs to. </typeparam>
    /// <typeparam name="TProperty"> The type of the property. </typeparam>
    public class PropertyEntry<TEntity, TProperty> : PropertyEntry
        where TEntity : class
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyEntry{TEntity, TProperty}" /> class. Instances of this class
        ///     are returned from methods when using the <see cref="ChangeTracker" /> API and it is not designed
        ///     to be directly constructed in your application code.
        /// </summary>
        /// <param name="internalEntry">  The internal entry tracking information about the entity the property belongs to. </param>
        /// <param name="name"> The name of the property. </param>
        public PropertyEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name)
            : base(internalEntry, name)
        {
        }

        public new virtual TProperty CurrentValue
        {
            get { return (TProperty)base.CurrentValue; }
            [param: CanBeNull] set { base.CurrentValue = value; }
        }

        public new virtual TProperty OriginalValue
        {
            get { return (TProperty)base.OriginalValue; }
            [param: CanBeNull] set { base.OriginalValue = value; }
        }
    }
}
