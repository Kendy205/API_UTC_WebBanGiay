using AutoMapper;
using WebBanHang.Model;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //
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
            //
            CreateMap<Color, ColorDto>();
            CreateMap<ColorDto, Color>().ForMember(dest => dest.ColorId, opt => opt.Ignore());
            //
            CreateMap<Size, SizeDto>();
            CreateMap<SizeDto, Size>().ForMember(dest => dest.SizeId, opt => opt.Ignore());
            //
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
                .ReverseMap();
            //
            CreateMap<Address, AddressDto>();
            CreateMap<Address, AddressDto>().ReverseMap();
            //
            CreateMap<Review, ReviewDto>();
            CreateMap<ReviewDto, Review>()
                .ForMember(dest => dest.ReviewId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItem, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            //
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductNameSnapshot, opt => opt.MapFrom(src => src.ProductVariant.Product.ProductName))
                .ForMember(dest => dest.SizeLabelSnapshot, opt => opt.MapFrom(src => src.ProductVariant.Size.SizeLabel))
                .ForMember(dest => dest.ColorNameSnapshot, opt => opt.MapFrom(src => src.ProductVariant.Color.ColorName))
                .ForMember(dest => dest.SkuSnapshot, opt => opt.MapFrom(src => src.ProductVariant.Sku))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductVariant.ProductId));

            //
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Payment, PaymentDto>();
            CreateMap<InventoryMovement, InventoryMovementDto>().ReverseMap();
            //
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductVariant.ProductId))
                .ForMember(dest => dest.VariantSku, opt => opt.MapFrom(src => src.ProductVariant.Sku))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductVariant.Product.ProductName))
                .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.ProductVariant.Size.SizeLabel))
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.ProductVariant.Color.ColorName))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.ProductVariant.StockQuantity));

            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));
        }
    }
}
