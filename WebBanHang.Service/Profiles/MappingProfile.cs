using AutoMapper;
using WebBanHang.Model;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.DTOs.Payment;

namespace WebBanHang.Service.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, AdminOrderListItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "N/A"))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderItems != null ? src.OrderItems.Count : 0))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src =>
                    src.Payments != null && src.Payments.Any()
                    ? src.Payments.OrderByDescending(p => p.CreatedAt).First().PaymentMethod
                    : "N/A"))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.OrderStatus));

            // 2. Mapping cho chi tiết mặt hàng trong đơn hàng (AdminOrderDetailItemDto)
            CreateMap<OrderItem, AdminOrderDetailItemDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.VariantId))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductNameSnapshot))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));

            // 3. Mapping cho chi tiết đơn hàng (AdminOrderDetailDto)
            CreateMap<Order, AdminOrderDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "N/A"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.OrderStatus))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src =>
                    src.Payments != null && src.Payments.Any()
                    ? src.Payments.OrderByDescending(p => p.CreatedAt).First().PaymentMethod
                    : "N/A"))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src =>
                    src.ShippingAddress != null
                    ? $"{src.ShippingAddress.StreetAddress}, {src.ShippingAddress.Ward}, {src.ShippingAddress.District}, {src.ShippingAddress.Province}"
                    : "N/A"));
            // Trong MappingProfile.cs

            CreateMap<Payment, AdminPaymentListItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.TransactionCode) ? src.TransactionCode.Trim() : $"PAY-{src.PaymentId}"))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Order != null && src.Order.User != null ? src.Order.User.FullName : "N/A"))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                // Logic Normalize Method đưa vào đây
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src =>
                    src.PaymentMethod != null ? (src.PaymentMethod.ToLower() == "vnpay" || src.PaymentMethod.ToLower() == "banking" ? "Banking" : "COD") : "N/A"))
                // Logic Normalize Status đưa vào đây
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.PaymentStatus != null && (src.PaymentStatus.ToLower() == "paid" || src.PaymentStatus.ToLower() == "success") ? "Paid" : "Pending"));

            //
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.BrandName));

            CreateMap<ProductDto, Product>()
                //.ForMember(dest => dest.ProductId, opt => opt.Ignore())
                //.ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                //.ForMember(dest => dest.BrandId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ProductVariant, ProductVariantDto>()
                .ForMember(dest => dest.SizeLabel, opt => opt.MapFrom(src => src.Size.SizeLabel))
                .ForMember(dest => dest.SizeSystem, opt => opt.MapFrom(src => src.Size.SizeSystem))
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.Color.ColorName))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));

            CreateMap<ProductVariantDto, ProductVariant>()
                //.ForMember(dest => dest.VariantId, opt => opt.Ignore())
                //.ForMember(dest => dest.SizeId, opt => opt.Ignore())
                //.ForMember(dest => dest.ColorId, opt => opt.Ignore())
                //.ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            //
            CreateMap<Color, ColorDto>();
            CreateMap<ColorDto, Color>().ForMember(dest => dest.ColorId, opt => opt.Ignore());
            //
            CreateMap<Size, SizeDto>();
            CreateMap<SizeDto, Size>().ForMember(dest => dest.SizeId, opt => opt.Ignore());
            //
            CreateMap<Order, OrderDto>()
               .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "N/A"))
               .ForMember(dest => dest.SubtotalAmount, opt => opt.MapFrom(src => src.SubtotalAmount)) // Đúng tên trong OrderDto
               .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))       // Đúng tên trong OrderDto
               .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))         // Khớp với List<OrderItemDto>
               //.ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.))         // Khớp với List<OrderItemDto>
               .ReverseMap();
            //
            CreateMap<Address, AddressDto>();
            CreateMap<Address, AddressDto>().ReverseMap();
            //
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));
                

            CreateMap<ReviewDto, Review>()
                //.ForMember(dest => dest.ReviewId, opt => opt.Ignore())
                //.ForMember(dest => dest.User, opt => opt.Ignore())
                //.ForMember(dest => dest.OrderItem, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            //
            CreateMap<OrderItem, OrderItemDto>()
                 .ForMember(dest => dest.OrderItemId, opt => opt.MapFrom(src => src.OrderItemId))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.VariantId))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductVariant != null ? src.ProductVariant.Product.ProductId : 0))
                // Map các trường Snapshot từ Entity sang DTO
                .ForMember(dest => dest.ProductNameSnapshot, opt => opt.MapFrom(src => src.ProductNameSnapshot))
                .ForMember(dest => dest.SizeLabelSnapshot, opt => opt.MapFrom(src => src.SizeLabelSnapshot))
                .ForMember(dest => dest.ColorNameSnapshot, opt => opt.MapFrom(src => src.ColorNameSnapshot))
                .ForMember(dest => dest.SkuSnapshot, opt => opt.MapFrom(src => src.SkuSnapshot))
                .ForMember(dest => dest.imageUrlSnapshot, opt => opt.MapFrom(src => src.ProductVariant.Product.Image))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.LineTotal))
                .ReverseMap();

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
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.ProductVariant.Product.Image))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.ProductVariant.StockQuantity));

            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));
            //User & Role
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleNames, opt => opt.MapFrom(src => string.Join(", ", src.UserRoles.Select(ur => ur.Role.RoleName))))
                .ReverseMap();
            CreateMap<UserRole, UserRoleDto>().ReverseMap();
            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<UserResgiterDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
