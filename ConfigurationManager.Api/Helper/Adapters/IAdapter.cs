﻿using System.Collections.Specialized;

namespace ConfigurationManager.Api.Helper.Adapters
{
    public interface IAdapter
    {
        NameValueCollection InnerConnectionStrings { get; }
        NameValueCollection InnerAppSettings { get; }
        string AppSettings(string key);
        string ConnectionStrings(string key);
    }
}