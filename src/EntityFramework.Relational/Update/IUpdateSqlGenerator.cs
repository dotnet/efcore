// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Update
{
    public interface IUpdateSqlGenerator
    {
        string BatchCommandSeparator { get; }
        string BatchSeparator { get; }

        string DelimitIdentifier([NotNull] string identifier);
        string DelimitIdentifier([NotNull] string name, [CanBeNull] string schema);
        string EscapeLiteral([NotNull] string literal);
        string GenerateLiteral([CanBeNull] object literal);
        string GenerateLiteral([NotNull] byte[] literal);
        string GenerateLiteral([NotNull] string literal);
        string GenerateLiteral(bool literal);
        string GenerateLiteral(char literal);
        string GenerateLiteral(DateTime literal);
        string GenerateLiteral(DateTimeOffset literal);
        string GenerateLiteral<T>([CanBeNull] T? literal) where T : struct;
        string GenerateNextSequenceValueOperation([NotNull] string name, [CanBeNull] string schema);
        void AppendBatchHeader([NotNull] StringBuilder commandStringBuilder);
        void AppendDeleteOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] ModificationCommand command);
        void AppendInsertOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] ModificationCommand command);
        void AppendUpdateOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] ModificationCommand command);
    }
}
