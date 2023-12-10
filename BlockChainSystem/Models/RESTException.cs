using System.Net;

namespace BlockChainSystem.Models
{
    public class RESTException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public RESTException(string message, HttpStatusCode statusCode) : base(message)
        {
            this.StatusCode = statusCode;
        }
    }
}
