// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    public class CreateSequenceOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _sequenceName;
        private readonly long _startValue;
        private readonly long _incrementBy;
        private readonly long? _minValue;
        private readonly long? _maxValue;
        private readonly Type _type;

        public CreateSequenceOperation(
            SchemaQualifiedName sequenceName,
            long startValue = Sequence.DefaultStartValue,
            int incrementBy = Sequence.DefaultIncrement,
            [CanBeNull] long? minValue = null,
            [CanBeNull] long? maxValue = null,
            [CanBeNull] Type type = null)
        {
            // TODO: Consider duplicating the validation performed by Relational.Metadata.Sequence.

            _sequenceName = sequenceName;
            _startValue = startValue;
            _incrementBy = incrementBy;
            _minValue = minValue;
            _maxValue = maxValue;
            _type = type ?? Sequence.DefaultType;
        }

        public virtual SchemaQualifiedName SequenceName
        {
            get { return _sequenceName; }
        }

        public virtual long StartValue
        {
            get { return _startValue; }
        }

        public virtual long IncrementBy
        {
            get { return _incrementBy; }
        }

        public virtual long? MinValue
        {
            get { return _minValue; }
        }

        public virtual long? MaxValue
        {
            get { return _maxValue; }
        }

        public virtual Type Type
        {
            get { return _type; }
        }

        public override void Accept<TVisitor, TContext>(TVisitor visitor, TContext context)
        {
            Check.NotNull(visitor, "visitor");
            Check.NotNull(context, "context");

            visitor.Visit(this, context);
        }

        public override void GenerateSql(MigrationOperationSqlGenerator generator, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(batchBuilder, "batchBuilder");

            generator.Generate(this, batchBuilder);
        }

        public override void GenerateCode(MigrationCodeGenerator generator, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(stringBuilder, "stringBuilder");

            generator.Generate(this, stringBuilder);
        }
    }
}
