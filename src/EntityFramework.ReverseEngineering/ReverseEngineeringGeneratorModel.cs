// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.CodeGeneration.CommandLine;

namespace Microsoft.Data.Entity.ReverseEngineering
{
    public class ReverseEngineeringGeneratorModel
    {
        [Argument(Description = "The full path to the assembly containing an implementation of the IDatabaseMetadataModelProvider interface for the given provider.")]
        public string ProviderAssembly { get; set; }

        [Argument(Description = "The connection string of the database.")]
        public string ConnectionString { get; set; }

        [Argument(Description = "The full path of the directory to which to output the results.")]
        public string OutputPath { get; set; }

        [Argument(Description = "The namespace to assign to the generated file.")]
        public string Namespace { get; set; }

        [Argument(Description = "The name to assign the class which inherits from DbContext.")]
        public string ContextClassName { get; set; }

        [Option(Description = "Filters which the IDatabaseMetadataModelProvider uses to decide which parts of the metadata to return.")]
        public string Filters { get; set; }

    }
}