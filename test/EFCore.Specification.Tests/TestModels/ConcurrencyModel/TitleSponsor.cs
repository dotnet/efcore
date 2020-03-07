// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel
{
    public class TitleSponsor : Sponsor
    {
        private readonly ILazyLoader _loader;
        private SponsorDetails _details;

        public TitleSponsor()
        {
        }

        private TitleSponsor(ILazyLoader loader)
        {
            _loader = loader;
        }

        public SponsorDetails Details
        {
            get => _loader.Load(this, ref _details);
            set => _details = value;
        }
    }
}
