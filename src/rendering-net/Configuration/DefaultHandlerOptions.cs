using System;
using System.Collections.Generic;

namespace PlayWebsite.Configuration
{
    public class DefaultHandlerOptions
    {
        public static readonly string Key = "LayoutService:Handler";

        public string Name { get; set; } = default!;

        public Uri Uri { get; set; } = default!;

        public string SiteName { get; set; } = default!;

        public string ApiKey { get; set; } = default!;

        public Dictionary<string, object?> RequestDefaults { get; } = new Dictionary<string, object?>();
    }
}
