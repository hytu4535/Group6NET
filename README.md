# Group6NET — Hệ thống quản lý và nhận diện thú cưng

## Kiến trúc chạy local

- **AI-Service:** chạy bằng Docker Desktop vì sử dụng Python, ONNX Runtime và FAISS.
- **Backend .NET:** chạy trực tiếp trên máy bằng Visual Studio hoặc .NET CLI.
- **Cơ sở dữ liệu:** chạy trực tiếp trên máy nhóm (SQL Server/LocalDB).

## Yêu cầu

- Git & **Git LFS** (để tải file model ONNX và FAISS index dung lượng lớn).
- Docker Desktop — chỉ dùng để chạy AI-Service.
- .NET SDK 10 (hoặc Visual Studio có workload ASP.NET Core).
- SQL Server/LocalDB theo cấu hình của nhóm.

---

## 1. Khởi động AI-Service bằng Docker

### Bước 1: Tải model và dữ liệu vector (Bắt buộc khi mới clone/pull code)
Mở terminal tại thư mục gốc `Group6NET` và kéo các file nhị phân lớn từ Git LFS về máy thật:

```powershell
git lfs pull

Mở Docker Desktop, sau đó mở terminal tại thư mục gốc `Group6NET` và chạy:

```powershell
docker compose up --build -d
```

Service AI có tên `pet-ai-service`. Kiểm tra API đã hoạt động:

- Swagger: <http://localhost:8000/docs>
- Health check: <http://localhost:8000/health>

AI-Service phải được khởi động trước khi chạy Backend .NET.

## 2. Chạy Backend .NET trên máy thật

Đảm bảo cơ sở dữ liệu đang chạy và cấu hình URL AI là:

```json
"AIService": {
  "BaseUrl": "http://localhost:8000"
}
```

Đặt cấu hình này trong `PetManagementSystem/appsettings.Development.json` nếu muốn dùng riêng cho môi trường Development. Hiện `appsettings.json` đang dùng `http://127.0.0.1:8000`, cũng trỏ tới cùng service local.

Từ thư mục gốc `Group6NET`, chạy:

```powershell
dotnet run --project PetManagementSystem
```

Hoặc mở solution trong Visual Studio và nhấn **F5**.

## Lệnh Docker hữu ích

Xem log AI-Service:

```powershell
docker compose logs -f
```

Tắt AI-Service khi nghỉ:

```powershell
docker compose down
```
