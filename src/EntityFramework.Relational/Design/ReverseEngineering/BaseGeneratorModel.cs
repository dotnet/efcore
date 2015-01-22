// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class BaseGeneratorModel
    {
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public string ProviderAssembly { get; set; }
        public string ConnectionString { get; set; }
        public string Filters { get; set; }
    }
}
