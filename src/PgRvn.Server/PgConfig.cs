using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public static class PgConfig
    {
        public static readonly Dictionary<string, string> ParameterStatusList = new()
        {
            ["client_encoding"] = "UTF8",
            ["server_encoding"] = "UTF8",
            ["server_version"] = "13.3",
            ["application_name"] = "",
            ["DataStyle"] = "ISO, DMY",
            ["integer_datetimes"] = "on",
            ["IntervalStyle"] = "postgres",
            ["is_superuser"] = "on", // TODO
            ["session_authorization"] = "postgres",
            ["standard_conforming_strings"] = "on",
            ["TimeZone"] = "Asia/Jerusalem", // TODO
        };
    }
}
