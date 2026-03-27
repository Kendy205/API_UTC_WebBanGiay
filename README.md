Markdown
# 🛒 WebBanHang - Hệ Thống Thương Mại Điện Tử (E-Commerce)

Chào mừng bạn đến với dự án **WebBanHang**! Đây là một hệ thống bán lẻ trực tuyến được xây dựng bằng **ASP.NET Core** và **Entity Framework Core**. 

Dự án này được thiết kế với kiến trúc cơ sở dữ liệu chuẩn mực, hỗ trợ bán hàng đa biến thể (Size/Màu sắc), quản lý tồn kho chặt chẽ và theo dõi đơn hàng chi tiết. Nếu bạn là người mới, đừng lo lắng! Hướng dẫn dưới đây sẽ giúp bạn cài đặt và chạy dự án trên máy của mình chỉ trong vài phút.

---

## 🛠️ Yêu cầu hệ thống (Prerequisites)

Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt các phần mềm sau:
* **[Git](https://git-scm.com/)**: Để tải source code về máy.
* **[.NET SDK 8.0](https://dotnet.microsoft.com/download)** (hoặc phiên bản tương ứng bạn đang dùng): Môi trường để chạy code C#.
* **[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)** (Bản Express hoặc Developer đều được): Để lưu trữ cơ sở dữ liệu.
* **[Visual Studio 2022](https://visualstudio.microsoft.com/)** hoặc **[Visual Studio Code](https://code.visualstudio.com/)**.

---

## 🚀 Hướng dẫn cài đặt và chạy dự án (Getting Started)

### Bước 1: Clone dự án về máy
Mở Terminal (hoặc Command Prompt / Git Bash) và chạy các lệnh sau:


git clone [https://github.com/Kendy205/API_UTC_WebBanGiay.git](https://github.com/TEN_CUA_BAN/WebBanHang.git)
cd WebBanHang

##
Bước 2: Cấu hình chuỗi kết nối Database (Connection String)
Mở file appsettings.json (hoặc appsettings.Development.json) nằm trong thư mục project chính. Tìm đến mục ConnectionStrings và sửa lại thông tin Server cho khớp với SQL Server trên máy bạn:

JSON
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=WebBanHangDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
##
Bước 3: Khởi tạo Cơ sở dữ liệu (Database Migration)
Mở Terminal tại thư mục chứa project (nơi có file .csproj) và chạy lệnh sau để Entity Framework tự động tạo bảng trong SQL Server:

Bash
dotnet ef database update
(Lưu ý: Nếu máy bạn báo lỗi không tìm thấy lệnh dotnet ef, hãy chạy lệnh dotnet tool install --global dotnet-ef trước).
##
Bước 4: Chạy ứng dụng
Bạn có thể ấn nút Play (F5) trực tiếp trong Visual Studio, hoặc dùng lệnh sau trong Terminal:

Bash
dotnet run
Mở trình duyệt và truy cập vào đường dẫn được hiển thị trên Terminal (thường là https://localhost:5001 hoặc http://localhost:5000). Dự án đã chạy thành công! 🎉
##
🗄️ Tổng quan Cơ sở dữ liệu (Database Overview)
Hệ thống được chia thành 5 phân hệ (modules) chính để dễ dàng quản lý và mở rộng:

🧑‍💻 Phân hệ Người dùng & Phân quyền: Quản lý User, Role và địa chỉ giao hàng (Addresses).

📦 Phân hệ Sản phẩm (Catalog): Quản lý Danh mục (Category), Thương hiệu (Brand), Sản phẩm (Product) và Biến thể sản phẩm (Product Variants - kết hợp Size & Color).

🏭 Phân hệ Kho hàng (Inventory): Lưu vết mọi lịch sử nhập/xuất kho qua bảng inventory_movements, giúp kiểm toán dễ dàng mà không sợ thất thoát.

🛒 Phân hệ Bán hàng: Xử lý logic Giỏ hàng (Carts) và chốt Đơn hàng (Orders). Thông tin sản phẩm lúc mua được lưu dưới dạng Snapshot để chống sai lệch dữ liệu về sau.

⭐ Phân hệ Đánh giá (Reviews): Liên kết chuẩn xác 1-1 với chi tiết đơn hàng (OrderItem) để đảm bảo chỉ những người đã mua hàng mới được đánh giá.

Sơ đồ Thực thể Liên kết (ERD)
Dưới đây là sơ đồ cấu trúc các bảng trong hệ thống:
https://drive.google.com/file/d/1NMQ6dNO9XQH6nyENWZ9Y3YJKGph0KfYa/view?usp=sharing
🤝 Đóng góp (Contributing)
Nếu bạn tìm thấy lỗi (bug) hoặc có ý tưởng cải thiện dự án, hãy tạo một Pull Request hoặc mở Issue. Mọi đóng góp của bạn đều được chào đón!
