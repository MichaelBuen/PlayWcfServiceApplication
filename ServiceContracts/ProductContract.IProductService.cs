using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
	public static class ProductContract
    {
        [ServiceContract]
        public interface IProductService
        {
            [OperationContract]
            int Multiply(int p1, int p2);
        }
    }

    
}
