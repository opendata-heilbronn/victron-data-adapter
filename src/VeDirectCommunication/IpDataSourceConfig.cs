using System.ComponentModel.DataAnnotations;

namespace VeDirectCommunication
{
    public class IpDataSourceConfig
    {
        [Required]
        public string Hostname { get; set; }
        [Required]
        public int? Port { get; set; }
    }
}