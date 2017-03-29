// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET46

using System;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Tests.Design
{
    public class OperationExecutorTest
    {
        [Fact]
        public void Ctor_validates_arguments()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new OperationExecutor(null, null));
            Assert.Equal(ex.ParamName, "reportHandler");

            ex = Assert.Throws<ArgumentNullException>(() => new OperationExecutor(Mock.Of<IOperationReportHandler>(), null));
            Assert.Equal(ex.ParamName, "args");
        }

        public class OperationBaseTests
        {
            [Fact]
            public void Execute_catches_exceptions()
            {
                var handler = new OperationResultHandler();
                var operation = new Mock<OperationExecutor.OperationBase>(handler) { CallBase = true };
                var error = new ArgumentOutOfRangeException("Needs to be about 20% more cool.");

                operation.Object.Execute(() => { throw error; });

                Assert.Equal(error.GetType().FullName, handler.ErrorType);
                Assert.Equal(error.Message, handler.ErrorMessage);
                Assert.NotEmpty(handler.ErrorStackTrace);
            }

            [Fact]
            public void Execute_sets_results()
            {
                var handler = new OperationResultHandler();
                var operation = new Mock<OperationExecutor.OperationBase>(handler) { CallBase = true };
                var result = "Twilight Sparkle";

                operation.Object.Execute(() => result);

                Assert.Equal(result, handler.Result);
            }

            [Fact]
            public void Execute_enumerates_results()
            {
                var handler = new OperationResultHandler();
                var operation = new Mock<OperationExecutor.OperationBase>(handler) { CallBase = true };

                operation.Object.Execute(() => YieldResults());

                Assert.IsType<string[]>(handler.Result);
                Assert.Equal(new[] { "Twilight Sparkle", "Princess Celestia" }, handler.Result);
            }

            private IEnumerable<string> YieldResults()
            {
                yield return "Twilight Sparkle";
                yield return "Princess Celestia";
            }
        }
    }
}
#elif NETCOREAPP2_0
#else
#error target frameworks need to be updated.
#endif
