// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DbSetTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "context",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new DbSet<Random>(null)).ParamName);

            var set = new DbSet<Random>(new Mock<DbContext>().Object);

            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => set.Add(null)).ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.ThrowsAsync<ArgumentNullException>(() => set.AddAsync(null)).Result.ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(
                    () => set.AddAsync(null, new CancellationToken()).GetAwaiter().GetResult()).ParamName);

            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => set.Update(null)).ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.ThrowsAsync<ArgumentNullException>(() => set.UpdateAsync(null)).Result.ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.ThrowsAsync<ArgumentNullException>(() => set.UpdateAsync(null, new CancellationToken())).Result.ParamName);
        }

        [Fact]
        public void Can_add_new_entities_to_context()
        {
            var contextMock = CreateContextMock();
            contextMock.Setup(m => m.Add(It.IsAny<Random>())).Returns<Random>(e => e);

            var entity = new Random();
            Assert.Same(entity, new DbSet<Random>(contextMock.Object).Add(entity));

            contextMock.Verify(m => m.Add(entity));
        }

        [Fact]
        public void Can_add_new_entities_to_context_async()
        {
            var contextMock = CreateContextMock();
            contextMock.Setup(m => m.AddAsync(It.IsAny<Random>(), It.IsAny<CancellationToken>()))
                .Returns<Random, CancellationToken>((e, c) => Task.FromResult(e));

            var entity = new Random();
            Assert.Same(entity, new DbSet<Random>(contextMock.Object).AddAsync(entity).Result);

            contextMock.Verify(m => m.AddAsync(entity, CancellationToken.None));
        }

        [Fact]
        public void Can_add_new_entities_to_context_async_with_token()
        {
            var contextMock = CreateContextMock();
            contextMock.Setup(m => m.AddAsync(It.IsAny<Random>(), It.IsAny<CancellationToken>()))
                .Returns<Random, CancellationToken>((e, c) => Task.FromResult(e));

            var entity = new Random();
            var cancellationToken = new CancellationToken();
            Assert.Same(entity, new DbSet<Random>(contextMock.Object).AddAsync(entity, cancellationToken).Result);

            contextMock.Verify(m => m.AddAsync(entity, cancellationToken));
        }

        [Fact]
        public void Can_add_existing_entities_for_update_to_context()
        {
            var contextMock = CreateContextMock();
            contextMock.Setup(m => m.Update(It.IsAny<Random>())).Returns<Random>(e => e);

            var entity = new Random();
            Assert.Same(entity, new DbSet<Random>(contextMock.Object).Update(entity));

            contextMock.Verify(m => m.Update(entity));
        }

        [Fact]
        public void Can_add_existing_entities_for_update_to_context_async()
        {
            var contextMock = CreateContextMock();
            contextMock.Setup(m => m.UpdateAsync(It.IsAny<Random>(), It.IsAny<CancellationToken>()))
                .Returns<Random, CancellationToken>((e, c) => Task.FromResult(e));

            var entity = new Random();
            Assert.Same(entity, new DbSet<Random>(contextMock.Object).UpdateAsync(entity).Result);

            contextMock.Verify(m => m.UpdateAsync(entity, CancellationToken.None));
        }

        [Fact]
        public void Can_add_existing_entities_for_update_to_context_async_with_token()
        {
            var contextMock = CreateContextMock();
            contextMock.Setup(m => m.UpdateAsync(It.IsAny<Random>(), It.IsAny<CancellationToken>()))
                .Returns<Random, CancellationToken>((e, c) => Task.FromResult(e));

            var entity = new Random();
            var cancellationToken = new CancellationToken();
            Assert.Same(entity, new DbSet<Random>(contextMock.Object).UpdateAsync(entity, cancellationToken).Result);

            contextMock.Verify(m => m.UpdateAsync(entity, cancellationToken));
        }

        private static Mock<DbContext> CreateContextMock()
        {
            var configMock = new Mock<DbContextConfiguration>();
            var contextMock = new Mock<DbContext>();
            configMock.Setup(m => m.Context).Returns(contextMock.Object);
            contextMock.Setup(m => m.Configuration).Returns(configMock.Object);
            return contextMock;
        }
    }
}
