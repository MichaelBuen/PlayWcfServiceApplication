using System.ServiceModel;

namespace ServiceContracts
{
    [ServiceContract]
    public interface ISampleService
    {
        [OperationContract]
        string SampleMessage();
    }
}
