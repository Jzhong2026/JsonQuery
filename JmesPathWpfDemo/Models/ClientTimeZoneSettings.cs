using Newtonsoft.Json;

namespace JmesPathWpfDemo.Models
{
    public sealed class ClientTimeZoneSettings
    {
        public string TimeZoneId { get; set; } = "Eastern Standard Time";

        public bool RespectDaylightSavings { get; set; } = true;

        public static ClientTimeZoneSettings Default => new ClientTimeZoneSettings();

        public ClientTimeZoneSettings Clone()
        {
            return new ClientTimeZoneSettings
            {
                TimeZoneId = TimeZoneId,
                RespectDaylightSavings = RespectDaylightSavings
            };
        }
    }
}