/* 1. Database and prerequisite tables from PetsManager.sql */
USE [master];
GO

IF DB_ID(N'PetsManager') IS NOT NULL
BEGIN
	ALTER DATABASE [PetsManager] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	DROP DATABASE [PetsManager];
END;
GO

IF DB_ID(N'PetsManager') IS NULL
BEGIN
	CREATE DATABASE [PetsManager];
END;
GO
USE [PetsManager];
GO

DROP TABLE IF EXISTS pet_images;
DROP TABLE IF EXISTS breed_prediction_details;
DROP TABLE IF EXISTS breed_predictions;
DROP TABLE IF EXISTS chat_conversations;
DROP TABLE IF EXISTS feedbacks;
DROP TABLE IF EXISTS notifications;
DROP TABLE IF EXISTS pet_packages;
DROP TABLE IF EXISTS medical_prescriptions;
DROP TABLE IF EXISTS vaccination_records;
DROP TABLE IF EXISTS pet_health_records;
DROP TABLE IF EXISTS appointments_services;
DROP TABLE IF EXISTS appointments;
DROP TABLE IF EXISTS pets;
DROP TABLE IF EXISTS invoices;
DROP TABLE IF EXISTS payments;
DROP TABLE IF EXISTS order_items;
DROP TABLE IF EXISTS orders;
DROP TABLE IF EXISTS cart_items;
DROP TABLE IF EXISTS carts;
DROP TABLE IF EXISTS import_details;
DROP TABLE IF EXISTS import_receipts;
DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS suppliers;
DROP TABLE IF EXISTS categories;
DROP TABLE IF EXISTS veterinarians;
DROP TABLE IF EXISTS staff;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS role_permissions;
DROP TABLE IF EXISTS permissions;
DROP TABLE IF EXISTS service_packages;
DROP TABLE IF EXISTS services;
DROP TABLE IF EXISTS roles;
GO

/* Vai trò người dùng */
CREATE TABLE roles (
	id INT IDENTITY(1,1) PRIMARY KEY,
	name NVARCHAR(40) NOT NULL,
	description NVARCHAR(255) NULL,
	status NVARCHAR(20) NOT NULL DEFAULT N'active',
	created_at DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
	updated_at DATETIME2(7) NULL
);
GO

/* Danh sách quyền hệ thống */
CREATE TABLE permissions (
	id INT IDENTITY(1,1) PRIMARY KEY,
	code NVARCHAR(60) NOT NULL UNIQUE,
	name NVARCHAR(40) NOT NULL,
	description NVARCHAR(255) NULL,
	module NVARCHAR(40) NOT NULL,
	is_active BIT NOT NULL DEFAULT 1,
	created_at DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
	updated_at DATETIME2(7) NULL
);
GO

/* Phân quyền theo vai trò */
CREATE TABLE role_permissions (
	role_id INT NOT NULL,
	permission_id INT NOT NULL,
	created_at DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
	CONSTRAINT PK_role_permissions PRIMARY KEY (role_id, permission_id),
	CONSTRAINT FK_role_permissions_roles FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
	CONSTRAINT FK_role_permissions_permissions FOREIGN KEY (permission_id) REFERENCES permissions(id) ON DELETE CASCADE
);
GO

/* Tài khoản người dùng */
CREATE TABLE users (
	id INT IDENTITY(1,1) PRIMARY KEY,
	role_id INT NOT NULL,
	username NVARCHAR(40) NOT NULL UNIQUE,
	password_hash NVARCHAR(255) NOT NULL,
	full_name NVARCHAR(40) NULL,
	email NVARCHAR(100) NOT NULL UNIQUE,
	phone_number NVARCHAR(40) NULL,
	address NVARCHAR(255) NULL,
	status NVARCHAR(20) NOT NULL DEFAULT N'active',
	created_at DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
	updated_at DATETIME2(7) NULL,
	CONSTRAINT FK_users_roles FOREIGN KEY (role_id) REFERENCES roles(id)
);
GO

/* Nhân viên phòng thú */
CREATE TABLE staff (
	id INT IDENTITY(1,1) PRIMARY KEY,
	user_id INT NOT NULL UNIQUE,
	position NVARCHAR(60) NULL,
	hire_date DATE NULL,
	status NVARCHAR(20) NOT NULL DEFAULT N'active',
	created_at DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
	updated_at DATETIME2(7) NULL,
	CONSTRAINT FK_staff_users FOREIGN KEY (user_id) REFERENCES users(id)
);
GO

