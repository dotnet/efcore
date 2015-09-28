// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Update
{
    public interface IUpdateSqlGenerator
    {
        string GenerateNextSequenceValueOperation([NotNull] string name, [CanBeNull] string schema);
        void AppendBatchHeader([NotNull] StringBuilder commandStringBuilder);
        void AppendDeleteOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] ModificationCommand command);
        void AppendInsertOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] ModificationCommand command);
        void AppendUpdateOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] ModificationCommand command);
    }
}
