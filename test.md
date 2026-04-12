# Test API Order và Admin Payment

## 1) Chuẩn bị

- Chạy API:
```bash
dotnet run --project WebBanHang/WebBanHang.csproj --launch-profile http
```

- Base URL:
```text
http://localhost:5277
```

- Lấy token user/admin:
```bash
curl -X POST "http://localhost:5277/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"<user_or_admin_email>","password":"<password>"}'
```

- Gán biến:
```bash
USER_TOKEN="<jwt_user>"
ADMIN_TOKEN="<jwt_admin>"
```

## 2) Test My Orders (User)

### 2.1 Lấy danh sách đơn của tôi
```bash
curl -X GET "http://localhost:5277/api/My/Orders" \
  -H "Authorization: Bearer $USER_TOKEN"
```
Kỳ vọng:
- Có token hợp lệ: `200`, `success=true`
- Không token/sai token: `401`

### 2.2 Lấy chi tiết đơn của tôi
```bash
curl -X GET "http://localhost:5277/api/My/Orders/{orderId}" \
  -H "Authorization: Bearer $USER_TOKEN"
```
Kỳ vọng:
- Đơn thuộc user: `200`
- Không tồn tại/không thuộc user: `404`

### 2.3 Checkout
```bash
curl -X POST "http://localhost:5277/api/My/Orders/checkout" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "shippingAddressId": 1,
    "items": [
      { "variantId": 1, "quantity": 1 }
    ]
  }'
```
Kỳ vọng:
- Thành công: HTTP `200`, trong body `statusCode=201`
- `items` rỗng/null: `400`
- `shippingAddressId` không thuộc user: `400`
- Hết hàng: `400`/`409` theo message service

### 2.4 Hủy đơn
```bash
curl -X POST "http://localhost:5277/api/My/Orders/{orderId}/cancel" \
  -H "Authorization: Bearer $USER_TOKEN"
```
Kỳ vọng:
- Thành công: `200`
- Không thấy đơn: `404`
- Đơn đã hủy trước đó: `400`

## 3) Test Admin Orders

### 3.1 Danh sách đơn (lọc + phân trang)
```bash
curl -G "http://localhost:5277/api/Admin/Orders" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  --data-urlencode "page=1" \
  --data-urlencode "pageSize=10" \
  --data-urlencode "status=Pending" \
  --data-urlencode "search=ORD"
```
Kỳ vọng:
- Admin hợp lệ: `200`
- User thường/không token: `401`/`403`

### 3.2 Chi tiết đơn
```bash
curl -X GET "http://localhost:5277/api/Admin/Orders/{orderId}" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```
Kỳ vọng:
- Có đơn: `200`
- Không có đơn: `404`

### 3.3 Cập nhật trạng thái
```bash
curl -X PUT "http://localhost:5277/api/Admin/Orders/{orderId}/status" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "status": "Cancelled" }'
```
Kỳ vọng:
- Thành công: `200`
- Đơn đã `Cancelled` rồi: `400` (không cộng kho lần 2)

### 3.4 Cập nhật đơn
```bash
curl -X PUT "http://localhost:5277/api/Admin/Orders/{orderId}" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "orderStatus": "Confirmed" }'
```
Kỳ vọng:
- Thành công: `200`
- Không thấy đơn: `404`

### 3.5 Xóa đơn
```bash
curl -X DELETE "http://localhost:5277/api/Admin/Orders/{orderId}" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```
Kỳ vọng:
- Thành công: `200`
- Không thấy đơn: `404`

### 3.6 Admin tạo đơn cho user
```bash
curl -X POST "http://localhost:5277/api/Admin/Orders" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 2,
    "shippingAddressId": 3,
    "items": [
      { "variantId": 1, "quantity": 1 }
    ]
  }'
```
Kỳ vọng:
- Thành công: HTTP `200`, body `statusCode=201`
- Thiếu `userId`: `400`
- Địa chỉ không thuộc userId: `400`
- `items` rỗng/null: `400`

## 4) Test Admin Payments

### 4.1 Danh sách thanh toán
```bash
curl -G "http://localhost:5277/api/Admin/Payments" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  --data-urlencode "page=1" \
  --data-urlencode "pageSize=10" \
  --data-urlencode "status=Paid" \
  --data-urlencode "method=COD" \
  --data-urlencode "search=PAY"
```
Kỳ vọng:
- Admin hợp lệ: `200`
- Không token/sai role: `401`/`403`

## 5) Checklist nhanh sau mỗi lần sửa

- `My Orders` route trả dữ liệu đúng user đang đăng nhập.
- Checkout chỉ nhận địa chỉ thuộc user.
- Admin cancel không cộng tồn kho lặp cho đơn đã hủy.
- Admin payment trả về được danh sách, lọc status/method/search không lỗi.
- Mọi API đều trả `ApiResponse` có `success`, `statusCode`, `message`, `data`.
