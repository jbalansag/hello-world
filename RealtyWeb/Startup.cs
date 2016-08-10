using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RealtyWeb.Startup))]
namespace RealtyWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}