/* Bác sĩ thú y */
CREATE TABLE veterinarians (
	id INT IDENTITY(1,1) PRIMARY KEY,
	user_id INT NOT NULL UNIQUE,
	specialty NVARCHAR(100) NULL,
	license_no NVARCHAR(50) NULL UNIQUE,
	years_of_experience INT NULL,
	status NVARCHAR(20) NOT NULL DEFAULT N'active',
	created_at DATETIME2(7) NOT NULL DEFAULT SYSDATETIME(),
	updated_at DATETIME2(7) NULL,
	CONSTRAINT FK_veterinarians_users FOREIGN KEY (user_id) REFERENCES users(id)
);
GO

/* Dịch vụ chăm sóc thú cưng */
CREATE TABLE services (
	id INT IDENTITY(1,1) PRIMARY KEY,
	name NVARCHAR(255) NOT NULL,
	description NVARCHAR(MAX) NULL,
	price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
	duration_minutes INT NOT NULL CHECK (duration_minutes > 0),
	image_url VARCHAR(MAX) NULL,
	status TINYINT NOT NULL DEFAULT 1,
	created_at DATETIME NULL DEFAULT GETDATE()
);
GO

/* Gói dịch vụ */
CREATE TABLE service_packages (
	id INT IDENTITY(1,1) PRIMARY KEY,
	name NVARCHAR(255) NOT NULL,
	description NVARCHAR(MAX) NULL,
	price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
	duration_minutes INT NOT NULL CHECK (duration_minutes > 0),
	status TINYINT NOT NULL DEFAULT 1,
	created_at DATETIME NULL DEFAULT GETDATE(),
	validity_days INT NOT NULL CHECK (validity_days > 0)
);
GO

/* 2. Commerce tables */
/* Danh mục sản phẩm */
CREATE TABLE categories (
	id INT IDENTITY(1,1) PRIMARY KEY,
	category_name NVARCHAR(100) NOT NULL UNIQUE,
	description NVARCHAR(255) NULL,
	created_at DATETIME DEFAULT GETDATE()
);
GO

/* Nhà cung cấp */
CREATE TABLE suppliers (
	id INT IDENTITY(1,1) PRIMARY KEY,
	supplier_name NVARCHAR(150) NOT NULL,
	phone VARCHAR(15) NOT NULL,
	email VARCHAR(100) NULL UNIQUE,
	address NVARCHAR(255) NULL,
	created_at DATETIME DEFAULT GETDATE()
);
GO

/* Sản phẩm */
CREATE TABLE products (
	id INT IDENTITY(1,1) PRIMARY KEY,
	category_id INT NOT NULL,
	product_name NVARCHAR(150) NOT NULL,
	description NVARCHAR(MAX) NULL,
	unit NVARCHAR(20) NULL,
	import_price DECIMAL(12,2) NOT NULL CHECK (import_price >= 0),
	sell_price DECIMAL(12,2) NOT NULL CHECK (sell_price >= 0),
	stock_quantity INT NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
	image_url VARCHAR(MAX) NULL,
	status VARCHAR(20) NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'inactive')),
	created_at DATETIME DEFAULT GETDATE(),
	updated_at DATETIME NULL,
	FOREIGN KEY (category_id) REFERENCES categories(id)
);
GO

/* Phiếu nhập hàng */
CREATE TABLE import_receipts (
	id INT IDENTITY(1,1) PRIMARY KEY,
	supplier_id INT NOT NULL,
	employee_id INT NULL,
	receipt_date DATETIME NOT NULL DEFAULT GETDATE(),
	total_amount DECIMAL(14,2) NOT NULL DEFAULT 0 CHECK (total_amount >= 0),
	note NVARCHAR(255) NULL,
	FOREIGN KEY (supplier_id) REFERENCES suppliers(id),
	FOREIGN KEY (employee_id) REFERENCES users(id)
);
GO

