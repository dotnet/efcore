// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class CreateSequenceOperation : MigrationOperation
    {
        private readonly Sequence _sequence;

        public CreateSequenceOperation([NotNull] Sequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            _sequence = sequence;
        }

        public virtual Sequence Sequence
        {
            get { return _sequence; }
        }

        public override void Accept([NotNull] MigrationOperationVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }
    }
}
