using System.ComponentModel.DataAnnotations;

namespace VictronDataAdapter.Impl
{
    public class IpDataSourceConfig
    {
        [Required]
        public string Hostname { get; set; }
        [Required]
        public int? Port { get; set; }
    }
}