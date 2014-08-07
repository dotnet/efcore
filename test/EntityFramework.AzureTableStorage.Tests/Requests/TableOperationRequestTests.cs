// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Requests
{
    public class TableOperationRequestTests
    {
        [Fact]
        public void Creates_row_request()
        {
            var request = new CreateRowRequest(new AtsTable("A"), new TableEntity());
            AssertOperationType(request.Operation, TableOperationType.Insert);
            AssertName(request);
        }

        [Fact]
        public void Update_row_request()
        {
            var request = new MergeRowRequest(new AtsTable("A"), new TableEntity());
            AssertOperationType(request.Operation, TableOperationType.Merge);
            AssertName(request);
        }

        [Fact]
        public void Delete_row_request()
        {
            var request = new DeleteRowRequest(new AtsTable("A"), new TableEntity());
            AssertOperationType(request.Operation, TableOperationType.Delete);
            AssertName(request);
        }

        private void AssertName(TableOperationRequest request)
        {
            Assert.Equal(request.GetType().Name, request.Name);
        }

        private void AssertOperationType(TableOperation operation, TableOperationType operationType)
        {
            var propInfo = typeof(TableOperation).GetProperty("OperationType", BindingFlags.NonPublic | BindingFlags.Instance);
            var type = (TableOperationType)propInfo.GetValue(operation);
            Assert.Equal(operationType, type);
        }
    }
}
