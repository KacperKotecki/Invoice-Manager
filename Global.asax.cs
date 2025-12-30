using Autofac;
using Autofac.Integration.Mvc;
using Invoice_Manager.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Invoice_Manager
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var builder = new ContainerBuilder();

            // 1. Register MVC Controllers
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // 2. Register Database Context (One instance per HTTP Request)
            builder.RegisterType<ApplicationDbContext>().AsSelf().InstancePerRequest();

            // 3. Register Repositories (Scan assembly for classes ending with "Repository")
            builder.RegisterAssemblyTypes(typeof(MvcApplication).Assembly)
                   .Where(t => t.Name.EndsWith("Repository"))
                   .AsSelf()
                   .InstancePerRequest();

            // 4. Register Identity Components
            builder.Register(c => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>())
                   .AsSelf()
                   .InstancePerRequest();

            builder.Register(c => HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>())
                   .AsSelf()
                   .InstancePerRequest();

            // Rejestracja AuthenticationManager (bez zmian)
            builder.Register(c => HttpContext.Current.GetOwinContext().Authentication)
                   .As<IAuthenticationManager>();

            // 5. Register Authentication Manager (from Owin Context)
            builder.Register(c => HttpContext.Current.GetOwinContext().Authentication)
                   .As<IAuthenticationManager>();

            // 6. Build and Set Resolver
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            // --- AUTOFAC CONFIGURATION END ---
        }
    }
}
