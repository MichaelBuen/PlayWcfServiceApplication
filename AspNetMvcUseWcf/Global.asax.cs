using System;
using System.Linq;
using System.Collections.Generic;

using AspNetMvcUseWcf.Controllers;

namespace AspNetMvcUseWcf
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            System.Web.Mvc.AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(System.Web.Http.GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(System.Web.Mvc.GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(System.Web.Routing.RouteTable.Routes);

            
            System.Web.Mvc.ControllerBuilder.Current.SetControllerFactory(new LightInjectDependencyResolver());   
        }
    }

    class LightInjectDependencyResolver : System.Web.Mvc.DefaultControllerFactory
    {
        LightInject.IServiceContainer _container;

        public LightInjectDependencyResolver()
        {
            _container = new LightInject.ServiceContainer();
            RegisterTheIocs();
        }

        protected override System.Web.Mvc.IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            using(_container.BeginScope())
            {
                System.Web.Mvc.IController ic = controllerType == null
                ? null
                : (System.Web.Mvc.IController)_container.GetInstance(controllerType);
               
                return ic;
            }
            
        }

        void RegisterTheIocs()
        {
            RegisterMvcControllers();
            RegisterWcfServices();
        }

        void RegisterWcfServices()
        {

            System.Reflection.Assembly serviceAssembly = typeof(ServiceContracts.PersonContract.IMemberService).Assembly;

            var binding = new System.ServiceModel.BasicHttpBinding();

            // http://stackoverflow.com/questions/2440608/wcf-service-client-lifetime
            IDictionary<Type,object> channelFactories = CreateChannelFactoryOnce(serviceAssembly.GetTypes().Where(x => x.IsInterface));

            // Assuming ServiceContracts assembly has interfaces only, the following code would suffice
            _container.RegisterFallback((t, s) => t.Assembly == serviceAssembly,
                factory =>
                {
                    Type serviceContractType = factory.ServiceType;

                    string serviceFullname = GetServiceFullName(serviceContractType);

                    System.ServiceModel.EndpointAddress endpointAddress = GenerateEndpointAddress(serviceFullname);

                    object channelFactory = channelFactories[serviceContractType];

                    return CreateChannelForServiceContract(channelFactory);

                });


            _container.RegisterFallback((t, s) => t == typeof(string),
                  factory =>
                  {
                      return "Hello world";
                  });
     
        }

        static object CreateChannelForServiceContract(object channelFactory)
        {
            System.Reflection.MethodInfo channelCreator = channelFactory.GetType().GetMethod("CreateChannel", new Type[] { });
            //now dynamic proxy generation using reflection
            return channelCreator.Invoke(channelFactory, null);
        }

        static System.ServiceModel.EndpointAddress GenerateEndpointAddress(string serviceFullname)
        {
            var svcAddress = "http://localhost:50549/" + serviceFullname + ".svc";
            var endpointAddress = new System.ServiceModel.EndpointAddress(svcAddress);
            return endpointAddress;
        }

        static string GetServiceFullName(Type serviceContractType)
        {
            string contractFullName = serviceContractType.FullName;
            string contractName = serviceContractType.Name;

            string[] fullnameSplit = contractFullName.Split('+');

            string serviceFullname;

            if (fullnameSplit.Length == 2) // The model service is in a schema (e.g., PersonImplementation static class)
            {
                string schemaName;
                string className;

                schemaName = fullnameSplit[0].Split('.').Last();
                className = fullnameSplit[1];

                serviceFullname = schemaName + "." + className;

                serviceFullname = serviceFullname.Replace("Contract.I", "Implementation."); // remove the I prefix, and change the schema to Implementation
                // sample output: PersonImplementation.MemberService.svc
            }
            else // domain model is not inside schema
            {
                serviceFullname = contractName.Substring(1); // remove the I prefix
            }

            return serviceFullname;
        }

        static IDictionary<Type,object> CreateChannelFactoryOnce(IEnumerable<Type> serviceContractTypes)
        {        
            IDictionary<Type, object> channelFactories = new Dictionary<Type, object>();


            foreach (var serviceContractType in serviceContractTypes)
            {
                string serviceFullname = GetServiceFullName(serviceContractType);
                System.ServiceModel.EndpointAddress endpointAddress = GenerateEndpointAddress(serviceFullname);


                var binding = new System.ServiceModel.BasicHttpBinding();
                //Get the address of the service from configuration or some other mechanism - Not shown here

                //dynamic factory generation
                object factory =
                    Activator.CreateInstance(typeof(System.ServiceModel.ChannelFactory<>)
                    .MakeGenericType(serviceContractType), binding, endpointAddress);

                channelFactories[serviceContractType] = factory;
            }

            return channelFactories;
        }


        static TService Cast<TService>(TService instance)
        {
            return (TService)instance;
        }

        void RegisterMvcControllers()
        {
            System.Reflection.Assembly assembly = typeof(HomeController).Assembly;
            
            foreach (var controller in assembly.GetTypes().Where(t => typeof(System.Web.Mvc.Controller).IsAssignableFrom(t)))
            {
                _container.Register(controller, new LightInject.PerScopeLifetime());
            }
        }


    } // class LightInjectDependencyResolver





    public static class ServiceModelHelper
    {
        public static object CreateService(
            Type contractType,
            System.ServiceModel.BasicHttpBinding basicHttpBinding,
            System.ServiceModel.EndpointAddress endpointAddress
            )
        {
            var binding = new System.ServiceModel.BasicHttpBinding();
            //Get the address of the service from configuration or some other mechanism - Not shown here

            //dynamic factory generation
            object channelFactory =
                Activator.CreateInstance(typeof(System.ServiceModel.ChannelFactory<>)
                .MakeGenericType(contractType), binding, endpointAddress);

            System.Reflection.MethodInfo channelCreator = channelFactory.GetType().GetMethod("CreateChannel", new Type[] { });
            //now dynamic proxy generation using reflection
            return channelCreator.Invoke(channelFactory, null);
        }
    }
    
}//namespace



// http://stackoverflow.com/questions/658316/runtime-creation-of-generic-funct


// http://stackoverflow.com/questions/19539871/how-to-assign-a-method-obtained-through-reflection-to-a-delegate-or-how-to-sp