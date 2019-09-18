﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http
{

    /// <summary>
    /// 
    /// </summary>
    public static class HttpResponseMessageExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task<(T Response, string ErrorContent)> DeserializeResponseAsync<T>(this HttpResponseMessage message)
        {
            if (!message.IsSuccessStatusCode)
            {
                return (default, await message.Content.ReadAsStringAsync());
            }
            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
            return (JsonConvert.DeserializeObject<T>(content), null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task<(TResponse Response, TError ErrorContent)> DeserializeResponseAsync<TResponse, TError>(this HttpResponseMessage message)
        {
            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!message.IsSuccessStatusCode)
            {
                return (default, JsonConvert.DeserializeObject<TError>(content));
            }
            return (JsonConvert.DeserializeObject<TResponse>(content), default);
        }

    }
}