// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            : base(name, schema, generateParameterName, sensitiveLoggingEnabled, null)
        {
            ColumnModifications = columnModifications;
        }

        public override IReadOnlyList<ColumnModification> ColumnModifications { get; }
    }
}
