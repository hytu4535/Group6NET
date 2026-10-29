# Group6NET — Hệ thống quản lý và nhận diện thú cưng

Đồ án kết hợp ứng dụng quản lý thú cưng trên nền tảng ASP.NET Core MVC với dịch vụ nhận diện cá thể bằng vector embedding, ONNX Runtime và FAISS.

## Tổng quan và cấu trúc thư mục

| Thư mục / tệp | Vai trò |
|---|---|
| `PetManagementSystem/` | Ứng dụng Web ASP.NET Core MVC, EF Core, đăng nhập/phân quyền và các chức năng quản lý thú cưng, dịch vụ, lịch sử nhận diện. |
| `AI-Service/` | Dịch vụ FastAPI nhận ảnh, trích xuất embedding bằng ONNX Runtime CPU và tìm kiếm ứng viên bằng FAISS-CPU. Dockerfile nằm trong thư mục này. |
| `docker-compose.yml` | Chạy riêng AI Service và mount model, FAISS index, mapping cùng gallery từ máy host. |
| `PetManagementSystem.slnx` | Solution .NET. |

ASP.NET Core và SQL Server chạy trên máy host; Docker Compose chỉ khởi chạy AI Service. ASP.NET gọi AI qua `http://127.0.0.1:8000` khi chạy trên cùng máy.

## Yêu cầu môi trường

- Git.
- **.NET 10 SDK**. Project hiện target `net10.0` và sử dụng EF Core 10; .NET 8 SDK không thể build project hiện tại. Cài SDK từ [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).
- SQL Server (SQL Server Express/Developer hoặc LocalDB) và công cụ quản trị như SSMS.
- Docker Desktop với backend WSL 2.
- Nếu cần tải model hoặc tự tạo FAISS index: Python 3.11 và kết nối Internet.

Clone repository kèm AI Service:

```powershell
git clone --recurse-submodules https://github.com/hytu4535/Group6NET.git
cd Group6NET
```

Nếu đã clone trước đó mà chưa lấy submodule:

```powershell
git submodule update --init --recursive
```

> Bản model ONNX, index FAISS, mapping và ảnh gallery là tài nguyên dữ liệu lớn, không nên đóng gói vào image. Sau khi clone, cần chuẩn bị các tệp bên dưới nếu chúng chưa có trong thư mục làm việc.

## Bước 1: Khởi chạy AI Service bằng Docker

Kiểm tra các tệp đầu vào:

```text
AI-Service/
├── models/
│   └── pet-recognition-base.onnx
├── vector_store/
│   ├── pets.faiss
│   └── mapping.json
└── gallery/
    └── PET.../             # ảnh hồ sơ dùng để dựng/cập nhật index
```

Nếu thiếu model, có thể cài dependency runtime rồi chạy script tải/kiểm tra model:

```powershell
cd AI-Service
python -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install -r requirements.txt
python scripts/test_pet_recognition.py
```

Nếu chưa có FAISS index và mapping, chuẩn bị ảnh trong các thư mục con `gallery/<PetId>/`, sau đó chạy:

```powershell
python scripts/build_gallery.py
cd ..
```

Từ thư mục gốc solution, build và chạy AI Service:

```powershell
docker compose up -d --build
```

Kiểm tra trạng thái và endpoint health:

```powershell
docker compose ps
curl.exe http://localhost:8000/health
```

Health response cần có `"status":"ok"` và `"ready":true`. Nếu container không healthy, xem log bằng `docker compose logs -f pet-ai-service`; thường cần kiểm tra lại model, index và `mapping.json`.

## Bước 2: Cấu hình SQL Server và cập nhật database

Sửa `ConnectionStrings:DefaultConnection` trong `PetManagementSystem/appsettings.json` để trỏ tới SQL Server đang chạy trên máy host. Ví dụ với SQL Server Express:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PetsManager;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30;"
}
```

Với LocalDB, có thể dùng `Server=(localdb)\\MSSQLLocalDB;Database=PetsManager;Trusted_Connection=True;TrustServerCertificate=True;`.

Từ thư mục gốc solution, cập nhật database theo EF Core migrations:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet ef database update --project PetManagementSystem
```

Nếu `dotnet-ef` đã cài, bỏ qua lệnh cài đặt. Solution có migration `InitialCreate`.

`PetManagementSystem/Database/PetsManager.sql` là bản dump SQL mẫu. **Không chạy mù quáng trên database đang có dữ liệu**: kiểm tra nội dung trước khi import vì script dump có các thao tác xóa/tạo lại database. Ưu tiên EF Core migrations cho cài đặt mới.

## Bước 3: Chạy ứng dụng Web ASP.NET Core

Đảm bảo AI Service đang healthy và SQL Server đang chạy. Từ thư mục gốc:

```powershell
dotnet run --project PetManagementSystem
```

Hoặc mở `PetManagementSystem.slnx` trong Visual Studio và nhấn **F5**. Project hiện cấu hình launch URLs:

- HTTPS: `https://localhost:7172`
- HTTP: `http://localhost:5269`

Nếu trình duyệt cảnh báo chứng chỉ HTTPS khi chạy lần đầu, tin cậy chứng chỉ phát triển bằng:

```powershell
dotnet dev-certs https --trust
```

Địa chỉ AI mặc định trong `appsettings.json` là `http://127.0.0.1:8000`, phù hợp khi ASP.NET chạy trực tiếp trên host và Docker publish cổng `8000`.

## Lệnh Docker hữu ích

Chạy lại và build image:

```powershell
docker compose up -d --build
```

Theo dõi log:

```powershell
docker compose logs -f
```

Kiểm tra trạng thái:

```powershell
docker compose ps
```

Dừng và gỡ container/network:

```powershell
docker compose down
```

Các thư mục `AI-Service/models`, `AI-Service/vector_store` và `AI-Service/gallery` được bind-mount vào container ở chế độ chỉ đọc. Vì vậy, container restart không làm mất model/index; thay index trên máy host sẽ được phản ánh qua mount.
