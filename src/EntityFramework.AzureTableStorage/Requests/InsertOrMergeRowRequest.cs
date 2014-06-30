// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Requests
{
    public class InsertOrMergeRowRequest : TableOperationRequest
    {
        public InsertOrMergeRowRequest([NotNull] AtsTable table, [NotNull] ITableEntity entity)
            : base(
                table,
                TableOperation.InsertOrMerge(Check.NotNull(entity, "entity"))
                )
        {
        }

        public override string Name
        {
            get { return "InsertOrMergeRowRequest"; }
        }
    }
}
