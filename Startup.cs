using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(APSViewFnApp.Startup))]
namespace APSViewFnApp
{
    public class Startup : FunctionsStartup
    {
        public Startup() { }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var clientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET");
            var bucket = Environment.GetEnvironmentVariable("APS_BUCKET"); // Optional
            if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientSecret))
            {
                throw new ApplicationException("Missing required environment variables APS_CLIENT_ID or APS_CLIENT_SECRET.");
            }
            
            builder.Services.AddSingleton<APS>(new APS(clientID, clientSecret, bucket));
        }
    }
}
