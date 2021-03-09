﻿using HttpReports.Core;
using HttpReports.Core.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpReports.Diagnostic.SqlClient
{
    public class SqlClientDiagnosticListener : IDiagnosticListener
    {
        private ILogger<SqlClientDiagnosticListener> _logger;

        private IReportsTransport _transport;

        private ISegmentContext _context;

        public SqlClientDiagnosticListener(ILogger<SqlClientDiagnosticListener> logger, IReportsTransport transport, ISegmentContext context)
        {
            _logger = logger;
            _transport = transport;
            _context = context;
        }



        public string ListenerName => "SqlClientDiagnosticListener";



        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            var activity = System.Diagnostics.Activity.Current;

            if (activity == null) return; 

            if (value.Key == "System.Data.SqlClient.WriteCommandBefore")
            {  
                var SqlCommand = value.Value.GetType().GetProperty("Command").GetValue(value.Value) as System.Data.SqlClient.SqlCommand;

                _context.Push(activity?.SpanId.ToHexString(), new Segment
                {
                    activity = activity,
                    CreateTime = DateTime.Now,
                    Value = SqlCommand

                });

            }

            if (value.Key == "System.Data.SqlClient.WriteCommandAfter")
            { 

                var SqlCommand = value.Value.GetType().GetProperty("Command").GetValue(value.Value) as System.Data.SqlClient.SqlCommand;

                _context.Push(activity?.SpanId.ToHexString(), new Segment
                {
                    activity = activity,
                    CreateTime = DateTime.Now,
                    Value = SqlCommand

                });

                Build(activity.SpanId.ToHexString());

            }

            if (value.Key == "System.Data.SqlClient.WriteCommandError")
            {

            }
        }

        public IRequestChain Build(string Id)
        {
            return null;
        }
    }
}
