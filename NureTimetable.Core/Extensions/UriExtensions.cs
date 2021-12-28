﻿using Flurl.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NureTimetable.Core.Extensions
{
    public static class UriExtensions
    {
        public static async Task<string> GetStringOrWebExceptionAsync(this Uri requestUri)
        {
            _ = requestUri ?? throw new ArgumentNullException(nameof(requestUri));

            try
            {
                return await requestUri.GetStringAsync();
            }
            catch (Exception ex)
            {
                throw new WebException(ex.Message, ex);
            }
        }
    }
}