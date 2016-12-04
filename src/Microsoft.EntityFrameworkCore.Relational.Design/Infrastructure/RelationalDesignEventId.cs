// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public enum RelationalDesignEventId
    {
        MissingSchemaWarning = 1,
        MissingTableWarning,
        SequenceMustBeNamedWarning,
        SequenceTypeNotSupportedWarning,
        UnableToGenerateEntityTypeWarning,
        ColumnTypeNotMappedWarning,
        MissingPrimaryKeyWarning,
        PrimaryKeyColumnsNotMappedWarning,
        IndexColumnsNotMappedWarning,
        ForeignKeyReferencesMissingTableWarning,
        ForeignKeyColumnsNotMappedWarning,
        ForeignKeyReferencesMissingPrincipalKeyWarning,
        FoundTable,
        TableSkipped,
        FoundColumn,
        ColumnSkipped,
        FoundIndex,
        FoundIndexColumn,
        IndexColumnSkipped,
        FoundForeignKeyColumn,
        FoundSequence,
        ForeignKeyReferencesMissingTable
    }
}
