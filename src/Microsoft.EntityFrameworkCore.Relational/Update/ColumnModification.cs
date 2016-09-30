// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class ColumnModification
    {
        private string _parameterName;
        private string _originalParameterName;
        private readonly Func<string> _generateParameterName;

        public ColumnModification(
            [NotNull] IUpdateEntry entry,
            [NotNull] IProperty property,
            [NotNull] IRelationalPropertyAnnotations propertyAnnotations,
            [NotNull] Func<string> generateParameterName,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition,
            bool isConcurrencyToken)
        {
            Check.NotNull(entry, nameof(entry));
            Check.NotNull(property, nameof(property));
            Check.NotNull(propertyAnnotations, nameof(propertyAnnotations));
            Check.NotNull(generateParameterName, nameof(generateParameterName));

            Entry = entry;
            Property = property;
            ColumnName = propertyAnnotations.ColumnName;
            _generateParameterName = generateParameterName;
            IsRead = isRead;
            IsWrite = isWrite;
            IsKey = isKey;
            IsCondition = isCondition;
            IsConcurrencyToken = isConcurrencyToken;
        }

        public virtual IUpdateEntry Entry { get; }

        public virtual IProperty Property { get; }

        public virtual bool IsRead { get; }

        public virtual bool IsWrite { get; }

        public virtual bool IsCondition { get; }

        public virtual bool IsConcurrencyToken { get; }

        public virtual bool IsKey { get; }

        public virtual bool UseOriginalValueParameter => IsCondition && IsConcurrencyToken;

        public virtual bool UseCurrentValueParameter => IsWrite || (IsCondition && !IsConcurrencyToken);

        public virtual string ParameterName
            => _parameterName ?? (_parameterName = _generateParameterName());

        public virtual string OriginalParameterName
            => _originalParameterName ?? (_originalParameterName = _generateParameterName());

        public virtual string ColumnName { get; }

        public virtual object OriginalValue => Entry.GetOriginalValue(Property);

        public virtual object Value
        {
            get { return Entry.GetCurrentValue(Property); }
            [param: CanBeNull] set { Entry.SetCurrentValue(Property, value); }
        }
    }
}
