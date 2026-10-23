USE [PetsManager];
GO

IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE id = 1)
BEGIN
	SET IDENTITY_INSERT dbo.roles ON;
	INSERT dbo.roles (id, name, description, status, created_at)
	VALUES
		(1, N'Quản trị viên', N'Quản trị hệ thống', N'active', SYSUTCDATETIME()),
		(2, N'staff', N'Nhân viên', N'active', SYSUTCDATETIME()),
		(3, N'member', N'Khách hàng', N'active', SYSUTCDATETIME()),
		(4, N'receptionist', N'Lễ tân', N'active', SYSUTCDATETIME()),
		(5, N'manager', N'Quản lý', N'active', SYSUTCDATETIME()),
		(6, N'nguyenthiY', N'Nhân viên', N'active', SYSUTCDATETIME()),
		(7, N'admin', N'Quản trị hệ thống', N'active', SYSUTCDATETIME());
	SET IDENTITY_INSERT dbo.roles OFF;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE id = 1)
BEGIN
	SET IDENTITY_INSERT dbo.users ON;
	INSERT dbo.users (id, role_id, username, password_hash, full_name, email, phone_number, address, status, created_at)
	VALUES
		(1, 7, N'admin01', N'AQAAAAIAAYagAAAAECDzzZ7N8Ju7QJrBorKIhgWME8195DYD2l9q7+8/jxgfyKqr4Sbz4g5Hl4sU/RvHLQ==', N'Nguyễn Tường Huy', N'an@example.com', N'0900000023', N'TP HCM', N'active', SYSUTCDATETIME()),
		(2, 2, N'staff01', N'demo_hash_02', N'Trần Thị Bình', N'binh@example.com', N'0900000002', N'TP HCM', N'active', SYSUTCDATETIME()),
		(3, 3, N'member01', N'demo_hash_03', N'Lê Văn Cường', N'cuong@example.com', N'0900000003', N'Bình Dương', N'active', SYSUTCDATETIME()),
		(4, 3, N'member02', N'demo_hash_04', N'Phạm Thị Dung', N'dung@example.com', N'0900000004', N'Đồng Nai', N'active', SYSUTCDATETIME()),
		(5, 5, N'manager01', N'demo_hash_05', N'Hoàng Văn Em', N'em@example.com', N'0900000005', N'Long An', N'active', SYSUTCDATETIME()),
		(6, 2, N'staff02', N'demo_hash_06', N'Võ Thị Hoa', N'hoa@example.com', N'0900000006', N'TP HCM', N'active', SYSUTCDATETIME()),
		(7, 4, N'receptionist01', N'demo_hash_07', N'Nguyễn Thị Lan', N'lan@example.com', N'0900000007', N'TP HCM', N'active', SYSUTCDATETIME());
	SET IDENTITY_INSERT dbo.users OFF;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.pets WHERE id = 1)
BEGIN
	SET IDENTITY_INSERT dbo.pets ON;
	INSERT dbo.pets (id, user_id, qr_token, name, species, breed, gender, weight_kg, created_at)
	VALUES
		(1, 1, N'PET-QR-0001', N'Milu', N'Chó', N'Poodle', N'Male', 6.20, SYSUTCDATETIME()),
		(2, 2, N'PET-QR-0002', N'Lucky', N'Mèo', N'Anh lông ngắn', N'Female', 3.80, SYSUTCDATETIME()),
		(3, 3, N'PET-QR-0003', N'Bông', N'Chó', N'Golden Retriever', N'Male', 22.50, SYSUTCDATETIME()),
		(4, 4, N'PET-QR-0004', N'Miu', N'Mèo', N'Munchkin', N'Female', 4.10, SYSUTCDATETIME()),
		(5, 5, N'PET-QR-0005', N'Kem', N'Thỏ', N'Thỏ trắng', N'Female', 2.30, SYSUTCDATETIME());
	SET IDENTITY_INSERT dbo.pets OFF;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.staff WHERE id = 1)
BEGIN
	SET IDENTITY_INSERT dbo.staff ON;
	INSERT dbo.staff (id, user_id, position, hire_date, status, created_at)
	VALUES
		(1, 2, N'Nhân viên tiếp nhận', '2024-01-15', N'active', SYSUTCDATETIME()),
		(2, 6, N'Nhân viên', '2025-02-20', N'active', SYSUTCDATETIME()),
		(3, 7, N'Lễ tân', '2025-05-12', N'active', SYSUTCDATETIME());
	SET IDENTITY_INSERT dbo.staff OFF;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.pet_images WHERE pet_id = 1)
BEGIN
	SET IDENTITY_INSERT dbo.pet_images ON;
	INSERT dbo.pet_images (id, pet_id, image_url, is_avatar, created_at)
	VALUES
		(1, 1, N'/images/pets/pet-01.jpg', 1, SYSUTCDATETIME()),
		(2, 2, N'/images/pets/pet-02.jpg', 1, SYSUTCDATETIME()),
		(3, 3, N'/images/pets/pet-03.jpg', 1, SYSUTCDATETIME()),
		(4, 4, N'/images/pets/pet-04.jpg', 1, SYSUTCDATETIME()),
		(5, 5, N'/images/pets/pet-05.jpg', 1, SYSUTCDATETIME());
	SET IDENTITY_INSERT dbo.pet_images OFF;
END;
GO
