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
            IUpdateEntry entry,
            IProperty property,
            IColumn column,
            Func<string> generateParameterName,
            RelationalTypeMapping typeMapping,
            bool valueIsRead,
            bool valueIsWrite,
            bool columnIsKey,
            bool columnIsCondition,
            bool sensitiveLoggingEnabled)
        {
            return new ColumnModification
                (entry,
                 property,
                 column,
                 generateParameterName,
                 typeMapping,
                 valueIsRead,
                 valueIsWrite,
                 columnIsKey,
                 columnIsCondition,
                 sensitiveLoggingEnabled);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ColumnModification CreateColumnModification(
            string columnName,
            object originalValue,
            object value,
            IProperty property,
            string columnType,
            RelationalTypeMapping typeMapping,
            bool valueIsRead,
            bool valueIsWrite,
            bool columnIsKey,
            bool columnIsCondition,
            bool sensitiveLoggingEnabled,
            bool? valueIsNullable)
        {
            return new ColumnModification
                (columnName,
                 originalValue,
                 value,
                 property,
                 columnType,
                 typeMapping,
                 valueIsRead,
                 valueIsWrite,
                 columnIsKey,
                 columnIsCondition,
                 sensitiveLoggingEnabled,
                 valueIsNullable);
        }
    }
}
