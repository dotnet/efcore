// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerModelBuilder
    {
        private readonly Model _model;

        public SqlServerModelBuilder([NotNull] Model model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        public virtual SqlServerModelBuilder UseIdentity()
        {
            _model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Identity;

            return this;
        }

        public virtual SqlServerModelBuilder UseSequence()
        {
            var extensions = _model.SqlServer();

            extensions.ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;
            extensions.DefaultSequenceName = null;
            extensions.DefaultSequenceSchema = null;

            return this;
        }

        public virtual SqlServerModelBuilder UseSequence([NotNull] string name, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, "schema");

            var extensions = _model.SqlServer();

            var sequence = extensions.GetOrAddSequence(name, schema);

            extensions.ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;
            extensions.DefaultSequenceName = sequence.Name;
            extensions.DefaultSequenceSchema = sequence.Schema;

            return this;
        }

        public virtual SqlServerSequenceBuilder Sequence([CanBeNull] string name = null, [CanBeNull] string schema = null)
        {
            Check.NullButNotEmpty(name, "name");
            Check.NullButNotEmpty(schema, "schema");

            return new SqlServerSequenceBuilder(_model.SqlServer().GetOrAddSequence(name, schema));
        }
    }
}
