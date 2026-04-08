using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Service.IServices;

namespace WebBanHang.Controllers.AddressController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController(IAddressService addressService) : ControllerBase
    {
    }
}
