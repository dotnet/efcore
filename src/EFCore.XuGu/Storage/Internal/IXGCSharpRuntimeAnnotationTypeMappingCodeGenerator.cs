// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

public interface IXGCSharpRuntimeAnnotationTypeMappingCodeGenerator
{
    void Create(
        CSharpRuntimeAnnotationCodeGeneratorParameters codeGeneratorParameters,
        CSharpRuntimeAnnotationCodeGeneratorDependencies codeGeneratorDependencies);
}
