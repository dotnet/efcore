// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities
{
    public class FakeModificationCommand : ModificationCommand
    {
        public FakeModificationCommand(
            string name,
            string schema,
            Func<string> generateParameterName,
            IRelationalAnnotationProvider annotationProvider,
            bool sensitiveLoggingEnabled,
            IReadOnlyList<ColumnModification> columnModifications)
            : base(name, schema, generateParameterName, annotationProvider, sensitiveLoggingEnabled, null)
        {
            ColumnModifications = columnModifications;
        }

        public override IReadOnlyList<ColumnModification> ColumnModifications { get; }
    }
}