/* Chi tiết phiếu nhập hàng */
CREATE TABLE import_details (
	id INT IDENTITY(1,1) PRIMARY KEY,
	receipt_id INT NOT NULL,
	product_id INT NOT NULL,
	quantity INT NOT NULL CHECK (quantity > 0),
	import_price DECIMAL(12,2) NOT NULL CHECK (import_price >= 0),
	subtotal DECIMAL(14,2) NOT NULL,
	UNIQUE (receipt_id, product_id),
	FOREIGN KEY (receipt_id) REFERENCES import_receipts(id) ON DELETE CASCADE,
	FOREIGN KEY (product_id) REFERENCES products(id)
);
GO

/* Giỏ hàng */
CREATE TABLE carts (
	id INT IDENTITY(1,1) PRIMARY KEY,
	user_id INT NOT NULL,
	status VARCHAR(20) NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'checked_out')),
	created_at DATETIME DEFAULT GETDATE(),
	updated_at DATETIME NULL,
	FOREIGN KEY (user_id) REFERENCES users(id)
);
GO

/* Sản phẩm trong giỏ hàng */
CREATE TABLE cart_items (
	id INT IDENTITY(1,1) PRIMARY KEY,
	cart_id INT NOT NULL,
	product_id INT NOT NULL,
	quantity INT NOT NULL DEFAULT 1 CHECK (quantity > 0),
	price DECIMAL(12,2) NOT NULL CHECK (price >= 0),
	UNIQUE (cart_id, product_id),
	FOREIGN KEY (cart_id) REFERENCES carts(id) ON DELETE CASCADE,
	FOREIGN KEY (product_id) REFERENCES products(id)
);
GO

/* Đơn hàng */
CREATE TABLE orders (
	id INT IDENTITY(1,1) PRIMARY KEY,
	user_id INT NOT NULL,
	order_date DATETIME NOT NULL DEFAULT GETDATE(),
	shipping_address NVARCHAR(255) NOT NULL,
	phone VARCHAR(15) NOT NULL,
	total_amount DECIMAL(14,2) NOT NULL DEFAULT 0 CHECK (total_amount >= 0),
	status VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'confirmed', 'shipping', 'completed', 'cancelled')),
	note NVARCHAR(255) NULL,
	FOREIGN KEY (user_id) REFERENCES users(id)
);
GO

/* Sản phẩm trong đơn hàng */
CREATE TABLE order_items (
	id INT IDENTITY(1,1) PRIMARY KEY,
	order_id INT NOT NULL,
	product_id INT NOT NULL,
	quantity INT NOT NULL CHECK (quantity > 0),
	price DECIMAL(12,2) NOT NULL CHECK (price >= 0),
	subtotal DECIMAL(14,2) NOT NULL,
	UNIQUE (order_id, product_id),
	FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
	FOREIGN KEY (product_id) REFERENCES products(id)
);
GO

/* Thanh toán */
CREATE TABLE payments (
	id INT IDENTITY(1,1) PRIMARY KEY,
	order_id INT NOT NULL,
	payment_method VARCHAR(20) NOT NULL CHECK (payment_method IN ('cash', 'bank_transfer', 'credit_card', 'e_wallet')),
	amount DECIMAL(14,2) NOT NULL CHECK (amount >= 0),
	payment_date DATETIME NOT NULL DEFAULT GETDATE(),
	status VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'completed', 'failed', 'refunded')),
	transaction_code VARCHAR(100) NULL UNIQUE,
	FOREIGN KEY (order_id) REFERENCES orders(id)
);
GO

/* Hóa đơn */
CREATE TABLE invoices (
	id INT IDENTITY(1,1) PRIMARY KEY,
	order_id INT NOT NULL UNIQUE,
	invoice_date DATETIME NOT NULL DEFAULT GETDATE(),
	total_amount DECIMAL(14,2) NOT NULL CHECK (total_amount >= 0),
	tax_amount DECIMAL(14,2) NOT NULL DEFAULT 0 CHECK (tax_amount >= 0),
	issued_by INT NULL,
	FOREIGN KEY (order_id) REFERENCES orders(id),
	FOREIGN KEY (issued_by) REFERENCES users(id)
);
GO

