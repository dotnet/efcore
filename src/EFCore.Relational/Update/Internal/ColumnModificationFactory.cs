// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    /// </summary>
    public class ColumnModificationFactory:IColumnModificationFactory
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ColumnModification CreateColumnModification(
            [NotNull] IUpdateEntry entry,
            [NotNull] IProperty property,
            [NotNull] IColumn column,
            [NotNull] Func<string> generateParameterName,
            [NotNull] RelationalTypeMapping typeMapping,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition,
            bool sensitiveLoggingEnabled)
        {
            return new ColumnModification
                (entry,
                 property,
                 column,
                 generateParameterName,
                 typeMapping,
                 isRead,
                 isWrite,
                 isKey,
                 isCondition,
                 sensitiveLoggingEnabled);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ColumnModification CreateColumnModification(
            [NotNull] string columnName,
            [CanBeNull] object originalValue,
            [CanBeNull] object value,
            [CanBeNull] IProperty property,
            [CanBeNull] string columnType,
            [CanBeNull] RelationalTypeMapping typeMapping,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition,
            bool sensitiveLoggingEnabled,
            bool? isNullable)
        {
            return new ColumnModification
                (columnName,
                 originalValue,
                 value,
                 property,
                 columnType,
                 typeMapping,
                 isRead,
                 isWrite,
                 isKey,
                 isCondition,
                 sensitiveLoggingEnabled,
                 isNullable);
        }
    }
}
