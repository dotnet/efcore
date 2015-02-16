// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringConfiguration
    {
        public Assembly ProviderAssembly { get; set; }
        public string ConnectionString { get; set; }
        public string OutputPath { get; set; }
        public string Namespace { get; set; }
        public string ContextClassName { get; set; }
    }
}
