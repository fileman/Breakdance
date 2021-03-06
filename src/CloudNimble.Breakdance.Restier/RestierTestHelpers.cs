﻿using CloudNimble.Breakdance.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.Restier.Core;
using Microsoft.Restier.Publishers.OData;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using System.Xml.Linq;

namespace CloudNimble.Breakdance.Restier
{

    /// <summary>
    /// A set of methods that make it easier to pull out Restier runtime components for unit testing.
    /// </summary>
    /// <remarks>See RestierTestHelperTests.cs for more examples of how to use these methods.</remarks>
    public static class RestierTestHelpers
    {

        #region Constants

        internal const string RouteName = "test";
        private const string acceptHeader = "application/json;odata.metadata=full";

        #endregion

        #region Public Methods

        /// <summary>
        /// l
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpMethod"></param>
        /// <param name="host"></param>
        /// <param name="routeName"></param>
        /// <param name="routePrefix"></param>
        /// <param name="resource"></param>
        /// <param name="acceptHeader"></param>
        /// <param name="defaultQuerySettings"></param>
        /// <param name="timeZoneInfo"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> ExecuteTestRequest<T>(HttpMethod httpMethod, string host = WebApiConstants.Localhost, string routeName = RouteName,
            string routePrefix = WebApiConstants.RoutePrefix, string resource = null, string acceptHeader = WebApiConstants.DefaultAcceptHeader, 
            DefaultQuerySettings defaultQuerySettings = null, TimeZoneInfo timeZoneInfo = null) where T : ApiBase
        {
            var config = await GetTestableRestierConfiguration<T>(routeName, routePrefix, defaultQuerySettings, timeZoneInfo);
            var client = config.GetTestableHttpClient();
            return await client.ExecuteTestRequest(httpMethod, host, routePrefix, resource, acceptHeader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="routeName"></param>
        /// <param name="routePrefix"></param>
        /// <returns></returns>
        public static async Task<ApiBase> GetTestableApiInstance<T>(string routeName = RouteName, string routePrefix = WebApiConstants.RoutePrefix) where T : ApiBase => await GetTestableApiService<T, ApiBase>(routeName, routePrefix);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TApi"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="routeName"></param>
        /// <param name="routePrefix"></param>
        /// <returns></returns>
        public static async Task<TService> GetTestableApiService<TApi, TService>(string routeName = RouteName, string routePrefix = WebApiConstants.RoutePrefix)
             where TApi : ApiBase
             where TService : class
        {
            var config = await GetTestableRestierConfiguration<TApi>(routeName, routePrefix);
            var request = HttpClientHelpers.GetTestableHttpRequestMessage(HttpMethod.Get, WebApiConstants.Localhost, routePrefix);
            request.SetConfiguration(config);
            return request.CreateRequestContainer(routeName).GetService<TService>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="routeName"></param>
        /// <param name="routePrefix"></param>
        /// <param name="defaultQuerySettings"></param>
        /// <param name="timeZoneInfo"></param>
        /// <returns></returns>
        public static async Task<HttpConfiguration> GetTestableRestierConfiguration<T>(string routeName = RouteName, string routePrefix = WebApiConstants.RoutePrefix, DefaultQuerySettings defaultQuerySettings = null, TimeZoneInfo timeZoneInfo = null) where T : ApiBase
        {
            var config = new HttpConfiguration();

            if (defaultQuerySettings == null)
            {
                defaultQuerySettings = new DefaultQuerySettings
                {
                    EnableCount = true,
                    EnableExpand = true,
                    EnableFilter = true,
                    EnableOrderBy = true,
                    EnableSelect = true,
                    MaxTop = 10
                };
            }

            config.SetDefaultQuerySettings(defaultQuerySettings);
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.SetTimeZoneInfo(timeZoneInfo ?? TimeZoneInfo.Utc);
            await config.MapRestierRoute<T>(routeName, routePrefix);
            return config;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="routeName"></param>
        /// <param name="routePrefix"></param>
        /// <returns></returns>
        public static async Task<HttpClient> GetTestableHttpClient<T>(string routeName = RouteName, string routePrefix = WebApiConstants.RoutePrefix) where T : ApiBase
        {
            var config = await GetTestableRestierConfiguration<T>(routeName, routePrefix);
            return new HttpClient(new HttpServer(config));
        }

        /// <summary>
        /// Gets the <see cref="IEdmModel"/> instance for a given API, whether it used a custom ModelBuilder or the RestierModelBuilder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<IEdmModel> GetTestableModelAsync<T>(string routeName = RouteName, string routePrefix = WebApiConstants.RoutePrefix) where T : ApiBase
        {
            var api = await GetTestableApiInstance<T>(routeName, routePrefix);
            return await api.GetModelAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<string> GetApiMetadata<T>(string host = WebApiConstants.Localhost, string routeName = RouteName, string routePrefix = WebApiConstants.RoutePrefix) where T : ApiBase
        {
            var response = await ExecuteTestRequest<T>(HttpMethod.Get, host, routeName, routePrefix, "/$metadata");
            var result = await response.Content.ReadAsStringAsync();
            var doc = XDocument.Parse(result);
            return doc.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceDirectory"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static async Task WriteCurrentApiMetadata<T>(string sourceDirectory = "", string suffix = "ApiMetadata") where T : ApiBase
        {
            var filePath = $"{sourceDirectory}{typeof(T).Name}-{suffix}.txt";
            var result = await GetApiMetadata<T>();
            var doc = XDocument.Parse(result);
            result = doc.ToString();
            System.IO.File.WriteAllText(filePath, result);
        }

        #endregion

    }

}