// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if !DNXCORE50

using System;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Commands.Tests
{
    public class ExecutorTest
    {
        [Fact]
        public void Ctor_validates_arguments()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Executor(null, null));
            Assert.Equal(ex.ParamName, "logHandler");

            ex = Assert.Throws<ArgumentNullException>(() => new Executor(Mock.Of<ILogHandler>(), null));
            Assert.Equal(ex.ParamName, "args");
        }

        public class OperationBaseTests
        {
            [Fact]
            public void Execute_catches_exceptions()
            {
                var handler = new ResultHandler();
                var operation = new Mock<Executor.OperationBase>(handler) { CallBase = true };
                var error = new ArgumentOutOfRangeException("Needs to be about 20% more cool.");

                operation.Object.Execute(() => { throw error; });

                Assert.Equal(error.GetType().AssemblyQualifiedName, handler.ErrorType);
                Assert.Equal(error.Message, handler.ErrorMessage);
                Assert.NotEmpty(handler.ErrorStackTrace);
            }

            [Fact]
            public void Execute_sets_results()
            {
                var handler = new ResultHandler();
                var operation = new Mock<Executor.OperationBase>(handler) { CallBase = true };
                var result = "Twilight Sparkle";

                operation.Object.Execute(() => result);

                Assert.Equal(result, handler.Result);
            }

            [Fact]
            public void Execute_enumerates_results()
            {
                var handler = new ResultHandler();
                var operation = new Mock<Executor.OperationBase>(handler) { CallBase = true };

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

#endif
