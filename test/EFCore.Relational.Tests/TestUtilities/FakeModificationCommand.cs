// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class FakeModificationCommand : ModificationCommand
    {
        public FakeModificationCommand(
            string name,
            string schema,
            Func<string> generateParameterName,
            bool sensitiveLoggingEnabled,
            IReadOnlyList<ColumnModification> columnModifications)
            : base(name, schema, generateParameterName, sensitiveLoggingEnabled, null, null)
        {
            ColumnModifications = columnModifications;
        }

        public override IReadOnlyList<ColumnModification> ColumnModifications { get; }
    }
}
