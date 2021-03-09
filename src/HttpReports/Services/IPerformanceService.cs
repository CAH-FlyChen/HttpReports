﻿
using HttpReports.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpReports.Services
{
    public interface IPerformanceService
    {
        Task<Performance> GetPerformance(string Instance);  
    }
}
