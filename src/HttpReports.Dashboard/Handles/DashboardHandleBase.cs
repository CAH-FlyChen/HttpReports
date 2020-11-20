﻿ 
using HttpReports.Dashboard.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HttpReports.Dashboard.Handles
{
    public abstract class DashboardHandleBase : IDashboardHandle
    {
        protected DashboardHandleBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            ViewData = new Dictionary<string, object>();
        }

        public DashboardContext Context { get; set; }

        public IServiceProvider ServiceProvider { get; }

        public Dictionary<string, object> ViewData { get; set; }


        public virtual string Json(bool state)
        {
            BaseResult baseResult = new BaseResult
            {
                Code = state ? 1 : -1,
                Msg = state ? "ok" : "error",
                Data = null
            };

            Context.HttpContext.Response.ContentType = "application/json;charset=utf-8"; 

            return JsonConvert.SerializeObject(baseResult, new JsonSerializerSettings { 
                
                ContractResolver = new CamelCasePropertyNamesContractResolver() 
            
            });
        }

        public virtual string Json(bool state,object data)
        {
            BaseResult baseResult = new BaseResult
            {
                Code = state ? 1 : -1,
                Msg = state ? "ok" : "error",
                Data = data
            };

            Context.HttpContext.Response.ContentType = "application/json;charset=utf-8";

            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonSerializerSettings.Converters.Add(new SnowFlakeConverter());

            return JsonConvert.SerializeObject(baseResult,jsonSerializerSettings);  
        } 

        public virtual string Json(bool state, string msg,object data)
        {
            BaseResult baseResult = new BaseResult
            {
                Code = state ? 1 : -1,
                Msg = state ? "ok" : msg,
                Data = data
            };

            Context.HttpContext.Response.ContentType = "application/json;charset=utf-8"; 
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonSerializerSettings.Converters.Add(new SnowFlakeConverter()); 

            return JsonConvert.SerializeObject(baseResult, jsonSerializerSettings); 
        }  
    }

    public class SnowFlakeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(long); 

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => reader.Value; 

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteValue(((long)value).ToString()); 
    }

}
