// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerModelAnnotations : ReadOnlySqlServerModelAnnotations
    {
        public SqlServerModelAnnotations([NotNull] Model model)
            : base(model)
        {
        }

        [CanBeNull]
        public new virtual SqlServerIdentityStrategy? IdentityStrategy
        {
            get { return base.IdentityStrategy; }
            [param: CanBeNull]
            set
            {
                // TODO: Issue #777: Non-string annotations
                ((Model)Model)[SqlServerValueGenerationAnnotation] = value?.ToString();
            }
        }

        public new virtual string HiLoSequenceName
        {
            get { return base.HiLoSequenceName; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((Model)Model)[SqlServerHiLoSequenceNameAnnotation] = value;
            }
        }

        public new virtual string HiLoSequenceSchema
        {
            get { return base.HiLoSequenceSchema; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((Model)Model)[SqlServerHiLoSequenceSchemaAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual int? HiLoSequencePoolSize
        {
            get { return base.HiLoSequencePoolSize; }
            [param: CanBeNull] set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Internal.Strings.HiLoBadPoolSize);
                }

                ((Model)Model)[SqlServerHiLoSequencePoolSizeAnnotation] = value;
            }
        }

        public virtual Sequence AddOrReplaceSequence([NotNull] Sequence sequence)
        {
            Check.NotNull(sequence, nameof(sequence));

            var model = (Model)Model;
            sequence.Model = model;
            model[SqlServerSequenceAnnotation + sequence.Schema + "." + sequence.Name] = sequence.Serialize();

            return sequence;
        }

        public virtual Sequence GetOrAddSequence([CanBeNull] string name = null, [CanBeNull] string schema = null)
        {
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            name = name ?? Sequence.DefaultName;

            return ((Model)Model).SqlServer().TryGetSequence(name, schema)
                   ?? AddOrReplaceSequence(new Sequence(name, schema));
        }

        public virtual void RemoveSequence([NotNull] Sequence sequence)
        {
            Check.NotNull(sequence, nameof(sequence));

            var model = (Model)Model;
            model[SqlServerSequenceAnnotation + sequence.Schema + "." + sequence.Name] = null;
        }

        public virtual void RemoveSequence([CanBeNull] string name = null, [CanBeNull] string schema = null)
        {
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            name = name ?? HiLoSequenceName;
            schema = schema ?? HiLoSequenceSchema;

            var model = (Model)Model;
            model[SqlServerSequenceAnnotation + schema + "." + name] = null;
        }
    }
}
