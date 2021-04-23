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
            bool sensitiveLoggingEnabled,
            IReadOnlyList<IColumnModification> columnModifications)
            : base(new ModificationCommandParameters(name, schema, columnModifications: null, sensitiveLoggingEnabled))
        {
            //TODO: [2021-04-22] Pass columnModifications into base class?
            ColumnModifications = columnModifications;
        }

        public override IReadOnlyList<IColumnModification> ColumnModifications { get; }
    }
}
