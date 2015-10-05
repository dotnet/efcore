// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqlServerSqlGenerator : RelationalSqlGenerator
    {
        public override string BatchSeparator => "GO" + Environment.NewLine + Environment.NewLine;

        public override string EscapeIdentifier([NotNull] string identifier)
            => Check.NotEmpty(identifier, nameof(identifier)).Replace("]", "]]");

        public override string DelimitIdentifier([NotNull] string identifier)
            => $"[{EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier)))}]";

        protected override string GenerateLiteralValue([NotNull] byte[] value)
        {
            Check.NotNull(value, nameof(value));

            var stringBuilder = new StringBuilder("0x");

            foreach (var @byte in value)
            {
                stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            return stringBuilder.ToString();
        }

        protected override string GenerateLiteralValue(DateTime value)
            => $"'{value.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}'";

        protected override string GenerateLiteralValue(DateTimeOffset value)
            => $"'{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'";
    }
}
