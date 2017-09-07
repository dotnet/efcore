using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Contains an ordered collection of <see cref="IModelCustomizer"/>
    /// </summary>
    public interface IModelCustomizerCollection
    {
        /// <summary>
        ///     An ordered collection of <see cref="IModelCustomizer"/>
        /// </summary>
        IEnumerable<IModelCustomizer> Items { get; }
    }
}
