// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerModelExtensions : ReadOnlySqlServerModelExtensions
    {
        public SqlServerModelExtensions([NotNull] Model model)
            : base(model)
        {
        }

        [CanBeNull]
        public new virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get { return base.ValueGenerationStrategy; }
            [param: CanBeNull]
            set
            {
                // TODO: Issue #777: Non-string annotations
                ((Model)Model)[SqlServerValueGenerationAnnotation] = value == null ? null : value.ToString();
            }
        }

        public new virtual string DefaultSequenceName
        {
            get { return base.DefaultSequenceName; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Model)Model)[SqlServerDefaultSequenceNameAnnotation] = value;
            }
        }

        public new virtual string DefaultSequenceSchema
        {
            get { return base.DefaultSequenceSchema; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Model)Model)[SqlServerDefaultSequenceSchemaAnnotation] = value;
            }
        }

        public virtual Sequence AddOrReplaceSequence([NotNull] Sequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            var model = (Model)Model;
            sequence.Model = model;
            model[SqlServerSequenceAnnotation + sequence.Schema + "." + sequence.Name] = sequence.Serialize();

            return sequence;
        }

        public virtual Sequence GetOrAddSequence([CanBeNull] string name = null, [CanBeNull] string schema = null)
        {
            Check.NullButNotEmpty(name, "name");
            Check.NullButNotEmpty(schema, "schema");

            name = name ?? Sequence.DefaultName;

            return ((Model)Model).SqlServer().TryGetSequence(name, schema)
                   ?? AddOrReplaceSequence(new Sequence(name, schema));
        }
    }
}
