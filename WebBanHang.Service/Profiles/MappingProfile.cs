
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Service.DTOs.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebBanHang.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Tự động map các trường giống tên nhau.
            // Với các trường khác tên (Làm phẳng dữ liệu), ta cấu hình như sau:
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.BrandName));
            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.BrandId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

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

            CreateMap<Color, ColorDto>();
            CreateMap<ColorDto, Color>().ForMember(dest => dest.ColorId, opt => opt.Ignore());

            CreateMap<Size, SizeDto>();
            CreateMap<SizeDto, Size>().ForMember(dest => dest.SizeId, opt => opt.Ignore());


            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.FullName)); // Hoặc FullName nếu có

            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<Cart, CartDto>();
            CreateMap<CartItem, CartItemDto>();
            CreateMap<Review, ReviewDto>();
            // Map cơ bản cho các bảng khác
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();

            // ── CART & CARTITEM MAPPINGS ──────────────────────────
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.VariantSku, opt => opt.MapFrom(src => src.ProductVariant.Sku))
                .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.ProductVariant.Size.SizeLabel))
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.ProductVariant.Color.ColorName))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductVariant.Product.ProductName))
                .ReverseMap();

            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems))
                .ReverseMap();
        }
    }
}
