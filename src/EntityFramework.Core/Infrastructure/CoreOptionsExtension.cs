// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class CoreOptionsExtension : IDbContextOptionsExtension
    {
        private IModel _model;
        private bool _isSensitiveDataLoggingEnabled;

        public CoreOptionsExtension()
        {
        }

        public CoreOptionsExtension([NotNull] CoreOptionsExtension copyFrom)
        {
            _isSensitiveDataLoggingEnabled = copyFrom.IsSensitiveDataLoggingEnabled;
            _model = copyFrom.Model;
        }

        public virtual bool IsSensitiveDataLoggingEnabled
        {
            get { return _isSensitiveDataLoggingEnabled; }
            set { _isSensitiveDataLoggingEnabled = value; }
        }

        public virtual bool SensitiveDataLoggingWarned { get; set; }

        public virtual IModel Model
        {
            get { return _model; }
            [param: CanBeNull] set { _model = value; }
        }

        public virtual void ApplyServices(EntityFrameworkServicesBuilder builder)
        {
        }
    }
}
