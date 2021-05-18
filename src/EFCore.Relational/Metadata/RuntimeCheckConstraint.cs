// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a check constraint in the <see cref="IEntityType" />.
    /// </summary>
    public class RuntimeCheckConstraint : AnnotatableBase, ICheckConstraint
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RuntimeCheckConstraint"/> class.
        /// </summary>
        /// <param name="name"> The constraint name. </param>
        /// <param name="entityType"> The affected entity type. </param>
        /// <param name="sql"> The SQL string. </param>
        public RuntimeCheckConstraint(
            string name,
            RuntimeEntityType entityType,
            string sql)
        {
            EntityType = entityType;
            Name = name;
            Sql = sql;
        }

        /// <summary>
        ///     Gets the entity type on which this check constraint is defined.
        /// </summary>
        public virtual RuntimeEntityType EntityType { get; }

        /// <summary>
        ///     Gets the name of the check constraint in the database.
        /// </summary>
        public virtual string Name { get; }

        private string Sql { get; }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        public override string ToString()
            => ((ICheckConstraint)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual DebugView DebugView
            => new(
                () => ((ICheckConstraint)this).ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => ((ICheckConstraint)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <inheritdoc/>
        IReadOnlyEntityType IReadOnlyCheckConstraint.EntityType
        {
            [DebuggerStepThrough]
            get => EntityType;
        }

        /// <inheritdoc/>
        IEntityType ICheckConstraint.EntityType
        {
            [DebuggerStepThrough]
            get => EntityType;
        }

        /// <inheritdoc/>
        string IReadOnlyCheckConstraint.Sql
        {
            [DebuggerStepThrough]
            get => Sql;
        }
    }
}
