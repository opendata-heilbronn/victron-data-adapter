using System.ComponentModel.DataAnnotations;

namespace VictronDataAdapter
{
    public class InfluxDbConfiguration
    {
        [Required]
        public string Endpoint { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Database { get; set; }
        [Required]
        public string Measurement { get; set; }
    }
}