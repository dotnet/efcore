// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Utilities
{
    public class GraphTest
    {
        [Fact]
        public void AddVertices_calls_AddVertex()
        {
            var graphMock = new Mock<Graph<object>> { CallBase = true };

            var vertex = new object();

            graphMock.Object.AddVertices(new[] { vertex, vertex });

            graphMock.Verify(m => m.AddVertex(vertex), Times.Exactly(2));
        }

        [Fact]
        public void AddEdges_calls_AddEdge()
        {
            var graphMock = new Mock<Graph<object>> { CallBase = true };

            var first = new object();
            var second = new object();

            graphMock.Object.AddEdges(new Dictionary<object, object> { { first, second }, { second, first } });

            graphMock.Verify(m => m.AddEdge(first, second), Times.Once);
            graphMock.Verify(m => m.AddEdge(second, first), Times.Once);
        }
    }
}
