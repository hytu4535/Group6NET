hãy sửa trực tiếp vào file như sau:
- chức năng "Người dùng", URL: http://localhost:5269/Identity/Users. Ở cột thao tác, ở mỗi hàng: có 4 nút (ký hiệu) gồm: "xem", "sửa", "xóa", "khóa" người dùng tương ứng với người dùng đang ở hàng được thao tác. Ở nút "xem", hiển thị form chi tiết thông tin người dùng. Ở nút "khóa", chuyển trạng thái người dùng thành "inactive" nhưng vẫn hiển thị người dùng trong danh sách. Ở nút "xóa" (soft delete), ẩn khỏi danh sách nhưng dữ liệu vẫn còn trong DB, hiển thị cảnh báo "Bạn có chắc chắn muốn xóa người dùng này khỏi danh sách?" trước khi xóa
- chức năng "vai trò" cũng làm 4 nút tương tự như "người dùng", URL: http://localhost:5269/Identity/Roles. Không cho phép khóa/xóa vai trò đang được gán cho người dùng. Nếu vai trò đang có người sử dụng thì phải gỡ liên kết với người dùng trước rồi mới khóa/xóa.
- chức năng "phân quyền" cũng làm 4 nút tương tự như 2 trang trên, URL: http://localhost:5269/Identity/Permissions. không cho "xóa","khóa" nếu đang được sử dụng
- chức năng "nhân viên" và "bác sĩ thú y", URL: http://localhost:5269/Identity/Staff, http://localhost:5269/Identity/Veterinarians cũng làm 4 nút như những trang trước đó.
- Thống nhất giao diện 100% cho các chức năng đã làm
- Đường dẫn sau: D:\SGU Nam 4 HK1\DoAnNET\PetManagementSystem\wwwroot\dist. Đây là đường dẫn thư mục AdminLTE, hãy sử dụng AdminLTEcd "D:\SGU Nam 4 HK1\DoAnNET"
dotnet restore .\PetManagementSystem\PetManagementSystem.csproj
dotnet run --project .\PetManagementSystem\PetManagementSystem.csproj --launch-profile http
 để làm 100% trang dashboard (http://localhost:5269/Admin/Dashboard)