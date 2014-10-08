// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451

using System;
using Microsoft.Data.Entity.Commands.TestUtilities;
using Xunit;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    internal interface IMagic
    {
        int Number { get; }
    }

    public class ForwardingProxyTest
    {
        [Fact]
        public void Forwards_to_instances_of_a_different_type()
        {
            using (var directory = new TempDirectory())
            {
                var source = new BuildSource
                {
                    TargetDir = directory.Path,
                    Source = @"
                        using System;

                        namespace Microsoft.Data.Entity.Commands.Utilities
                        {
                            // NOTE: This interface will have a different identity than the one above
                            internal interface IMagic
                            {
                                int Number { get; }
                            }

                            internal class Magic : MarshalByRefObject, IMagic
                            {
                                public int Number
                                {
                                    get { return 7; }
                                }
                            }
                        }
                    "
                };
                var build = source.Build();

                var domain = AppDomain.CreateDomain(
                    "ForwardingProxyTest",
                    null,
                    new AppDomainSetup { ApplicationBase = build.TargetDir });
                try
                {
                    var target = domain.CreateInstanceAndUnwrap(
                        build.TargetName,
                        "Microsoft.Data.Entity.Commands.Utilities.Magic");
                    var forwardingProxy = new ForwardingProxy<IMagic>(target);
                    var transparentProxy = forwardingProxy.GetTransparentProxy();

                    Assert.Equal(7, transparentProxy.Number);
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
            }
        }
    }
}

#endif
