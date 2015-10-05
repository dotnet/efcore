// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata
{
    public interface ISqlServerModelAnnotations : IRelationalModelAnnotations
    {
        SqlServerValueGenerationStrategy? ValueGenerationStrategy { get; }
        string HiLoSequenceName { get; }
        string HiLoSequenceSchema { get; }
    }
}
