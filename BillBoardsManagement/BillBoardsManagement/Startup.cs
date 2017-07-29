using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BillBoardsManagement.Startup))]
namespace BillBoardsManagement
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
