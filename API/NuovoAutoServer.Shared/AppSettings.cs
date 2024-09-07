namespace NuovoAutoServer.Shared
{

    public class AppSettings : IAppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public ApiProvider VehicleDatabasesApiProvider { get; set; }
        public ApiProvider VehicleDatabasesReportApiProvider { get; set; }
        public bool LogDbDiagnosticts { get; set; }
        public int CacheExpirationTimeInHours { get; set; }
        public RateLimiting RateLimiting { get; set; }
        public EmailConfig EmailConfig { get; set; }
    }

    public interface IAppSettings
    {
        ConnectionStrings ConnectionStrings { get; set; }
        ApiProvider VehicleDatabasesApiProvider { get; set; }
    }

    public class ConnectionStrings
    {
        public CosmosDbConnection CosmosDbContextConnection { get; set; }
        public string CloudSorageConnection { get; set; }
    }

    public class CosmosDbConnection
    {
        public string Name { get; set; }
        public string DbName { get; set; }
    }

    public class ApiProvider
    {
        public string BaseUrl { get; set; }
        public string AuthKey { get; set; }
    }

    public class RateLimiting
    {
        public string WhitelistedIPs { get; set; }
        public int RequestsLimit { get; set; }
        public int WindowDurationInHours { get; set; }
    }

    public class EmailConfig
    {
        public string SenderEmail { get; set; }
        public string SenderPwd { get; set; }
        public string SenderHost { get; set; }
    }
}
