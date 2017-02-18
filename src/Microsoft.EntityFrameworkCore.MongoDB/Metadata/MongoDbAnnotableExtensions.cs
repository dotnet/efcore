using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public static class MongoDbAnnotableExtensions
    {
        public static void SetAnnotation<TValue>([NotNull] this IAnnotatable annotatable,
            [NotNull] string annotationName,
            [CanBeNull] TValue value)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            if (string.IsNullOrWhiteSpace(annotationName))
            {
                throw new ArgumentException(message: "Annotation name cannot be null, empty, or exclusively white-space.", paramName: nameof(annotationName));
            }
            var mutableAnnotatable = annotatable as IMutableAnnotatable;
            if (mutableAnnotatable == null)
            {
                throw new InvalidOperationException($"Annotable object must be an instance of {nameof(IMutableAnnotatable)}.");
            }
            mutableAnnotatable.RemoveAnnotation(annotationName);
            if (value != null)
            {
                mutableAnnotatable.AddAnnotation(annotationName, value);
            }
        }

        public static TValue GetAnnotation<TValue>([NotNull] this IAnnotatable annotatable,
            [NotNull] string annotationName)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            if (string.IsNullOrWhiteSpace(annotationName))
            {
                throw new ArgumentException(message: "Annotation name cannot be null, empty, or exclusively white-space.", paramName: nameof(annotationName));
            }
            return (TValue)annotatable.FindAnnotation(annotationName)?.Value;
        }
    }
}