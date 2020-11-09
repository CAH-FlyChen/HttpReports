﻿using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpReports.Storage.Abstractions.Models
{
    [Table(Name = "SysConfig")]
    [Index("idx_config_id", "Id", true)]
    public class DBSysConfig
    {
        [Column(IsPrimary = true)]
        public long Id { get; set; }


        [Column(IsPrimary = true, StringLength = 50)]
        public string Key { get; set; }


        [Column(IsPrimary = true, StringLength = 255)]
        public string Value { get; set; }
    }
}
