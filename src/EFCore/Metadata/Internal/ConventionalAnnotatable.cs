// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ConventionalAnnotatable : Annotatable
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual IEnumerable<ConventionalAnnotation> GetAnnotations() => base.GetAnnotations().Cast<ConventionalAnnotation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionalAnnotation AddAnnotation(
            [NotNull] string name, [CanBeNull] object value, ConfigurationSource configurationSource)
            => (ConventionalAnnotation)base.AddAnnotation(name, CreateAnnotation(name, value, configurationSource));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ConventionalAnnotation AddAnnotation([NotNull] string name, [CanBeNull] object value)
            => (ConventionalAnnotation)base.AddAnnotation(name, value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionalAnnotation SetAnnotation(
            [NotNull] string name, [CanBeNull] object value, ConfigurationSource configurationSource)
            => (ConventionalAnnotation)base.SetAnnotation(name, CreateAnnotation(name, value, configurationSource));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ConventionalAnnotation GetOrAddAnnotation([NotNull] string name, [CanBeNull] object value)
            => (ConventionalAnnotation)base.GetOrAddAnnotation(name, value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ConventionalAnnotation FindAnnotation([NotNull] string name)
            => (ConventionalAnnotation)base.FindAnnotation(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ConventionalAnnotation RemoveAnnotation([NotNull] string name)
            => (ConventionalAnnotation)base.RemoveAnnotation(name);

        private static ConventionalAnnotation CreateAnnotation(
            string name, object value, ConfigurationSource configurationSource)
            => new ConventionalAnnotation(name, value, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Annotation CreateAnnotation(string name, object value)
            => CreateAnnotation(name, value, ConfigurationSource.Explicit);
    }
}
