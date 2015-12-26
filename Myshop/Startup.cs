using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Myshop.Startup))]
namespace Myshop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
