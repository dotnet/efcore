using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Internal
{
    public interface IModelBuilderConventionSource
    {
        IReadOnlyList<IModelBuilderConvention> GetConventions();
    }
}