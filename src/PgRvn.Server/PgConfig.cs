using System;
using System.Collections.Generic;
using System.Text;
using PgRvn.Server.Messages;
using PgRvn.Server.Types;

namespace PgRvn.Server
{
    public static class PgConfig
    {
        // TODO: Customize these
        // TODO: Support changing these after startup by the user
        public static readonly Dictionary<string, string> ParameterStatusList = new(StringComparer.OrdinalIgnoreCase)
        {
            ["client_encoding"] = "UTF8",
            ["server_encoding"] = "UTF8", // Cannot be modified after startup
            ["server_version"] = "13.3", // Cannot be modified after startup
            ["application_name"] = "",
            ["DataStyle"] = "ISO, DMY",
            ["integer_datetimes"] = "on", // Cannot be modified after startup
            ["IntervalStyle"] = "postgres",
            ["is_superuser"] = "on",
            ["session_authorization"] = "postgres",
            ["standard_conforming_strings"] = "on",
            ["TimeZone"] = "UTC",
        };
    }
}
