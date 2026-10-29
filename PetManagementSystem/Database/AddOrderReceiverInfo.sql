-- Thêm thông tin người nhận (họ tên, email) cho bảng orders phục vụ trang thanh toán.
-- Script an toàn khi chạy nhiều lần. Ứng dụng cũng tự chạy đoạn này khi khởi động (xem Program.cs).
IF COL_LENGTH('dbo.orders', 'receiver_name') IS NULL
    ALTER TABLE dbo.orders ADD receiver_name NVARCHAR(150) NULL;
GO
IF COL_LENGTH('dbo.orders', 'receiver_email') IS NULL
    ALTER TABLE dbo.orders ADD receiver_email NVARCHAR(150) NULL;
GO
