using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BookingSystem.DataAccess;
using BookingSystem.Helpers;

namespace BookingSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            DataSeeder.Seed();
            ReservationLifecycle.EnsureCompletedForPastCheckouts();
        }
    }
}
