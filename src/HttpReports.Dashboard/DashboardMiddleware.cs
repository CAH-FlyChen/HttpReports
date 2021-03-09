﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HttpReports.Dashboard.Handles;
using HttpReports.Dashboard.Routes;
using System.Reflection;
using System.Linq; 
using System.IO; 
using System.Net; 
using HttpReports.Dashboard.Abstractions;
using HttpReports.Core;
using System.Text.Json;

namespace HttpReports.Dashboard
{
    public class DashboardMiddleware
    {
        private readonly RequestDelegate _next;

        private IAuthService _authService;

        private JsonSerializerOptions _jsonSetting;

        public DashboardMiddleware(RequestDelegate next, JsonSerializerOptions jsonSetting, IAuthService authService)
        {
            _next = next;
            _authService = authService;
            _jsonSetting = jsonSetting;
        }


        public async Task InvokeAsync(HttpContext httpContext)
        {   
            using (var scope = httpContext.RequestServices.CreateScope()) 
            {
                var options = scope.ServiceProvider.GetService<DashboardOptions>();

                var requestUrl = httpContext.Request.Path.Value;

                if (requestUrl == "/")
                {
                    requestUrl = $"{BasicConfig.StaticUIRoot}/index.html";
                } 
                else if (requestUrl.StartsWith("/static"))
                {
                    requestUrl = $"{BasicConfig.StaticUIRoot}{requestUrl}";
                }
                else
                {
                    if (!requestUrl.StartsWith("/HttpReportsData"))
                    {
                        await _next(httpContext);
                        return;
                    } 
                  
                } 



                //EmbeddedFile  
                if (requestUrl.Contains("."))
                {
                    await DashboardEmbeddedFiles.IncludeEmbeddedFile(httpContext, requestUrl);
                    return;
                }


                // Find Router
                var router = DashboardRoutes.Routes.FindRoute(requestUrl);

                if (router == null)
                {
                    httpContext.Response.StatusCode = 404;
                    return;
                } 


                var DashboardContext = new DashboardContext(httpContext, router, options);

                //Activate Handle  
                var handles = Assembly.GetAssembly(typeof(DashboardRoute)).GetTypes();

                var handleType = handles.FirstOrDefault(x => x.Name.Contains(router.Handle.Replace("HttpReports", "Dashboard") + "Handle"));

                var handle = scope.ServiceProvider.GetRequiredService(handleType) as IDashboardHandle;

                if (handle == null)
                {
                    httpContext.Response.StatusCode = 404;
                    return;
                }

                //Authorization
                if (!_authService.ValidToken(httpContext, handle, router))
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                    await httpContext.Response.WriteAsync("Unauthorized");

                    return;
                }

                handle.Context = DashboardContext;

                string html;

                var method = handle.GetType().GetMethod(router.Action);
                var parametersLength = method.GetParameters().Length;

                try
                {
                    if (parametersLength == 0)
                    {
                        html = await (Task<string>)method.Invoke(handle, null);
                    }
                    else
                    {
                        if (httpContext.Request.ContentLength == null && httpContext.Request.Query.Count <= 0)
                        {
                            html = await (Task<string>)method.Invoke(handle, new Object[] { null });
                        }
                        else
                        {
                            object args;
                            if (httpContext.Request.Query.Count == 1)
                            {
                                var paraType = method.GetParameters().First().ParameterType;

                                if (paraType.Name.ToLowerInvariant().Contains("string"))
                                {
                                    args = httpContext.Request.Query.FirstOrDefault().Value.ToString();
                                }
                                else
                                {
                                    var dict = new Dictionary<string, string>();
                                    httpContext.Request.Query.ToList().ForEach(x => dict.Add(x.Key, x.Value));
                                    args = System.Text.Json.JsonSerializer.Serialize<object>(System.Text.Json.JsonSerializer.Serialize(dict, _jsonSetting), _jsonSetting);
                                }
                            }
                            else if (httpContext.Request.Query.Count > 1)
                            {
                                var dict = new Dictionary<string, string>();
                                httpContext.Request.Query.ToList().ForEach(x => dict.Add(x.Key, x.Value));
                                args = System.Text.Json.JsonSerializer.Serialize(System.Text.Json.JsonSerializer.Serialize(dict, _jsonSetting),method.GetParameters().First().ParameterType,_jsonSetting);  
                            }
                            else
                            {
                                string requestJson = await GetRequestBodyAsync(httpContext);

                                var paraType = method.GetParameters().First().ParameterType;

                                args = System.Text.Json.JsonSerializer.Deserialize(requestJson, paraType,_jsonSetting);

                            }

                            html = await (Task<string>)method.Invoke(handle, new[] { args }); 

                        }  
                    }

                    await httpContext.Response.WriteAsync(html);

                }
                catch (Exception ex)
                {
                    
                } 
            }  
        }

        private async Task<string> GetRequestBodyAsync(HttpContext context)
        {
            try
            {
                string result = string.Empty;

                context.Request.EnableBuffering();

                var requestReader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8);

                result = await requestReader.ReadToEndAsync();

                context.Request.Body.Position = 0;

                return result;
            }
            catch (Exception ex)
            { 
                return string.Empty;
            }
        }
    }
}
