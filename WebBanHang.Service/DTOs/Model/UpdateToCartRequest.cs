using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.Service.DTOs.Model
{
    public class AddCartItemRequest
    {
        public long VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public  class UpdateToCartRequest
    {
        public  List<AddCartItemRequest>? variants { get; set; }
    }
}
