// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ColumnModification
    {
        private readonly LazyRef<string> _parameterName;
        private readonly LazyRef<string> _originalParameterName;
        private readonly LazyRef<string> _outputParameterName;

        public ColumnModification(
            [NotNull] InternalEntityEntry entry,
            [NotNull] IProperty property,
            [NotNull] IRelationalPropertyExtensions propertyExtensions,
            [NotNull] ParameterNameGenerator parameterNameGenerator,
            [CanBeNull] IBoxedValueReader boxedValueReader,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition)
        {
            Check.NotNull(entry, nameof(entry));
            Check.NotNull(property, nameof(property));
            Check.NotNull(propertyExtensions, nameof(propertyExtensions));
            Check.NotNull(parameterNameGenerator, nameof(parameterNameGenerator));

            Debug.Assert(!isRead || boxedValueReader != null);

            Entry = entry;
            Property = property;
            ColumnName = propertyExtensions.Column;

            _parameterName = isWrite
                ? new LazyRef<string>(parameterNameGenerator.GenerateNext)
                : new LazyRef<string>((string)null);
            _originalParameterName = isCondition
                ? new LazyRef<string>(parameterNameGenerator.GenerateNext)
                : new LazyRef<string>((string)null);
            _outputParameterName = isRead
                ? new LazyRef<string>(parameterNameGenerator.GenerateNext)
                : new LazyRef<string>((string)null);

            BoxedValueReader = boxedValueReader;

            IsRead = isRead;
            IsWrite = isWrite;
            IsKey = isKey;
            IsCondition = isCondition;
        }

        public virtual InternalEntityEntry Entry { get; }

        public virtual IProperty Property { get; }

        public virtual bool IsRead { get; }

        public virtual bool IsWrite { get; }

        public virtual bool IsCondition { get; }

        public virtual bool IsKey { get; }

        public virtual string ParameterName
            => _parameterName.Value;

        public virtual string OriginalParameterName
            => _originalParameterName.Value;

        public virtual string OutputParameterName
            => _outputParameterName.Value;

        public virtual string ColumnName { get; }

        public virtual object OriginalValue
            => Entry.OriginalValues.CanStoreValue(Property) ? Entry.OriginalValues[Property] : Value;

        public virtual object Value
        {
            get { return Entry[Property]; }
            [param: CanBeNull] set { Entry[Property] = value; }
        }

        public virtual IBoxedValueReader BoxedValueReader { get; }
    }
}
