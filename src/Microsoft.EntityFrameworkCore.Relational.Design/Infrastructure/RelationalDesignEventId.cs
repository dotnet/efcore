// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public enum RelationalDesignEventId
    {
        MissingSchema = 1,
        MissingTable = 2,
        SequencesRequireName = 3,
        BadSequenceType = 4,
        UnableToGenerateEntityType = 5,
        CannotFindTypeMappingForColumn = 6,
        MissingPrimaryKey = 7,
        PrimaryKeyErrorPropertyNotFound = 8,
        UnableToScaffoldIndexMissingProperty = 9,
        ForeignKeyScaffoldErrorPrincipalTableNotFound = 10,
        ForeignKeyScaffoldErrorPropertyNotFound = 11,
        ForeignKeyScaffoldErrorPrincipalTableScaffoldingError = 12,
        ForeignKeyScaffoldErrorPrincipalKeyNotFound = 13
    }
}
