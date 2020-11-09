﻿using System;
using System.Threading.Tasks;
using HttpReports.Core;
using HttpReports.Storage.Abstractions;
using HttpReports.Storage.PostgreSQL;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpReports.Test
{
    [TestClass]
    public class PostgreSQLStorageTest : StorageTest<IHttpReportsStorage>
    {
        private PostgreSQLStorage _storage;

        public override IHttpReportsStorage Storage => _storage;

        [TestInitialize]
        public override async Task Init()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging();

            services.Configure<PostgreStorageOptions>(o =>
            {
                o.ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=123456;Database=HttpReports;";
                o.DeferSecond = 5;
                o.DeferThreshold = 5;


            });
            services.AddTransient<PostgreSQLStorage>(); 

            _storage = services.BuildServiceProvider().GetRequiredService<PostgreSQLStorage>();
            await _storage.InitAsync();
        }


        [TestMethod]
        public new async Task GetRequestInfoDetail()
        {
            var ids = new[] { 0 };

            var id = ids[new Random().Next(0, ids.Length - 1)];

            var result = await Storage.GetRequestInfo(id);

            Assert.IsNotNull(result);

        }

    }
}