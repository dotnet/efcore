// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class ColumnModificationBase
    {
        public ColumnModificationBase(
            [NotNull] string columnName,
            [CanBeNull] string parameterName,
            [CanBeNull] object originalValue,
            [CanBeNull] object value,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition,
            bool useOriginalValueParameter)
        {
            Check.NotNull(columnName, nameof(columnName));
 
            ColumnName = columnName;
            ParameterName = OriginalParameterName = parameterName;
            OriginalValue = originalValue;
            Value = value;
            IsRead = isRead;
            IsWrite = isWrite;
            IsKey = isKey;
            IsCondition = isCondition;
            UseOriginalValueParameter = useOriginalValueParameter;
        }

        public virtual string ColumnName { get; }

        public virtual string ParameterName { get; [param: CanBeNull] protected set; }

        public virtual string OriginalParameterName { get; [param: CanBeNull] protected set; }

        public virtual bool IsRead { get; }

        public virtual bool IsWrite { get; }

        public virtual bool IsKey { get; }

        public virtual bool IsCondition { get; }

        public virtual bool UseOriginalValueParameter { get; }

        public virtual object OriginalValue { get; }

        public virtual object Value { get; }
    }
}