/* 3. Pets */
/* Thú cưng */
CREATE TABLE pets (
	id INT IDENTITY(1,1) PRIMARY KEY,
	user_id INT NOT NULL,
	qr_token VARCHAR(100) NOT NULL UNIQUE,
	name NVARCHAR(100) NOT NULL,
	species NVARCHAR(50) NOT NULL,
	breed NVARCHAR(80) NULL,
	gender VARCHAR(10) NOT NULL CHECK (gender IN ('Male', 'Female')),
	weight_kg DECIMAL(4,2) NULL,
	created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
	updated_at DATETIME2 NULL,
	CONSTRAINT FK_pets_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_pets_user_id ON pets(user_id);
GO

/* 4. Appointments and veterinary records */
/* Lịch hẹn khám */
CREATE TABLE appointments (
	id INT IDENTITY(1,1) PRIMARY KEY,
	pet_id INT NOT NULL,
	user_id INT NOT NULL,
	vet_id INT NULL,
	appointment_date DATE NOT NULL,
	appointment_time TIME NOT NULL,
	reason NVARCHAR(MAX) NULL,
	status TINYINT NOT NULL DEFAULT 1,
	FOREIGN KEY (pet_id) REFERENCES pets(id),
	FOREIGN KEY (user_id) REFERENCES users(id),
	FOREIGN KEY (vet_id) REFERENCES veterinarians(id)
);
GO

CREATE NONCLUSTERED INDEX IX_appointments_pet_id ON appointments(pet_id);
CREATE NONCLUSTERED INDEX IX_appointments_user_id ON appointments(user_id);
CREATE NONCLUSTERED INDEX IX_appointments_date ON appointments(appointment_date);
GO

/* Dịch vụ được đặt trong lịch hẹn */
CREATE TABLE appointments_services (
	appointment_id INT NOT NULL,
	service_id INT NOT NULL,
	price_at_booking DECIMAL(10,2) NOT NULL CHECK (price_at_booking >= 0),
	PRIMARY KEY (appointment_id, service_id),
	FOREIGN KEY (appointment_id) REFERENCES appointments(id),
	FOREIGN KEY (service_id) REFERENCES services(id)
);
GO

/* Hồ sơ sức khỏe thú cưng */
CREATE TABLE pet_health_records (
	id INT IDENTITY(1,1) PRIMARY KEY,
	pet_id INT NOT NULL,
	appointment_id INT NULL,
	vet_id INT NOT NULL,
	weight_kg DECIMAL(5,2) NULL,
	temperature DECIMAL(4,1) NULL,
	diagnosis NVARCHAR(MAX) NULL,
	treatment NVARCHAR(MAX) NULL,
	visit_date DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
	FOREIGN KEY (pet_id) REFERENCES pets(id),
	FOREIGN KEY (appointment_id) REFERENCES appointments(id),
	FOREIGN KEY (vet_id) REFERENCES veterinarians(id)
);
GO

CREATE NONCLUSTERED INDEX IX_pet_health_records_pet_id ON pet_health_records(pet_id);
GO

/* Lịch sử tiêm chủng */
CREATE TABLE vaccination_records (
	id INT IDENTITY(1,1) PRIMARY KEY,
	pet_id INT NOT NULL,
	vaccine_name NVARCHAR(255) NOT NULL,
	batch_number VARCHAR(100) NULL,
	administered_date DATE NOT NULL,
	next_due_date DATE NULL,
	vet_id INT NOT NULL,
	FOREIGN KEY (pet_id) REFERENCES pets(id),
	FOREIGN KEY (vet_id) REFERENCES veterinarians(id)
);
GO

CREATE NONCLUSTERED INDEX IX_vaccination_records_pet_id ON vaccination_records(pet_id);
GO

/* Đơn thuốc */
CREATE TABLE medical_prescriptions (
	id INT IDENTITY(1,1) PRIMARY KEY,
	record_id INT NOT NULL,
	medication_name NVARCHAR(255) NOT NULL,
	dosage NVARCHAR(100) NOT NULL,
	frequency NVARCHAR(100) NOT NULL,
	duration_days INT NOT NULL CHECK (duration_days > 0),
	FOREIGN KEY (record_id) REFERENCES pet_health_records(id)
);
GO

CREATE NONCLUSTERED INDEX IX_medical_prescriptions_record_id ON medical_prescriptions(record_id);
GO

/* Gói dịch vụ của thú cưng */
CREATE TABLE pet_packages (
	id INT IDENTITY(1,1) PRIMARY KEY,
	pet_id INT NOT NULL,
	package_id INT NOT NULL,
	start_date DATE NOT NULL,
	end_date DATE NOT NULL,
	status TINYINT NOT NULL DEFAULT 1,
	FOREIGN KEY (pet_id) REFERENCES pets(id),
	FOREIGN KEY (package_id) REFERENCES service_packages(id)
);
GO

CREATE NONCLUSTERED INDEX IX_pet_packages_pet_id ON pet_packages(pet_id);
GO

/* 5. Customer care */
/* Thông báo */
CREATE TABLE notifications (
	id INT IDENTITY(1,1) PRIMARY KEY,
	user_id INT NOT NULL,
	appointment_id INT NULL,
	message NVARCHAR(1000) NOT NULL,
	type VARCHAR(20) NOT NULL CHECK (type IN ('appointment', 'promotion', 'system')),
	is_read BIT NOT NULL DEFAULT 0,
	created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
	CONSTRAINT FK_notifications_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
	CONSTRAINT FK_notifications_appointments FOREIGN KEY (appointment_id) REFERENCES appointments(id) ON DELETE SET NULL
);
GO

CREATE INDEX IX_notifications_user_id ON notifications(user_id);
GO

/* Phản hồi của khách hàng */
CREATE TABLE feedbacks (
	id INT IDENTITY(1,1) PRIMARY KEY,
	user_id INT NOT NULL,
	comment NVARCHAR(2000) NOT NULL,
	status VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'resolved')),
	created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
	CONSTRAINT FK_feedbacks_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_feedbacks_user_id ON feedbacks(user_id);
