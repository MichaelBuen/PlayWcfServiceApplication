using System;

using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Activation;



namespace AppService
{
    public static class DependencyFactory
    {
        public static LightInject.IServiceContainer Container { get; set; }
    }

    public class LightInjectInstanceProvider : IInstanceProvider
    {
        readonly Type _serviceType;

        public LightInjectInstanceProvider(Type serviceType)
        {
            _serviceType = serviceType;

        }

        object IInstanceProvider.GetInstance(InstanceContext instanceContext)
        {
            return Resolve();
        }

        object IInstanceProvider.GetInstance(InstanceContext instanceContext, Message message)
        {
            return Resolve();
        }


        object Resolve()
        {
            var instance = DependencyFactory.Container.GetInstance(_serviceType);
            return instance;
        }


        void IInstanceProvider.ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }

    public class LightInjectServiceBehavior : IServiceBehavior
    {
        void IServiceBehavior.AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers)
            {
                var cd = cdb as ChannelDispatcher;
                if (cd != null)
                {
                    foreach (EndpointDispatcher ed in cd.Endpoints)
                    {
                        ed.DispatchRuntime.InstanceProvider = new LightInjectInstanceProvider(serviceDescription.ServiceType);
                    }
                }
            }
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }

    public class LightInjectServiceHost : ServiceHost
    {

        public LightInjectServiceHost(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
        }

        protected override void OnOpening()
        {
            Description.Behaviors.Add(new LightInjectServiceBehavior());
            base.OnOpening();
        }

    }


    public class LightInjectServiceHostFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new LightInjectServiceHost(serviceType, baseAddresses);
        }
    }

}