// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Operations
{
    public class CreateSequenceOperation : MigrationOperation
    {
        public CreateSequenceOperation(
            [NotNull] string name,
            [CanBeNull] string schema,
            long startValue,
            int incrementBy,
            [CanBeNull] long? minValue,
            [CanBeNull] long? maxValue,
            [CanBeNull] string storeType,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
            : base(annotations)
        {
            // TODO: Duplicating Sequence validation?
            Check.NotEmpty(name, nameof(name));

            Name = name;
            Schema = schema;
            StartValue = startValue;
            IncrementBy = incrementBy;
            MinValue = minValue;
            MaxValue = maxValue;
            StoreType = storeType;
        }

        public virtual string Name { get;[param: NotNull]set; }
        public virtual string Schema { get;[param: CanBeNull] set; }
        public virtual long StartValue { get; set; }
        public virtual long IncrementBy { get; set; }
        public virtual long? MinValue { get; set; }
        public virtual long? MaxValue { get; set; }
        public virtual string StoreType { get;[param: CanBeNull] set; }
    }
}
