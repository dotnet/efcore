// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public interface ISqlGenerationHelper
    {
        string StatementTerminator { get; }

        string BatchTerminator { get; }

        string GenerateParameterName([NotNull] string name);

        void GenerateParameterName([NotNull] StringBuilder builder, [NotNull] string name);

        string GenerateLiteral([CanBeNull] object value, [CanBeNull] RelationalTypeMapping typeMapping = null);

        void GenerateLiteral([NotNull] StringBuilder builder, [CanBeNull] object value, [CanBeNull] RelationalTypeMapping typeMapping = null);

        string EscapeLiteral([NotNull] string literal);

        void EscapeLiteral([NotNull] StringBuilder builder, [NotNull] string literal);

        string EscapeIdentifier([NotNull] string identifier);

        void EscapeIdentifier([NotNull] StringBuilder builder, [NotNull] string identifier);

        string DelimitIdentifier([NotNull] string identifier);

        void DelimitIdentifier([NotNull] StringBuilder builder, [NotNull] string identifier);

        string DelimitIdentifier([NotNull] string name, [CanBeNull] string schema);

        void DelimitIdentifier([NotNull] StringBuilder builder, [NotNull] string name, [CanBeNull] string schema);
    }
}
