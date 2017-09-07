// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Performs additional configuration of the model in addition to what is discovered by convention.
    ///     </para>
    ///     <para>
    ///         This service does not replace the <see cref="IModelCustomizer"/> or other <see cref="IAdditionalModelCustomizer"/>
    ///     </para>
    /// </summary>
    public interface IAdditionalModelCustomizer : IModelCustomizer
    {
    }
}
