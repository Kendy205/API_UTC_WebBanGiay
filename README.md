
    %% 1. QUẢN LÝ NGƯỜI DÙNG
    User ||--o{ UserRole : has
    Role ||--o{ UserRole : has
    User ||--o{ Address : saves
    
    %% 2. DANH MỤC & SẢN PHẨM
    Category ||--o{ Category : "parent/child"
    Category ||--o{ Product : contains
    Brand ||--o{ Product : brands
    Product ||--o{ ProductVariant : has_variants
    Size ||--o{ ProductVariant : sizes
    Color ||--o{ ProductVariant : colors

    %% 3. KHO & GIỎ HÀNG
    ProductVariant ||--o{ InventoryMovement : logs_stock
    User ||--o{ InventoryMovement : "created by (admin)"
    
    User ||--o{ Cart : owns
    Cart ||--o{ CartItem : contains
    ProductVariant ||--o{ CartItem : added_as

    %% 4. ĐƠN HÀNG & THANH TOÁN
    User ||--o{ Order : places
    Address ||--o{ Order : shipped_to
    Order ||--o{ Payment : paid_via
    Order ||--o{ OrderItem : contains
    ProductVariant ||--o{ OrderItem : ordered_as

    %% 5. ĐÁNH GIÁ (REVIEW)
    OrderItem ||--o| Review : "1-1 (Purchased = Reviewed)"
    User ||--o{ Review : writes

Giải thích 5 Phân hệ (Modules) cốt lõi của hệ thống:
1. Module Người dùng & Phân quyền (Users & Roles)
Các bảng: User, Role, UserRole, Address.

Cách hoạt động: Dùng cơ chế phân quyền N-N (Nhiều-Nhiều) qua bảng trung gian UserRole. Một người dùng có thể là Admin, Customer hoặc Manager. Bảng Address lưu danh sách địa chỉ nhận hàng của khách, có cờ IsDefault để tự động điền khi thanh toán.

2. Module Sản phẩm & Biến thể (Catalog & Variants)
Các bảng: Category, Brand, Product, Size, Color, ProductVariant.

Cách hoạt động: Thiết kế này rất xịn! Thay vì nhồi nhét mọi thứ vào bảng Product, bạn tách ra.

Product chỉ lưu thông tin chung (Tên, Mô tả, Slug SEO).

ProductVariant (Biến thể) là "ngôi sao" thực sự. Mỗi dòng kết hợp 1 Product + 1 Size + 1 Color tạo thành 1 mã SKU duy nhất để bán. Ví dụ: Áo thun (Product) + Màu Đen (Color) + Size L (Size) = Mã SKU: AO-DEN-L.

3. Module Quản lý Kho (Inventory)
Các bảng: InventoryMovement.

Cách hoạt động: Đây là tư duy của hệ thống lớn. Tồn kho (StockQuantity) trong ProductVariant thay đổi không phải bằng cách sửa trực tiếp, mà được log lại qua bảng InventoryMovement. Có người nhập hàng (IN), xuất hàng (OUT), mọi thao tác đều ghi lại ai làm, lý do gì, số lượng bao nhiêu. Rất dễ kiểm toán (Audit)!

4. Module Mua sắm (Cart & Checkout)
Các bảng: Cart, CartItem, Order, OrderItem, Payment.

Cách hoạt động: * Khách chọn Variant -> Bỏ vào CartItem.

Lúc thanh toán -> Chốt thành Order và Payment.

Điểm sáng: Bảng OrderItem lưu lại "Snapshot" (Ảnh chụp lúc mua) của tên sản phẩm, size, màu, giá. Nếu 1 năm sau shop đổi tên sản phẩm đó, hóa đơn cũ của khách vẫn hiển thị đúng tên cũ!

5. Module Tương tác (Reviews)
Các bảng: Review.

Cách hoạt động: Trực tiếp liên kết 1-1 với OrderItem. Điều này khóa chặt nghiệp vụ: "Khách hàng phải mua món đồ đó (có OrderItem) thì mới được phép đánh giá, và mỗi món đồ trong đơn chỉ được đánh giá 1 lần". Đây là cơ chế "Verified Purchase" (Người mua thực sự) giống Shopee hay Amazon.