GO

/* Lịch sử hội thoại với trợ lý AI */
CREATE TABLE chat_conversations (
	id INT IDENTITY(1,1) PRIMARY KEY,
	user_id INT NOT NULL,
	title NVARCHAR(255) NULL,
	user_request NVARCHAR(MAX) NOT NULL,
	ai_response NVARCHAR(MAX) NOT NULL,
	created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
	CONSTRAINT FK_chat_conversations_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_chat_conversations_user_id ON chat_conversations(user_id);
GO

/* 6. Breed prediction and pet images */
/* Kết quả dự đoán giống thú cưng */
CREATE TABLE breed_predictions (
	id INT IDENTITY(1,1) PRIMARY KEY,
	user_id INT NULL,
	image_url NVARCHAR(500) NOT NULL,
	predicted_breed NVARCHAR(100) NOT NULL,
	confidence DECIMAL(5,4) NOT NULL CHECK (confidence BETWEEN 0.0000 AND 1.0000),
	execution_time_ms INT NULL,
	created_at DATETIME NOT NULL DEFAULT GETDATE(),
	CONSTRAINT FK_BreedPredictions_Users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
);
GO

/* Chi tiết các kết quả dự đoán giống */
CREATE TABLE breed_prediction_details (
	id INT IDENTITY(1,1) PRIMARY KEY,
	prediction_id INT NOT NULL,
	predicted_breed NVARCHAR(100) NOT NULL,
	rank_order TINYINT NOT NULL,
	confidence DECIMAL(5,4) NOT NULL CHECK (confidence BETWEEN 0.0000 AND 1.0000),
	CONSTRAINT FK_BreedPredictionDetails_Predictions FOREIGN KEY (prediction_id)
		REFERENCES breed_predictions(id) ON DELETE CASCADE,
	CONSTRAINT UQ_BreedPredictionDetails_Rank UNIQUE (prediction_id, rank_order),
	CONSTRAINT UQ_BreedPredictionDetails_Breed UNIQUE (prediction_id, predicted_breed)
);
GO

/* Hình ảnh thú cưng */
CREATE TABLE pet_images (
	id INT IDENTITY(1,1) PRIMARY KEY,
	pet_id INT NOT NULL,
	image_url NVARCHAR(500) NOT NULL,
	is_avatar BIT DEFAULT 0,
	uploaded_at DATETIME DEFAULT GETDATE(),
	CONSTRAINT FK_PetImages_Pets FOREIGN KEY (pet_id) REFERENCES pets(id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_BreedPredictions_UserId ON breed_predictions(user_id);
CREATE INDEX IX_BreedPredictionDetails_PredictionId ON breed_prediction_details(prediction_id);
CREATE INDEX IX_PetImages_PetId ON pet_images(pet_id);
GO
