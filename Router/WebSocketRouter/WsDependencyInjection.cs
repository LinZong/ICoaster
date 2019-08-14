using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flurl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SNotiSSL;

namespace ICoaster.Router.WebSocketRouter
{
    public static class WsDependencyInjection
    {
        public static IServiceCollection AddWsRouters(this IServiceCollection service)
        {
            var routerTable = new Dictionary<string, WsControllerInfo>();

            var logger = service.BuildServiceProvider().GetService<ILogger<IServiceCollection>>();
            var routerLogger = service.BuildServiceProvider().GetService<ILogger<WsRouter>>();
            var snotiClient = service.BuildServiceProvider().GetService<SNotiClient>();
            System.Console.WriteLine($"添加到服务中的SNoti :{snotiClient.GetHashCode()}");
            var wsControllers = AppDomain.CurrentDomain.GetAssemblies()
                                                        .SelectMany(assm => assm.GetTypes()
                                                        .Where(t => t.IsClass &&
                                                                t.CustomAttributes
                                                                .Select(attr => attr.AttributeType)
                                                                .Contains(typeof(WebSocketControllerAttribute))))
                                                        .Select(x =>
                                                        {
                                                            var rootPathAttr = x.CustomAttributes.First(y => y.AttributeType == typeof(WebSocketControllerAttribute));
                                                            var pathArg = rootPathAttr.NamedArguments.FirstOrDefault(arg => arg.MemberName == "Path");
                                                            string pathStr = null;
                                                            if (pathArg != null)
                                                            {
                                                                pathStr = pathArg.TypedValue.Value as string;
                                                            }
                                                            return (x, pathStr);
                                                        });

            foreach ((Type ctol, string path) in wsControllers)
            {
                var ReqHandleMethods = ctol.GetMethods()
                                            .Where(x => x.CustomAttributes.Select(attr => attr.AttributeType)
                                                                           .Contains(typeof(SubPathAttribute)))
                                            .Select(x =>
                                            {
                                                var subPathAttr = x.CustomAttributes.First(y => y.AttributeType == typeof(SubPathAttribute));
                                                var subPathArg = subPathAttr.NamedArguments.FirstOrDefault(y => y.MemberName == "Path");
                                                string subPath = null;
                                                if (subPathArg != null)
                                                {
                                                    subPath = subPathArg.TypedValue.Value as string;
                                                }
                                                return (x, subPath);
                                            });

                foreach ((MethodInfo method, string subPath) in ReqHandleMethods)
                {
                    if (StringAllNotEmpty(path, subPath))
                    {
                        var url = "/" + Url.Combine(path, subPath);
                        logger.LogInformation($"WsController found! {url}");
                        var info = new WsControllerInfo
                        {
                            FullPath = url,
                            ControllerType = ctol,
                            HandleRequestMethod = method
                        };

                        service.AddScoped(ctol);
                        routerTable.Add(url, info);
                    }
                }
            }

            var router = new WsRouter(routerTable, service, routerLogger,snotiClient);
            return service.AddSingleton(router);
        }

        private static bool StringAllNotEmpty(params string[] strs)
        {
            return strs.All(str => !string.IsNullOrEmpty(str));
        }
    }
}