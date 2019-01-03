namespace VictronDataAdapter.Impl
{
    public class InfluxDbConfiguration
    {
        public string Endpoint { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string Measurement { get; set; }
    }
}