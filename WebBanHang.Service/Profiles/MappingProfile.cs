
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Service.DTOs.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebBanHang.Service.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Tự động map các trường giống tên nhau.


            //Product <-> ProductDto: map CategoryName và BrandName từ navigation properties, ignore các khóa chính và khóa ngoại khi map ngược lại
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.BrandName));
            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.BrandId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            // ProductVariant <-> ProductVariantDto: map SizeLabel, SizeSystem, ColorName, ProductName từ navigation properties, ignore các khóa chính và khóa ngoại khi map ngược lại
            CreateMap<ProductVariant, ProductVariantDto>()
                .ForMember(dest => dest.SizeLabel, opt => opt.MapFrom(src => src.Size.SizeLabel))
                .ForMember(dest => dest.SizeSystem, opt => opt.MapFrom(src => src.Size.SizeSystem))
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.Color.ColorName))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));
            CreateMap<ProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.VariantId, opt => opt.Ignore())
                .ForMember(dest => dest.SizeId, opt => opt.Ignore())
                .ForMember(dest => dest.ColorId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            // Color <-> ColorDto và Size <-> SizeDto: map cơ bản, ignore khóa chính khi map ngược lại
            CreateMap<Color, ColorDto>();
            CreateMap<ColorDto, Color>().ForMember(dest => dest.ColorId, opt => opt.Ignore());
            // Size <-> SizeDto: map cơ bản, ignore khóa chính khi map ngược lại
            CreateMap<Size, SizeDto>();
            CreateMap<SizeDto, Size>().ForMember(dest => dest.SizeId, opt => opt.Ignore());
            // Order <-> OrderDto: map CustomerName từ navigation property User.FullName, tự động map ngược lại
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
                .ReverseMap();

            // Address <-> AddressDto: map cơ bản, tự động map ngược lại
            CreateMap<Address, AddressDto>();
            CreateMap<Address, AddressDto>().ReverseMap();
            // 
            CreateMap<Review, ReviewDto>();
            CreateMap<ReviewDto, Review>()
                .ForMember(dest => dest.ReviewId, opt => opt.Ignore()) // DB generates key
                .ForMember(dest => dest.User, opt => opt.Ignore())     // navigation properties handled by EF
                .ForMember(dest => dest.OrderItem, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            // OrderItem <-> OrderItemDto: map cơ bản, tự động map ngược lại
            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            // CreateMap<Cart, CartDto>();
            //CreateMap<CartItem, CartItemDto>();
            CreateMap<Review, ReviewDto>();
            // Map cơ bản cho các bảng khác
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();
            // CartItem <-> CartItemDto: map ProductId, VariantSku, ProductName, SizeName, ColorName từ navigation properties của ProductVariant, map StockQuantity từ ProductVariant, tự động map ngược lại
            CreateMap<CartItem, CartItemDto>()
                // Lấy ProductId từ bảng ProductVariant
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductVariant.ProductId))
                // Lấy SKU
                .ForMember(dest => dest.VariantSku, opt => opt.MapFrom(src => src.ProductVariant.Sku))
                // Xuyên qua ProductVariant lấy tên Sản phẩm
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductVariant.Product.ProductName))
                // Xuyên qua ProductVariant lấy tên Size (nhớ dùng đúng tên cột của bạn, ví dụ SizeLabel)
                .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.ProductVariant.Size.SizeLabel))
                // Xuyên qua ProductVariant lấy tên Màu
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.ProductVariant.Color.ColorName))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.ProductVariant.StockQuantity));
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems)); // Map danh sách item
        }
    }
}
