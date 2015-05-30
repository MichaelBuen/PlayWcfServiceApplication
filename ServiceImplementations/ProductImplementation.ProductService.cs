using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceImplementations
{
    public static partial class ProductImplementation
    {
        [System.ServiceModel.ServiceBehavior(InstanceContextMode = System.ServiceModel.InstanceContextMode.PerCall)]
        public class ProductService : ServiceContracts.ProductContract.IProductService
        {
            int ServiceContracts.ProductContract.IProductService.Multiply(int p1, int p2)
            {
                return p1 * p2;
            }
        }
    }

}
