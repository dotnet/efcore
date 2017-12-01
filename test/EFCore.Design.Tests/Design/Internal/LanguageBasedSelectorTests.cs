// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class LanguageBasedSelectorTests
    {
        [Fact]
        public void Select_works()
        {
            var vbService = new TestLanguageBasedService("VB");
            var selector = new TestLanguageBasedSelector(
                new TestLanguageBasedService("C#"),
                new TestLanguageBasedService("F#"),
                vbService);

            var result = selector.Select("VB");

            Assert.Same(vbService, result);
        }

        [Fact]
        public void Select_favors_legacy_services()
        {
            var legacyService = new TestLanguageBasedService(null);
            var selector = new TestLanguageBasedSelector(
                legacyService,
                new TestLanguageBasedService("C#"));

            var result = selector.Select("C#");

            Assert.Same(legacyService, result);
        }

        [Fact]
        public void Select_ignores_case()
        {
            var csharpService = new TestLanguageBasedService("C#");
            var selector = new TestLanguageBasedSelector(csharpService);

            var result = selector.Select("c#");

            Assert.Same(csharpService, result);
        }

        [Fact]
        public void Select_picks_csharp_when_no_language()
        {
            var csharpService = new TestLanguageBasedService("C#");
            var selector = new TestLanguageBasedSelector(
                csharpService,
                new TestLanguageBasedService("F#"),
                new TestLanguageBasedService("VB"));

            var result = selector.Select(null);

            Assert.Same(csharpService, result);
        }

        [Fact]
        public void Select_throws_when_no_service()
        {
            var selector = new TestLanguageBasedSelector(new TestLanguageBasedService("C#"));

            var ex = Assert.Throws<OperationException>(() => selector.Select("VB"));

            Assert.Equal(DesignStrings.NoLanguageService("VB", "TestLanguageBasedService"), ex.Message);
        }

        [Fact]
        public void Select_uses_last_when_multiple_services()
        {
            var lastService = new TestLanguageBasedService("C#");
            var selector = new TestLanguageBasedSelector(
                new TestLanguageBasedService("C#"),
                lastService);

            var result = selector.Select("C#");

            Assert.Same(lastService, result);
        }

        private class TestLanguageBasedSelector : LanguageBasedSelector<TestLanguageBasedService>
        {
            public TestLanguageBasedSelector(params TestLanguageBasedService[] services)
                : base(services)
            {
            }
        }

        private class TestLanguageBasedService : ILanguageBasedService
        {
            public TestLanguageBasedService(string language)
                => Language = language;

            public string Language { get; }
        }
    }
}
