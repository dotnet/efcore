using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Metadata
{
    /// <summary>
    ///     <para>
    ///         A class that exposes annotations that can be modified. Annotations allow for arbitrary metadata to be 
    ///         stored on an object.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IMutableAnnotatable : IAnnotatable
    {
        /// <summary>
        ///     Gets or sets the value of the annotation with the given name.
        /// </summary>
        /// <param name="annotationName"> The key of the annotation. </param>
        /// <returns>         
        ///     The value of the existing annotation if an annotation with the specified name already exists. Otherwise, null. 
        /// </returns>
        // ReSharper disable once AnnotationRedundancyInHierarchy
        // TODO: Fix API test to handle indexer
        new object this[[NotNull] string annotationName] { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets all annotations on the current object.
        /// </summary>
        new IEnumerable<Annotation> GetAnnotations();

        /// <summary>
        ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="annotationName"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The newly added annotation. </returns>
        Annotation AddAnnotation([NotNull] string annotationName, [NotNull] object value);

        /// <summary>
        ///     Gets the annotation with the given name, returning null if it does not exist.
        /// </summary>
        /// <param name="annotationName"> The key of the annotation to find. </param>
        /// <returns>
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, null. 
        /// </returns>
        new Annotation FindAnnotation([NotNull] string annotationName);

        /// <summary>
        ///     Removes the given annotation from this object.
        /// </summary>
        /// <param name="annotationName"> The annotation to remove. </param>
        /// <returns> The annotation that was removed. </returns>
        Annotation RemoveAnnotation([NotNull] string annotationName);
    }
}
