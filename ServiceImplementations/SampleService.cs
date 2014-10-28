using System.ServiceModel;

namespace ServiceImplementations
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class SampleService : ServiceContracts.ISampleService
    {
        string ServiceContracts.ISampleService.GetGreet()
        {
            return "Hey " + System.Guid.NewGuid();
        }
    }
}
