using System;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

using AspNetMvcUseWcf.Controllers;

namespace AspNetMvcUseWcf
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            
            ControllerBuilder.Current.SetControllerFactory(new LightInjectDependencyResolver());   
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

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
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


            //_container.RegisterFallback((t, s) => typeof(Controller).IsAssignableFrom(t), factory =>
            //    Activator.CreateInstance(factory.ServiceType)
            //    );

            //foreach (var contractType in serviceAssembly.GetTypes())
            //{
            //    var svcAddress = "http://localhost:61930/" + contractType.Name.Substring(1) + ".svc";
            //    var endpointAddress = new System.ServiceModel.EndpointAddress(svcAddress);

            //    object instance = ServiceModelHelper.CreateService(contractType, binding, endpointAddress);
            //    _container.RegisterInstance(contractType, instance);
            //}


            //_container.RegisterFallback((t, s) => t.Assembly == serviceAssembly, factory =>
            //    {
            //        var svcAddress = "http://localhost:61930/" + factory.ServiceType.Name.Substring(1) + ".svc";
            //        var endpointAddress = new System.ServiceModel.EndpointAddress(svcAddress);

            //        return ServiceModelHelper.CreateService(factory.ServiceType, binding, endpointAddress);
            //    }
            // );            





            // Assuming ServiceContracts assembly has interfaces only, the following code would suffice
            _container.RegisterFallback((t, s) => t.Assembly == serviceAssembly,
                factory =>
                {
                    string fullName = factory.ServiceType.FullName;

                    string[] fullnameSplit = fullName.Split('+');

                    string schemaName;
                    string className;

                    string serviceFullname;

                    if (fullnameSplit.Length == 2) // The model service is in a schema (e.g., PersonImplementation static class)
                    {
                        schemaName = fullnameSplit[0].Split('.').Last();
                        className = fullnameSplit[1];

                        serviceFullname = schemaName + "." + className;

                        serviceFullname = serviceFullname.Replace("Contract.I", "Implementation."); // remove the I prefix, and change the schema to Implementation
                        // sample output: PersonImplementation.MemberService.svc
                    }
                    else // domain model is not inside schema
                    {
                        serviceFullname = factory.ServiceType.Name.Substring(1); // remove the I prefix
                    }



                    var svcAddress = "http://localhost:50549/" + serviceFullname + ".svc";
                    var endpointAddress = new System.ServiceModel.EndpointAddress(svcAddress);

                    object instance = ServiceModelHelper.CreateService(factory.ServiceType, binding, endpointAddress);

                    return instance;
                });


            _container.RegisterFallback((t, s) => t == typeof(string),
                  factory =>
                  {
                      return "Hello world";
                  });
     
        }


        static TService Cast<TService>(TService instance)
        {
            return (TService)instance;
        }

        void RegisterMvcControllers()
        {
            System.Reflection.Assembly assembly = typeof(HomeController).Assembly;
            
            foreach (var controller in assembly.GetTypes().Where(t => typeof(Controller).IsAssignableFrom(t)))
            {
                _container.Register(controller, new LightInject.PerScopeLifetime());
            }
        }


    } // class LightInjectDependencyResolver




    //// Was using DryIoC before...

    //public static class ResolverExtensionHelper
    //{
    //    public static void RegisterDelegate(this DryIoc.IResolver container,
    //         Type contractType, Func<DryIoc.IResolver, object> objectConstructorDelegate,
    //         DryIoc.IReuse reuse = null, DryIoc.FactorySetup setup = null, string named = null)
    //    {
    //        ResolverBuilder.Build(container, contractType, objectConstructorDelegate, reuse, setup, named);
    //    }
    //}

    //public class ResolverBuilder
    //{
        
    //    Func<DryIoc.IResolver,object> _objectConstructorDelegate;


    //    public static void Build(DryIoc.IResolver container, Type contractType, Func<IResolver, object> objectConstructor, IReuse reuse, FactorySetup setup, string named)
    //    {
    //        new ResolverBuilder(container, contractType, objectConstructor, reuse, setup, named);
    //    }

    //    ResolverBuilder(
    //        DryIoc.IResolver resolver, Type contractType, Func<DryIoc.IResolver, object> objectConstructorDelegate,
    //        DryIoc.IReuse reuse = null, DryIoc.FactorySetup setup = null, string named = null
    //        )
    //    {
    //        _objectConstructorDelegate = objectConstructorDelegate;

    //        Delegate lambda;
    //        {
    //            System.Reflection.MethodInfo constructObjectThenCastMethodInfo = Create_ConstructObjectThenCast_MethodInfo_For_ContractType(contractType);
    //            lambda = Create_ConstructObjectThenCast_Lambda_For_ContractType(this, constructObjectThenCastMethodInfo, contractType);
    //        }


    //        // Invoke the original RegisterDelegate<TService>            
    //        {
    //            // public static void RegisterDelegate<TService>(this DryIoc.IRegistrator registrator, Func<DryIoc.IResolver, TService> lambda, DryIoc.IReuse reuse = null, DryIoc.FactorySetup setup = null, string named = null);

    //            // obj is null, means RegisterDelegate is a static method
    //            // resolver comes from _container
    //            // contractType is the TService in the original _container.RegisterDelegate<TService>
    //            System.Reflection.MethodInfo registerDelegateMethodInfo = typeof(DryIoc.Registrator).GetMethod("RegisterDelegate").MakeGenericMethod(contractType);
    //            registerDelegateMethodInfo.Invoke(/*obj*/ null, new object[] { resolver, lambda, reuse, setup, named });
    //        }
    //    }

    //    static System.Reflection.MethodInfo Create_ConstructObjectThenCast_MethodInfo_For_ContractType(Type contractType)
    //    {

    //        System.Reflection.MethodInfo constructObjectThenCastMethodInfo =
    //            // typeof(IResolver) is the resolver parameter of: TService ConstructObjectThenCast<TService>(IResolver resolver)
    //           typeof(ResolverBuilder).GetMethod(
    //               name: "ConstructObjectThenCast",
    //               types: new[] { typeof(IResolver) },
    //               bindingAttr: System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
    //               binder: null,
    //               modifiers: null
    //               )
    //            // contractType is the TService of ConstructObjectThenCast<TService>
    //           .MakeGenericMethod(contractType);
    //        return constructObjectThenCastMethodInfo;
    //    }


    //    // Create a lambda out of this class method: TService ConstructObjectThenCast<TService>(IResolver resolver)
    //    static Delegate Create_ConstructObjectThenCast_Lambda_For_ContractType(ResolverBuilder resolverBuilder,  System.Reflection.MethodInfo constructObjectThenCastMethodInfo, Type contractType)
    //    {
    //        // Create a Func<IResolver,TService> delegate type
    //        // This will be used as the lambda parameter on the original RegisterDelegate static method.                                          
    //        Type lambdaDelegateType = typeof(Func<,>).MakeGenericType(typeof(DryIoc.IResolver), contractType);
    //        // The above corresponds to this example: Func<IResolver, ServiceContracts.IProductService>


    //        // Cannot use Activator.CreateInstance on delegate type as delegates don't have constructor
    //        // ConstructObjectThenCast method has a signature same as lambdaDelegateType 
    //        // Create a lambda out of ConstructObjectThenCast method info
    //        Delegate lambda = Delegate.CreateDelegate(lambdaDelegateType, resolverBuilder, constructObjectThenCastMethodInfo);

    //        return lambda;
    //    }

     
    //    TService ConstructObjectThenCast<TService>(IResolver resolver)
    //    {
    //        var svc = (TService) this._objectConstructorDelegate(resolver);
    //        return svc;
    //    }


    //}// class ResolverBuilder

    //// ...was using DryIoC before


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
            object factory =
                Activator.CreateInstance(typeof(System.ServiceModel.ChannelFactory<>)
                .MakeGenericType(contractType), binding, endpointAddress);

            System.Reflection.MethodInfo createFactory = factory.GetType().GetMethod("CreateChannel", new Type[] { });
            //now dynamic proxy generation using reflection
            return createFactory.Invoke(factory, null);
        }
    }
    
}//namespace





// http://stackoverflow.com/questions/658316/runtime-creation-of-generic-funct


// http://stackoverflow.com/questions/19539871/how-to-assign-a-method-obtained-through-reflection-to-a-delegate-or-how-to-sp