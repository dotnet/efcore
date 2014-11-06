// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.FunctionalTests.TestModels
{
    public class SimpleEntity
    {
        public static string ShadowPropertyName = "ShadowStringProperty";
        public static string ShadowPartitionIdName = "ShadowPartitionIdProperty";

        // TODO: Change to int when SqLite supports generating int values
        public virtual long Id { get; set; }

        public virtual string StringProperty { get; set; }
    }
}
