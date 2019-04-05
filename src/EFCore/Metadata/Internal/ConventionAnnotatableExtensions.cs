// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class ConventionAnnotatableExtensions
    {
        public static void SetOrRemoveAnnotation(
           [NotNull] this ConventionAnnotatable annotatable,
           [NotNull] string name,
           [CanBeNull] object value,
           ConfigurationSource configurationSource)
        {
            if (value == null)
            {
                annotatable.RemoveAnnotation(name);
            }
            else
            {
                annotatable.SetAnnotation(name, value, configurationSource);
            }
        }
    }
}
