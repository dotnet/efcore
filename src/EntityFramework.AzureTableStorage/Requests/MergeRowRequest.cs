// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Requests
{
    public class MergeRowRequest : TableOperationRequest
    {
        public MergeRowRequest([NotNull] AtsTable table, [NotNull] ITableEntity entity)
            : base(
                table,
                TableOperation.Merge(ResetETag(Check.NotNull(entity, "entity")))
                )
        {
        }

        private static ITableEntity ResetETag(ITableEntity entity)
        {
            entity.ETag = entity.ETag ?? "*";
            return entity;
        }

        public override string Name
        {
            get { return "MergeRowRequest"; }
        }
    }
}
