using System;
using System.Linq;
using UnitTestFriendlyDal;


namespace AppService
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            DependencyFactory.Container = new LightInject.ServiceContainer();


            RegisterIocs();
        }

        static void RegisterIocs()
        {

            //// Instead of manually adding each implementation:
            //DependencyFactory.Container.Register<ServiceImplementations.ProductImplementation.ProductService>(new LightInject.PerRequestLifeTime());
            //DependencyFactory.Container.Register<ServiceImplementations.PersonImplementation.MemberService>(new LightInject.PerRequestLifeTime());

            // Just do this:
            var serviceImplementations = typeof(ServiceImplementations.SampleService).Assembly.GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(i => i.GetCustomAttributes(false)
                        .Any(a => a.GetType() == typeof(System.ServiceModel.ServiceContractAttribute))));

            foreach (var item in serviceImplementations)
            {
                DependencyFactory.Container.Register(item, new LightInject.PerRequestLifeTime());
            }


            DependencyFactory.Container.Register<IDomainAccessFactory, DomainAccessFactory>(new LightInject.PerContainerLifetime());
            DependencyFactory.Container.RegisterInstance<NHibernate.ISessionFactory>(RichDomainModelsMapping.Mapper.SessionFactory);
        }
    }
}