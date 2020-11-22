﻿using HttpReports;
using HttpReports.Core; 
using HttpReprots.Collector.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IApplicationBuilder UseHttpReportsHttpCollector(this IApplicationBuilder app)

            => app.Map(BasicConfig.TransportPath, builder 
                
                => builder.UseMiddleware<HttpCollectorMiddleware>());  

    }
}
