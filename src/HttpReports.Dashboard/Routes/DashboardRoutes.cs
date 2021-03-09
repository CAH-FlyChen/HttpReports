﻿using HttpReports.Dashboard.Handles; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HttpReports.Dashboard.Routes
{
    public static class DashboardRoutes
    {
        public static RouteCollection Routes { get; } 

        static DashboardRoutes()
        {
            Routes = new RouteCollection(); 

            typeof(DashboardDataHandle).GetMethods().Select(x => x.Name).ToList().ForEach(action => {

                Routes.AddRoute(new DashboardRoute("/HttpReportsData/" + action, null));

            });  

        }

    }
}
