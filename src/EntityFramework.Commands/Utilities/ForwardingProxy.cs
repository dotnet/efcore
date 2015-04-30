// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !DNXCORE50

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    /// <summary>
    ///     This is a small piece of Remoting magic. It enables us to invoke methods on a
    ///     remote object without knowing its actual type. The only restriction is that the
    ///     names and shapes of the types and their members must be the same on each side of
    ///     the boundary.
    /// </summary>
    public class ForwardingProxy<T> : RealProxy
    {
        private readonly MarshalByRefObject _target;

        public ForwardingProxy([NotNull] object target)
            : base(typeof(T))
        {
            Check.NotNull(target, nameof(target));

            _target = (MarshalByRefObject)target;
        }

        /// <summary>
        ///     Intercepts method invocations on the object represented by the current instance
        ///     and forwards them to the target to finish processing.
        /// </summary>
        public override IMessage Invoke([NotNull] IMessage msg)
        {
            Check.NotNull(msg, nameof(msg));

            // NOTE: This sets the wrapped message's Uri
            new MethodCallMessageWrapper((IMethodCallMessage)msg).Uri = RemotingServices.GetObjectUri(_target);

            return RemotingServices.GetEnvoyChainForProxy(_target).SyncProcessMessage(msg);
        }

        public new virtual T GetTransparentProxy() => (T)base.GetTransparentProxy();
    }
}

#endif
