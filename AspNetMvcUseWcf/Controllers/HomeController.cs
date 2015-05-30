using System;
using System.Web.Mvc;

namespace AspNetMvcUseWcf.Controllers
{
    public class HomeController : Controller
    {
        ServiceContracts.ProductContract.IProductService _productService;
        ServiceContracts.PersonContract.IMemberService _memberService;

        ServiceContracts.ISampleService _sampleService;

        public HomeController(
            ServiceContracts.ProductContract.IProductService productService, 
            ServiceContracts.PersonContract.IMemberService memberService, 
            ServiceContracts.ISampleService sampleService,
            string message)
        {
            _productService = productService;
            _memberService = memberService;
            _sampleService = sampleService;
        }

        public string Index()
        {
            return _productService.Multiply(new Random().Next(10), new Random().Next(10)).ToString()
                + " - " + _memberService.GetMembership() + " : " + _sampleService.SampleMessage();
        }


        // /Home/XIndex
        public string XIndex()
        {
            
            // should be singleton
            var binding = new System.ServiceModel.BasicHttpBinding();

            // should be singleton
            var address = new System.ServiceModel.EndpointAddress("http://localhost:50549/ProductImplementation.ProductService.svc");

            //// When services can only be determined at runtime
            //Type contractType = typeof(ServiceContracts.ProductContract.IProductService);
            //var svc = (ServiceContracts.ProductContract.IProductService)ServiceModelHelper.CreateService(contractType, binding, address);
            //return svc.Multiply(new Random().Next(10), new Random().Next(10)).ToString();

            // When things can be determined at compile-time
            var factory = new System.ServiceModel.ChannelFactory<ServiceContracts.ProductContract.IProductService>(binding, address);
            ServiceContracts.ProductContract.IProductService service = factory.CreateChannel();

            int n = service.Multiply(7, 6) + 1000000;
            return n.ToString();
            
        }

    }//HomeController


}//namespace
