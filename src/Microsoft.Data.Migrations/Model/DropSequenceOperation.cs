// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class DropSequenceOperation : MigrationOperation<Sequence, CreateSequenceOperation>
    {
        public DropSequenceOperation([NotNull] Sequence sequence)
            : base(Check.NotNull(sequence, "sequence"))
        {
        }

        public override CreateSequenceOperation Inverse
        {
            get { return new CreateSequenceOperation(Target); }
        }
    }
}
