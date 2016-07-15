﻿using Microsoft.Owin.Cors;
using Owin;

namespace Bevelop.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
}