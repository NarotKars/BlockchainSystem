using MongoDB.Driver;
using System.Data.SqlClient;
using System.Security.Authentication;
using System.Text;
using System.Xml.Xsl;

namespace BlockChainSystem
{
    public class ConnectionManager
    {
        private static string? ConnectionString { get; set; }

        public static IMongoDatabase GetMongoDb(IConfiguration configuration)
        {
            return GetMongoClient(configuration).GetDatabase("orders");
        }
         
        public static MongoClient GetMongoClient(IConfiguration configuration)
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(
              new MongoUrl(configuration["MongoDbConnectionString"])
            );
            settings.SslSettings =
              new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

            return new MongoClient(settings);
        }

    }
}
