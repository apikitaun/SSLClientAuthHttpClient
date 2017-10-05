using CentralServerConceptTest.Bussiness;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CentralServerConceptTest.Startup))]
namespace CentralServerConceptTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            CustomHttpClient.SetServerCertificateValidation("CN=DESKTOP-L1FD9CA.NA.LOCAL",@"c:\work\client.cer");
            ConfigureAuth(app);
        }
    }
}
