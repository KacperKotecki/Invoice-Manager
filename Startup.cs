using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Invoice_Manager.Startup))]
namespace Invoice_Manager
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
