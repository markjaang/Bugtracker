using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(mjaang_bugtracker.Startup))]
namespace mjaang_bugtracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
