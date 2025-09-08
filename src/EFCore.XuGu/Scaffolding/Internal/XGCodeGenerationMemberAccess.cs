// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal
{
    internal class XGCodeGenerationMemberAccess
    {
        public MemberInfo MemberInfo { get; }

        public XGCodeGenerationMemberAccess(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
        }
    }
}
