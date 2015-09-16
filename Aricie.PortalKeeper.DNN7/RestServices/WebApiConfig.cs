using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using Aricie.DNN.Modules.PortalKeeper;
using Aricie.Services;

namespace Aricie.PortalKeeper.DNN7.WebAPI
{
    public static class WebApiConfig
    {

        public static HttpConfiguration GetConfiguration()
        {
            return System.Web.Http.GlobalConfiguration.Configuration;
        }


        public static void RegisterWebHosted()
        {
            WebApiConfig.Register(GetConfiguration());
        }

        public static void Register(HttpConfiguration config)
        {
            if (PortalKeeperConfig.Instance.RestServices.Enabled)
            {
                var originalControllerSelector = config.Services.GetHttpControllerSelector();
                config.Services.Replace(typeof(IHttpControllerSelector), new DynamicControllerSelector(config, originalControllerSelector));
                var originalActionSelector = config.Services.GetActionSelector();
                config.Services.Replace(typeof(IHttpActionSelector), new DynamicActionSelector(config, originalActionSelector));
                config.Formatters.Add(new BrowserJsonFormatter());

               

                foreach (RestService objService in PortalKeeperConfig.Instance.RestServices.RestServices)
                {
                    if (objService.Enabled)
                    {
                        
                        foreach (DynamicControllerInfo objController in objService.DynamicControllers)
                        {
                            if (objController.Enabled)
                            {
                                foreach (DynamicRoute objRoute in objController.SpecificRoutes)
                                {
                                    RegisterRoute(config, objRoute);
                                }    
                            }
                        }

                        foreach (DynamicRoute objRoute in objService.SpecificRoutes)
                        {
                            RegisterRoute(config, objRoute);
                        }
                       
                    }

                }

                foreach (DynamicRoute objRoute in PortalKeeperConfig.Instance.RestServices.GlobalRoutes)
                {
                    RegisterRoute(config, objRoute);
                }
               
               
            }
        }

        private static void RegisterRoute(HttpConfiguration config, DynamicRoute  objRoute )
        {
            if (objRoute.Enabled)
            {
                if (!objRoute.DNNRoute.Enabled)
                {
                    config.Routes.MapHttpRoute(
                        name: objRoute.Name,
                        routeTemplate: objRoute.Template,
                        defaults: objRoute.Defaults.EvaluateVariables(PortalKeeperContext<RequestEvent>.Instance, PortalKeeperContext<RequestEvent>.Instance),
                        constraints: objRoute.Constraints.EvaluateVariables(PortalKeeperContext<RequestEvent>.Instance, PortalKeeperContext<RequestEvent>.Instance)
                        );
                }

            }
        }

        public static IEnumerable<string> GetSampleRoutes(string basePath, DynamicControllerInfo controller)
        {
            var toReturn = new List<string>();
            IApiExplorer apiExplorer = GetConfiguration().Services.GetApiExplorer();

            foreach (ApiDescription api in apiExplorer.ApiDescriptions)
            {
                if (DotNetNuke.Web.Api.RouteExtensions.GetName(api.Route).StartsWith(PortalKeeperContext<SimpleEngineEvent>.MODULE_NAME, StringComparison.OrdinalIgnoreCase)
                    && api.ActionDescriptor.ControllerDescriptor.ControllerName == controller.Name)
                {
                    var routeName = DotNetNuke.Web.Api.RouteExtensions.GetName(api.Route);
                    if (routeName == null)
                    {
                        routeName = "Unknown Route Name";
                    }
                    var strParams = new StringBuilder();
                    foreach (ApiParameterDescription parameterDescription in api.ParameterDescriptions)
                    {
                        strParams.Append(string.Format("[{2}] {0} {1}, ", ReflectionHelper.GetSimpleTypeName( parameterDescription.ParameterDescriptor.ParameterType), parameterDescription.Name, parameterDescription.Source.ToString()));
                    }

                    toReturn.Add(string.Format("{0} : {3} : {4} : {1}({2})"
                        , routeName 
                        , api.ActionDescriptor.ActionName
                        , strParams.ToString().Trim().Trim(',')
                        , api.HttpMethod.ToString()
                        , basePath + "/" + api.RelativePath)
                        );
                }
            }
            return toReturn;
        }


    }
}