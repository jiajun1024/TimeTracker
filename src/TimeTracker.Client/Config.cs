﻿namespace TimeTracker.Client
{
    public static class Config
    {
        public const string ApiRootUrl = "https://localhost:44303/";
        public const string ApiResourceUrl = ApiRootUrl + "api/v2/";
        public const string TokenUrl = ApiRootUrl + "get-token";
    }
}
