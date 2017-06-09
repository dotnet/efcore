// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class ColumnModification : ColumnModificationBase
    {
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
        : base(
            Check.NotNull(propertyAnnotations, nameof(propertyAnnotations)).ColumnName,
            null,
            null,
            null,
            isRead,
            isWrite,
            isKey,
            isCondition,
            isCondition && isConcurrencyToken)
        {
            Check.NotNull(entry, nameof(entry));
            Check.NotNull(property, nameof(property));
            Check.NotNull(generateParameterName, nameof(generateParameterName));

            Entry = entry;
            Property = property;
            _generateParameterName = generateParameterName;
            IsConcurrencyToken = isConcurrencyToken;
        }

        public virtual IUpdateEntry Entry { get; }

        public virtual IProperty Property { get; }

        public virtual bool IsConcurrencyToken { get; }

        public virtual bool UseCurrentValueParameter => IsWrite || (IsCondition && !IsConcurrencyToken);

        public override string ParameterName
            => base.ParameterName ?? (base.ParameterName = _generateParameterName());

        public override string OriginalParameterName
            => base.OriginalParameterName ?? (base.OriginalParameterName = _generateParameterName());

        public override object OriginalValue => Entry.GetOriginalValue(Property);

        public override object Value => Entry.GetCurrentValue(Property);

        public virtual void SetValue([CanBeNull] object value)
        {
            Entry.SetCurrentValue(Property, value);
        }
    }
}
