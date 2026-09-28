USE [master]
GO
IF DB_ID(N'PetsManager') IS NULL
BEGIN
	CREATE DATABASE [PetsManager]
END
GO
USE [PetsManager]
GO
ALTER DATABASE [PetsManager] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [PetsManager].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [PetsManager] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [PetsManager] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [PetsManager] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [PetsManager] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [PetsManager] SET ARITHABORT OFF 
GO
ALTER DATABASE [PetsManager] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [PetsManager] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [PetsManager] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [PetsManager] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [PetsManager] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [PetsManager] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [PetsManager] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [PetsManager] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [PetsManager] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [PetsManager] SET  ENABLE_BROKER 
GO
ALTER DATABASE [PetsManager] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [PetsManager] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [PetsManager] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [PetsManager] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [PetsManager] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [PetsManager] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [PetsManager] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [PetsManager] SET RECOVERY FULL 
GO
ALTER DATABASE [PetsManager] SET  MULTI_USER 
GO
ALTER DATABASE [PetsManager] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [PetsManager] SET DB_CHAINING OFF 
GO
ALTER DATABASE [PetsManager] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [PetsManager] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [PetsManager] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [PetsManager] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [PetsManager] SET QUERY_STORE = ON
GO
ALTER DATABASE [PetsManager] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [PetsManager]
GO
/****** Object:  Table [dbo].[appointments]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[appointments](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[pet_id] [int] NOT NULL,
	[user_id] [int] NOT NULL,
	[vet_id] [int] NULL,
	[appointment_date] [date] NOT NULL,
	[appointment_time] [time](7) NOT NULL,
	[reason] [nvarchar](max) NULL,
	[status] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[appointments_services]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[appointments_services](
	[appointment_id] [int] NOT NULL,
	[service_id] [int] NOT NULL,
	[price_at_booking] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[appointment_id] ASC,
	[service_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[breed_prediction_details]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[breed_prediction_details](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[prediction_id] [int] NOT NULL,
	[predicted_breed] [nvarchar](100) NOT NULL,
	[rank_order] [tinyint] NOT NULL,
	[confidence] [decimal](5, 4) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[breed_predictions]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[breed_predictions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NULL,
	[image_url] [nvarchar](500) NOT NULL,
	[predicted_breed] [nvarchar](100) NOT NULL,
	[confidence] [decimal](5, 4) NOT NULL,
	[execution_time_ms] [int] NULL,
	[created_at] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[cart_items]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cart_items](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[cart_id] [int] NOT NULL,
	[product_id] [int] NOT NULL,
	[quantity] [int] NOT NULL,
	[price] [decimal](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[carts]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[carts](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[status] [varchar](20) NOT NULL,
	[created_at] [datetime] NULL,
	[updated_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[categories]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[categories](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[category_name] [nvarchar](100) NOT NULL,
	[description] [nvarchar](255) NULL,
	[created_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[chat_conversations]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[chat_conversations](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[title] [nvarchar](255) NULL,
	[user_request] [nvarchar](max) NOT NULL,
	[ai_response] [nvarchar](max) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[feedbacks]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[feedbacks](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[comment] [nvarchar](2000) NOT NULL,
	[status] [varchar](20) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[import_details]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[import_details](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[receipt_id] [int] NOT NULL,
	[product_id] [int] NOT NULL,
	[quantity] [int] NOT NULL,
	[import_price] [decimal](12, 2) NOT NULL,
	[subtotal] [decimal](14, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[import_receipts]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[import_receipts](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[supplier_id] [int] NOT NULL,
	[employee_id] [int] NULL,
	[receipt_date] [datetime] NOT NULL,
	[total_amount] [decimal](14, 2) NOT NULL,
	[note] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[invoices]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[invoices](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[order_id] [int] NOT NULL,
	[invoice_date] [datetime] NOT NULL,
	[total_amount] [decimal](14, 2) NOT NULL,
	[tax_amount] [decimal](14, 2) NOT NULL,
	[issued_by] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[medical_prescriptions]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[medical_prescriptions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[record_id] [int] NOT NULL,
	[medication_name] [nvarchar](255) NOT NULL,
	[dosage] [nvarchar](100) NOT NULL,
	[frequency] [nvarchar](100) NOT NULL,
	[duration_days] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[notifications]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[notifications](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[appointment_id] [int] NULL,
	[message] [nvarchar](1000) NOT NULL,
	[type] [varchar](20) NOT NULL,
	[is_read] [bit] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[order_items]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[order_items](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[order_id] [int] NOT NULL,
	[product_id] [int] NOT NULL,
	[quantity] [int] NOT NULL,
	[price] [decimal](12, 2) NOT NULL,
	[subtotal] [decimal](14, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[orders]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[orders](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[order_date] [datetime] NOT NULL,
	[shipping_address] [nvarchar](255) NOT NULL,
	[phone] [varchar](15) NOT NULL,
	[total_amount] [decimal](14, 2) NOT NULL,
	[status] [varchar](20) NOT NULL,
	[note] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[payments]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[payments](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[order_id] [int] NOT NULL,
	[payment_method] [varchar](20) NOT NULL,
	[amount] [decimal](14, 2) NOT NULL,
	[payment_date] [datetime] NOT NULL,
	[status] [varchar](20) NOT NULL,
	[transaction_code] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[permissions]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[permissions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[code] [nvarchar](60) NOT NULL,
	[name] [nvarchar](40) NOT NULL,
	[description] [nvarchar](255) NULL,
	[module] [nvarchar](40) NOT NULL,
	[is_active] [bit] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[pet_health_records]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[pet_health_records](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[pet_id] [int] NOT NULL,
	[appointment_id] [int] NULL,
	[vet_id] [int] NOT NULL,
	[weight_kg] [decimal](5, 2) NULL,
	[temperature] [decimal](4, 1) NULL,
	[diagnosis] [nvarchar](max) NULL,
	[treatment] [nvarchar](max) NULL,
	[visit_date] [date] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[pet_images]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[pet_images](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[pet_id] [int] NOT NULL,
	[image_url] [nvarchar](500) NOT NULL,
	[is_avatar] [bit] NULL,
	[uploaded_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[pet_packages]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[pet_packages](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[pet_id] [int] NOT NULL,
	[package_id] [int] NOT NULL,
	[start_date] [date] NOT NULL,
	[end_date] [date] NOT NULL,
	[status] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[pets]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[pets](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[qr_token] [varchar](100) NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[species] [nvarchar](50) NOT NULL,
	[breed] [nvarchar](80) NULL,
	[gender] [varchar](10) NOT NULL,
	[weight_kg] [decimal](4, 2) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[products]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[products](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[category_id] [int] NOT NULL,
	[product_name] [nvarchar](150) NOT NULL,
	[description] [nvarchar](max) NULL,
	[unit] [nvarchar](20) NULL,
	[import_price] [decimal](12, 2) NOT NULL,
	[sell_price] [decimal](12, 2) NOT NULL,
	[stock_quantity] [int] NOT NULL,
	[image_url] [varchar](max) NULL,
	[status] [varchar](20) NOT NULL,
	[created_at] [datetime] NULL,
	[updated_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[role_permissions]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[role_permissions](
	[role_id] [int] NOT NULL,
	[permission_id] [int] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_role_permissions] PRIMARY KEY CLUSTERED 
(
	[role_id] ASC,
	[permission_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[roles]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[roles](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](40) NOT NULL,
	[description] [nvarchar](255) NULL,
	[status] [nvarchar](20) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[service_packages]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[service_packages](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[description] [nvarchar](max) NULL,
	[price] [decimal](10, 2) NOT NULL,
	[duration_minutes] [int] NOT NULL,
	[status] [tinyint] NOT NULL,
	[created_at] [datetime] NULL,
	[validity_days] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[service_package_services] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[service_package_services](
	[package_id] [int] NOT NULL,
	[service_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED
(
	[package_id] ASC,
	[service_id] ASC
)
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[services]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[services](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[description] [nvarchar](max) NULL,
	[price] [decimal](10, 2) NOT NULL,
	[duration_minutes] [int] NOT NULL,
	[image_url] [varchar](max) NULL,
	[status] [tinyint] NOT NULL,
	[created_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[staff]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[staff](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[position] [nvarchar](60) NULL,
	[hire_date] [date] NULL,
	[status] [nvarchar](20) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[suppliers]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[suppliers](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[supplier_name] [nvarchar](150) NOT NULL,
	[phone] [varchar](15) NOT NULL,
	[email] [varchar](100) NULL,
	[address] [nvarchar](255) NULL,
	[created_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[users]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[users](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[role_id] [int] NOT NULL,
	[username] [nvarchar](40) NOT NULL,
	[password_hash] [nvarchar](255) NOT NULL,
	[full_name] [nvarchar](40) NULL,
	[email] [nvarchar](100) NOT NULL,
	[phone_number] [nvarchar](40) NULL,
	[address] [nvarchar](255) NULL,
	[status] [nvarchar](20) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[vaccination_records]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[vaccination_records](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[pet_id] [int] NOT NULL,
	[vaccine_name] [nvarchar](255) NOT NULL,
	[batch_number] [varchar](100) NULL,
	[administered_date] [date] NOT NULL,
	[next_due_date] [date] NULL,
	[vet_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[veterinarians]    Script Date: 9/17/2026 10:26:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[veterinarians](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[specialty] [nvarchar](100) NULL,
	[license_no] [nvarchar](50) NULL,
	[years_of_experience] [int] NULL,
	[status] [nvarchar](20) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[appointments] ON 

INSERT [dbo].[appointments] ([id], [pet_id], [user_id], [vet_id], [appointment_date], [appointment_time], [reason], [status]) VALUES (1, 1, 1, 1, CAST(N'2026-10-01' AS Date), CAST(N'09:00:00' AS Time), N'Khám định kỳ', 2)
INSERT [dbo].[appointments] ([id], [pet_id], [user_id], [vet_id], [appointment_date], [appointment_time], [reason], [status]) VALUES (2, 2, 2, 2, CAST(N'2026-10-02' AS Date), CAST(N'10:00:00' AS Time), N'Tiêm phòng', 1)
INSERT [dbo].[appointments] ([id], [pet_id], [user_id], [vet_id], [appointment_date], [appointment_time], [reason], [status]) VALUES (3, 3, 3, 3, CAST(N'2026-10-03' AS Date), CAST(N'11:00:00' AS Time), N'Kiểm tra da', 3)
INSERT [dbo].[appointments] ([id], [pet_id], [user_id], [vet_id], [appointment_date], [appointment_time], [reason], [status]) VALUES (4, 4, 4, 4, CAST(N'2026-10-04' AS Date), CAST(N'14:00:00' AS Time), N'Tư vấn giống', 4)
INSERT [dbo].[appointments] ([id], [pet_id], [user_id], [vet_id], [appointment_date], [appointment_time], [reason], [status]) VALUES (5, 5, 5, 5, CAST(N'2026-10-05' AS Date), CAST(N'15:00:00' AS Time), N'Tái khám', 2)
SET IDENTITY_INSERT [dbo].[appointments] OFF
GO
INSERT [dbo].[appointments_services] ([appointment_id], [service_id], [price_at_booking]) VALUES (1, 1, CAST(150000.00 AS Decimal(10, 2)))
INSERT [dbo].[appointments_services] ([appointment_id], [service_id], [price_at_booking]) VALUES (2, 2, CAST(200000.00 AS Decimal(10, 2)))
INSERT [dbo].[appointments_services] ([appointment_id], [service_id], [price_at_booking]) VALUES (3, 3, CAST(120000.00 AS Decimal(10, 2)))
INSERT [dbo].[appointments_services] ([appointment_id], [service_id], [price_at_booking]) VALUES (4, 4, CAST(180000.00 AS Decimal(10, 2)))
INSERT [dbo].[appointments_services] ([appointment_id], [service_id], [price_at_booking]) VALUES (5, 5, CAST(350000.00 AS Decimal(10, 2)))
GO
SET IDENTITY_INSERT [dbo].[breed_prediction_details] ON 

INSERT [dbo].[breed_prediction_details] ([id], [prediction_id], [predicted_breed], [rank_order], [confidence]) VALUES (1, 1, N'Poodle', 1, CAST(0.9500 AS Decimal(5, 4)))
INSERT [dbo].[breed_prediction_details] ([id], [prediction_id], [predicted_breed], [rank_order], [confidence]) VALUES (2, 2, N'Munchkin', 1, CAST(0.9100 AS Decimal(5, 4)))
INSERT [dbo].[breed_prediction_details] ([id], [prediction_id], [predicted_breed], [rank_order], [confidence]) VALUES (3, 3, N'Golden Retriever', 1, CAST(0.8800 AS Decimal(5, 4)))
INSERT [dbo].[breed_prediction_details] ([id], [prediction_id], [predicted_breed], [rank_order], [confidence]) VALUES (4, 4, N'Mèo Anh lông ngắn', 1, CAST(0.9300 AS Decimal(5, 4)))
INSERT [dbo].[breed_prediction_details] ([id], [prediction_id], [predicted_breed], [rank_order], [confidence]) VALUES (5, 5, N'Thỏ trắng', 1, CAST(0.8700 AS Decimal(5, 4)))
SET IDENTITY_INSERT [dbo].[breed_prediction_details] OFF
GO
SET IDENTITY_INSERT [dbo].[breed_predictions] ON 

INSERT [dbo].[breed_predictions] ([id], [user_id], [image_url], [predicted_breed], [confidence], [execution_time_ms], [created_at]) VALUES (1, 1, N'prediction-01.jpg', N'Poodle', CAST(0.9500 AS Decimal(5, 4)), 120, CAST(N'2026-09-17T21:27:35.877' AS DateTime))
INSERT [dbo].[breed_predictions] ([id], [user_id], [image_url], [predicted_breed], [confidence], [execution_time_ms], [created_at]) VALUES (2, 2, N'prediction-02.jpg', N'Munchkin', CAST(0.9100 AS Decimal(5, 4)), 135, CAST(N'2026-09-17T21:27:35.877' AS DateTime))
INSERT [dbo].[breed_predictions] ([id], [user_id], [image_url], [predicted_breed], [confidence], [execution_time_ms], [created_at]) VALUES (3, 3, N'prediction-03.jpg', N'Golden Retriever', CAST(0.8800 AS Decimal(5, 4)), 110, CAST(N'2026-09-17T21:27:35.877' AS DateTime))
INSERT [dbo].[breed_predictions] ([id], [user_id], [image_url], [predicted_breed], [confidence], [execution_time_ms], [created_at]) VALUES (4, 4, N'prediction-04.jpg', N'Mèo Anh lông ngắn', CAST(0.9300 AS Decimal(5, 4)), 125, CAST(N'2026-09-17T21:27:35.877' AS DateTime))
INSERT [dbo].[breed_predictions] ([id], [user_id], [image_url], [predicted_breed], [confidence], [execution_time_ms], [created_at]) VALUES (5, 5, N'prediction-05.jpg', N'Thỏ trắng', CAST(0.8700 AS Decimal(5, 4)), 140, CAST(N'2026-09-17T21:27:35.877' AS DateTime))
SET IDENTITY_INSERT [dbo].[breed_predictions] OFF
GO
SET IDENTITY_INSERT [dbo].[cart_items] ON 

INSERT [dbo].[cart_items] ([id], [cart_id], [product_id], [quantity], [price]) VALUES (1, 1, 1, 2, CAST(120000.00 AS Decimal(12, 2)))
INSERT [dbo].[cart_items] ([id], [cart_id], [product_id], [quantity], [price]) VALUES (2, 2, 2, 3, CAST(40000.00 AS Decimal(12, 2)))
INSERT [dbo].[cart_items] ([id], [cart_id], [product_id], [quantity], [price]) VALUES (3, 3, 3, 1, CAST(90000.00 AS Decimal(12, 2)))
INSERT [dbo].[cart_items] ([id], [cart_id], [product_id], [quantity], [price]) VALUES (4, 4, 4, 2, CAST(100000.00 AS Decimal(12, 2)))
INSERT [dbo].[cart_items] ([id], [cart_id], [product_id], [quantity], [price]) VALUES (5, 5, 5, 1, CAST(55000.00 AS Decimal(12, 2)))
SET IDENTITY_INSERT [dbo].[cart_items] OFF
GO
SET IDENTITY_INSERT [dbo].[carts] ON 

INSERT [dbo].[carts] ([id], [user_id], [status], [created_at], [updated_at]) VALUES (1, 1, N'active', CAST(N'2026-09-17T21:27:35.703' AS DateTime), NULL)
INSERT [dbo].[carts] ([id], [user_id], [status], [created_at], [updated_at]) VALUES (2, 2, N'active', CAST(N'2026-09-17T21:27:35.703' AS DateTime), NULL)
INSERT [dbo].[carts] ([id], [user_id], [status], [created_at], [updated_at]) VALUES (3, 3, N'active', CAST(N'2026-09-17T21:27:35.703' AS DateTime), NULL)
INSERT [dbo].[carts] ([id], [user_id], [status], [created_at], [updated_at]) VALUES (4, 4, N'active', CAST(N'2026-09-17T21:27:35.703' AS DateTime), NULL)
INSERT [dbo].[carts] ([id], [user_id], [status], [created_at], [updated_at]) VALUES (5, 5, N'checked_out', CAST(N'2026-09-17T21:27:35.703' AS DateTime), NULL)
SET IDENTITY_INSERT [dbo].[carts] OFF
GO
SET IDENTITY_INSERT [dbo].[categories] ON 

INSERT [dbo].[categories] ([id], [category_name], [description], [created_at]) VALUES (1, N'Thức ăn', N'Thức ăn cho thú cưng', CAST(N'2026-09-17T21:27:35.660' AS DateTime))
INSERT [dbo].[categories] ([id], [category_name], [description], [created_at]) VALUES (2, N'Phụ kiện', N'Vòng cổ và đồ dùng', CAST(N'2026-09-17T21:27:35.660' AS DateTime))
INSERT [dbo].[categories] ([id], [category_name], [description], [created_at]) VALUES (3, N'Vệ sinh', N'Sản phẩm vệ sinh', CAST(N'2026-09-17T21:27:35.660' AS DateTime))
INSERT [dbo].[categories] ([id], [category_name], [description], [created_at]) VALUES (4, N'Y tế', N'Sản phẩm chăm sóc sức khỏe', CAST(N'2026-09-17T21:27:35.660' AS DateTime))
INSERT [dbo].[categories] ([id], [category_name], [description], [created_at]) VALUES (5, N'Đồ chơi', N'Đồ chơi cho thú cưng', CAST(N'2026-09-17T21:27:35.660' AS DateTime))
SET IDENTITY_INSERT [dbo].[categories] OFF
GO
SET IDENTITY_INSERT [dbo].[chat_conversations] ON 

INSERT [dbo].[chat_conversations] ([id], [user_id], [title], [user_request], [ai_response], [created_at]) VALUES (1, 1, N'Chăm sóc chó', N'Chó nên ăn gì?', N'Nên ăn thức ăn phù hợp độ tuổi và cân nặng.', CAST(N'2026-09-17T21:27:35.8666667' AS DateTime2))
INSERT [dbo].[chat_conversations] ([id], [user_id], [title], [user_request], [ai_response], [created_at]) VALUES (2, 2, N'Tiêm phòng', N'Mèo bao lâu tiêm phòng?', N'Thông thường nên tiêm theo lịch của bác sĩ thú y.', CAST(N'2026-09-17T21:27:35.8666667' AS DateTime2))
INSERT [dbo].[chat_conversations] ([id], [user_id], [title], [user_request], [ai_response], [created_at]) VALUES (3, 3, N'Vệ sinh', N'Bao lâu tắm chó?', N'Có thể tắm mỗi 2 đến 4 tuần tùy giống và tình trạng da.', CAST(N'2026-09-17T21:27:35.8666667' AS DateTime2))
INSERT [dbo].[chat_conversations] ([id], [user_id], [title], [user_request], [ai_response], [created_at]) VALUES (4, 4, N'Dinh dưỡng', N'Mèo có ăn được sữa không?', N'Nên hạn chế sữa bò vì có thể gây rối loạn tiêu hóa.', CAST(N'2026-09-17T21:27:35.8666667' AS DateTime2))
INSERT [dbo].[chat_conversations] ([id], [user_id], [title], [user_request], [ai_response], [created_at]) VALUES (5, 5, N'Sức khỏe', N'Làm sao theo dõi cân nặng?', N'Cần đo định kỳ và ghi lại trong hồ sơ sức khỏe.', CAST(N'2026-09-17T21:27:35.8666667' AS DateTime2))
SET IDENTITY_INSERT [dbo].[chat_conversations] OFF
GO
SET IDENTITY_INSERT [dbo].[feedbacks] ON 

INSERT [dbo].[feedbacks] ([id], [user_id], [comment], [status], [created_at]) VALUES (1, 1, N'Dịch vụ tốt', N'resolved', CAST(N'2026-09-17T21:27:35.8566667' AS DateTime2))
INSERT [dbo].[feedbacks] ([id], [user_id], [comment], [status], [created_at]) VALUES (2, 2, N'Cần thêm khung giờ hẹn', N'pending', CAST(N'2026-09-17T21:27:35.8566667' AS DateTime2))
INSERT [dbo].[feedbacks] ([id], [user_id], [comment], [status], [created_at]) VALUES (3, 3, N'Nhân viên thân thiện', N'resolved', CAST(N'2026-09-17T21:27:35.8566667' AS DateTime2))
INSERT [dbo].[feedbacks] ([id], [user_id], [comment], [status], [created_at]) VALUES (4, 4, N'Muốn có thêm sản phẩm', N'pending', CAST(N'2026-09-17T21:27:35.8566667' AS DateTime2))
INSERT [dbo].[feedbacks] ([id], [user_id], [comment], [status], [created_at]) VALUES (5, 5, N'Giao hàng đúng hẹn', N'resolved', CAST(N'2026-09-17T21:27:35.8566667' AS DateTime2))
SET IDENTITY_INSERT [dbo].[feedbacks] OFF
GO
SET IDENTITY_INSERT [dbo].[import_details] ON 

INSERT [dbo].[import_details] ([id], [receipt_id], [product_id], [quantity], [import_price], [subtotal]) VALUES (1, 1, 1, 20, CAST(80000.00 AS Decimal(12, 2)), CAST(1600000.00 AS Decimal(14, 2)))
INSERT [dbo].[import_details] ([id], [receipt_id], [product_id], [quantity], [import_price], [subtotal]) VALUES (2, 2, 2, 40, CAST(25000.00 AS Decimal(12, 2)), CAST(1000000.00 AS Decimal(14, 2)))
INSERT [dbo].[import_details] ([id], [receipt_id], [product_id], [quantity], [import_price], [subtotal]) VALUES (3, 3, 3, 10, CAST(50000.00 AS Decimal(12, 2)), CAST(500000.00 AS Decimal(14, 2)))
INSERT [dbo].[import_details] ([id], [receipt_id], [product_id], [quantity], [import_price], [subtotal]) VALUES (4, 4, 4, 15, CAST(60000.00 AS Decimal(12, 2)), CAST(900000.00 AS Decimal(14, 2)))
INSERT [dbo].[import_details] ([id], [receipt_id], [product_id], [quantity], [import_price], [subtotal]) VALUES (5, 5, 5, 25, CAST(30000.00 AS Decimal(12, 2)), CAST(750000.00 AS Decimal(14, 2)))
SET IDENTITY_INSERT [dbo].[import_details] OFF
GO
SET IDENTITY_INSERT [dbo].[import_receipts] ON 

INSERT [dbo].[import_receipts] ([id], [supplier_id], [employee_id], [receipt_date], [total_amount], [note]) VALUES (1, 1, 1, CAST(N'2026-09-17T21:27:35.683' AS DateTime), CAST(6000000.00 AS Decimal(14, 2)), N'Nhập hàng đợt 1')
INSERT [dbo].[import_receipts] ([id], [supplier_id], [employee_id], [receipt_date], [total_amount], [note]) VALUES (2, 2, 2, CAST(N'2026-09-17T21:27:35.683' AS DateTime), CAST(3500000.00 AS Decimal(14, 2)), N'Nhập hàng đợt 2')
INSERT [dbo].[import_receipts] ([id], [supplier_id], [employee_id], [receipt_date], [total_amount], [note]) VALUES (3, 3, 1, CAST(N'2026-09-17T21:27:35.683' AS DateTime), CAST(2200000.00 AS Decimal(14, 2)), N'Nhập hàng đợt 3')
INSERT [dbo].[import_receipts] ([id], [supplier_id], [employee_id], [receipt_date], [total_amount], [note]) VALUES (4, 4, 2, CAST(N'2026-09-17T21:27:35.683' AS DateTime), CAST(1800000.00 AS Decimal(14, 2)), N'Nhập hàng đợt 4')
INSERT [dbo].[import_receipts] ([id], [supplier_id], [employee_id], [receipt_date], [total_amount], [note]) VALUES (5, 5, 5, CAST(N'2026-09-17T21:27:35.683' AS DateTime), CAST(2750000.00 AS Decimal(14, 2)), N'Nhập hàng đợt 5')
SET IDENTITY_INSERT [dbo].[import_receipts] OFF
GO
SET IDENTITY_INSERT [dbo].[invoices] ON 

INSERT [dbo].[invoices] ([id], [order_id], [invoice_date], [total_amount], [tax_amount], [issued_by]) VALUES (1, 1, CAST(N'2026-09-17T21:27:35.760' AS DateTime), CAST(240000.00 AS Decimal(14, 2)), CAST(24000.00 AS Decimal(14, 2)), 1)
INSERT [dbo].[invoices] ([id], [order_id], [invoice_date], [total_amount], [tax_amount], [issued_by]) VALUES (2, 2, CAST(N'2026-09-17T21:27:35.760' AS DateTime), CAST(120000.00 AS Decimal(14, 2)), CAST(12000.00 AS Decimal(14, 2)), 2)
INSERT [dbo].[invoices] ([id], [order_id], [invoice_date], [total_amount], [tax_amount], [issued_by]) VALUES (3, 3, CAST(N'2026-09-17T21:27:35.760' AS DateTime), CAST(90000.00 AS Decimal(14, 2)), CAST(9000.00 AS Decimal(14, 2)), 1)
INSERT [dbo].[invoices] ([id], [order_id], [invoice_date], [total_amount], [tax_amount], [issued_by]) VALUES (4, 4, CAST(N'2026-09-17T21:27:35.760' AS DateTime), CAST(200000.00 AS Decimal(14, 2)), CAST(20000.00 AS Decimal(14, 2)), 5)
INSERT [dbo].[invoices] ([id], [order_id], [invoice_date], [total_amount], [tax_amount], [issued_by]) VALUES (5, 5, CAST(N'2026-09-17T21:27:35.760' AS DateTime), CAST(55000.00 AS Decimal(14, 2)), CAST(5500.00 AS Decimal(14, 2)), 2)
SET IDENTITY_INSERT [dbo].[invoices] OFF
GO
SET IDENTITY_INSERT [dbo].[medical_prescriptions] ON 

INSERT [dbo].[medical_prescriptions] ([id], [record_id], [medication_name], [dosage], [frequency], [duration_days]) VALUES (1, 1, N'Vitamin tổng hợp', N'1 viên', N'Mỗi ngày 1 lần', 5)
INSERT [dbo].[medical_prescriptions] ([id], [record_id], [medication_name], [dosage], [frequency], [duration_days]) VALUES (2, 2, N'Vắc-xin hỗ trợ', N'1 liều', N'Theo chỉ định', 1)
INSERT [dbo].[medical_prescriptions] ([id], [record_id], [medication_name], [dosage], [frequency], [duration_days]) VALUES (3, 3, N'Kem bôi da', N'Một lớp mỏng', N'Mỗi ngày 2 lần', 7)
INSERT [dbo].[medical_prescriptions] ([id], [record_id], [medication_name], [dosage], [frequency], [duration_days]) VALUES (4, 4, N'Dung dịch rửa', N'10 ml', N'Mỗi ngày 1 lần', 5)
INSERT [dbo].[medical_prescriptions] ([id], [record_id], [medication_name], [dosage], [frequency], [duration_days]) VALUES (5, 5, N'Vitamin C', N'1 viên', N'Mỗi ngày 1 lần', 7)
SET IDENTITY_INSERT [dbo].[medical_prescriptions] OFF
GO
SET IDENTITY_INSERT [dbo].[notifications] ON 

INSERT [dbo].[notifications] ([id], [user_id], [appointment_id], [message], [type], [is_read], [created_at]) VALUES (1, 1, 1, N'Lịch hẹn của bạn đã được xác nhận', N'appointment', 0, CAST(N'2026-09-17T21:27:35.8466667' AS DateTime2))
INSERT [dbo].[notifications] ([id], [user_id], [appointment_id], [message], [type], [is_read], [created_at]) VALUES (2, 2, 2, N'Bạn có lịch tiêm phòng vào ngày mai', N'appointment', 0, CAST(N'2026-09-17T21:27:35.8466667' AS DateTime2))
INSERT [dbo].[notifications] ([id], [user_id], [appointment_id], [message], [type], [is_read], [created_at]) VALUES (3, 3, NULL, N'Ưu đãi tháng này giảm 10 phần trăm', N'promotion', 1, CAST(N'2026-09-17T21:27:35.8466667' AS DateTime2))
INSERT [dbo].[notifications] ([id], [user_id], [appointment_id], [message], [type], [is_read], [created_at]) VALUES (4, 4, NULL, N'Hệ thống đã cập nhật', N'system', 1, CAST(N'2026-09-17T21:27:35.8466667' AS DateTime2))
INSERT [dbo].[notifications] ([id], [user_id], [appointment_id], [message], [type], [is_read], [created_at]) VALUES (5, 5, 5, N'Lịch tái khám đang chờ xác nhận', N'appointment', 0, CAST(N'2026-09-17T21:27:35.8466667' AS DateTime2))
SET IDENTITY_INSERT [dbo].[notifications] OFF
GO
SET IDENTITY_INSERT [dbo].[order_items] ON 

INSERT [dbo].[order_items] ([id], [order_id], [product_id], [quantity], [price], [subtotal]) VALUES (1, 1, 1, 2, CAST(120000.00 AS Decimal(12, 2)), CAST(240000.00 AS Decimal(14, 2)))
INSERT [dbo].[order_items] ([id], [order_id], [product_id], [quantity], [price], [subtotal]) VALUES (2, 2, 1, 1, CAST(120000.00 AS Decimal(12, 2)), CAST(120000.00 AS Decimal(14, 2)))
INSERT [dbo].[order_items] ([id], [order_id], [product_id], [quantity], [price], [subtotal]) VALUES (3, 3, 3, 1, CAST(90000.00 AS Decimal(12, 2)), CAST(90000.00 AS Decimal(14, 2)))
INSERT [dbo].[order_items] ([id], [order_id], [product_id], [quantity], [price], [subtotal]) VALUES (4, 4, 4, 2, CAST(100000.00 AS Decimal(12, 2)), CAST(200000.00 AS Decimal(14, 2)))
INSERT [dbo].[order_items] ([id], [order_id], [product_id], [quantity], [price], [subtotal]) VALUES (5, 5, 5, 1, CAST(55000.00 AS Decimal(12, 2)), CAST(55000.00 AS Decimal(14, 2)))
SET IDENTITY_INSERT [dbo].[order_items] OFF
GO
SET IDENTITY_INSERT [dbo].[orders] ON 

INSERT [dbo].[orders] ([id], [user_id], [order_date], [shipping_address], [phone], [total_amount], [status], [note]) VALUES (1, 1, CAST(N'2026-09-17T21:27:35.723' AS DateTime), N'1 Nguyễn Huệ, TP HCM', N'0900000001', CAST(240000.00 AS Decimal(14, 2)), N'completed', N'Đã giao')
INSERT [dbo].[orders] ([id], [user_id], [order_date], [shipping_address], [phone], [total_amount], [status], [note]) VALUES (2, 2, CAST(N'2026-09-17T21:27:35.723' AS DateTime), N'2 Lê Lợi, TP HCM', N'0900000002', CAST(120000.00 AS Decimal(14, 2)), N'confirmed', N'Chờ xác nhận')
INSERT [dbo].[orders] ([id], [user_id], [order_date], [shipping_address], [phone], [total_amount], [status], [note]) VALUES (3, 3, CAST(N'2026-09-17T21:27:35.723' AS DateTime), N'3 Cách Mạng Tháng Tám, TP HCM', N'0900000003', CAST(90000.00 AS Decimal(14, 2)), N'shipping', N'Đang giao')
INSERT [dbo].[orders] ([id], [user_id], [order_date], [shipping_address], [phone], [total_amount], [status], [note]) VALUES (4, 4, CAST(N'2026-09-17T21:27:35.723' AS DateTime), N'4 Phạm Văn Đồng, TP HCM', N'0900000004', CAST(200000.00 AS Decimal(14, 2)), N'pending', N'Mới tạo')
INSERT [dbo].[orders] ([id], [user_id], [order_date], [shipping_address], [phone], [total_amount], [status], [note]) VALUES (5, 5, CAST(N'2026-09-17T21:27:35.723' AS DateTime), N'5 Võ Văn Ngân, TP HCM', N'0900000005', CAST(55000.00 AS Decimal(14, 2)), N'cancelled', N'Khách hủy')
SET IDENTITY_INSERT [dbo].[orders] OFF
GO
SET IDENTITY_INSERT [dbo].[payments] ON 

INSERT [dbo].[payments] ([id], [order_id], [payment_method], [amount], [payment_date], [status], [transaction_code]) VALUES (1, 1, N'cash', CAST(240000.00 AS Decimal(14, 2)), CAST(N'2026-09-17T21:27:35.750' AS DateTime), N'completed', N'TXN-0001')
INSERT [dbo].[payments] ([id], [order_id], [payment_method], [amount], [payment_date], [status], [transaction_code]) VALUES (2, 2, N'bank_transfer', CAST(120000.00 AS Decimal(14, 2)), CAST(N'2026-09-17T21:27:35.750' AS DateTime), N'completed', N'TXN-0002')
INSERT [dbo].[payments] ([id], [order_id], [payment_method], [amount], [payment_date], [status], [transaction_code]) VALUES (3, 3, N'e_wallet', CAST(90000.00 AS Decimal(14, 2)), CAST(N'2026-09-17T21:27:35.750' AS DateTime), N'completed', N'TXN-0003')
INSERT [dbo].[payments] ([id], [order_id], [payment_method], [amount], [payment_date], [status], [transaction_code]) VALUES (4, 4, N'credit_card', CAST(200000.00 AS Decimal(14, 2)), CAST(N'2026-09-17T21:27:35.750' AS DateTime), N'pending', N'TXN-0004')
INSERT [dbo].[payments] ([id], [order_id], [payment_method], [amount], [payment_date], [status], [transaction_code]) VALUES (5, 5, N'cash', CAST(55000.00 AS Decimal(14, 2)), CAST(N'2026-09-17T21:27:35.750' AS DateTime), N'refunded', N'TXN-0005')
SET IDENTITY_INSERT [dbo].[payments] OFF
GO
SET IDENTITY_INSERT [dbo].[permissions] ON 

INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (1, N'users.view', N'Xem người dùng', N'Xem danh sách tài khoản', N'Identity', 1, CAST(N'2026-09-17T21:27:35.6068438' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (2, N'users.manage', N'Quản lý người dùng', N'Thêm sửa xóa tài khoản', N'Identity', 1, CAST(N'2026-09-17T21:27:35.6068438' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (3, N'roles.manage', N'Quản lý vai trò', N'Thêm sửa xóa vai trò', N'Identity', 1, CAST(N'2026-09-17T21:27:35.6068438' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (4, N'appointments.manage', N'Quản lý lịch hẹn', N'Thêm sửa xóa lịch hẹn', N'Service', 1, CAST(N'2026-09-17T21:27:35.6068438' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (5, N'orders.manage', N'Quản lý đơn hàng', N'Xử lý đơn hàng', N'Commerce', 1, CAST(N'2026-09-17T21:27:35.6068438' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (6, N'users.create', N'Thêm Quản lý người dùng', N'Tự tạo quyền create cho Quản lý người dùng', N'users', 1, CAST(N'2026-09-17T14:32:38.2298840' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (7, N'users.update', N'Sửa Quản lý người dùng', N'Tự tạo quyền update cho Quản lý người dùng', N'users', 1, CAST(N'2026-09-17T14:32:38.2442533' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (8, N'users.delete', N'Xóa Quản lý người dùng', N'Tự tạo quyền delete cho Quản lý người dùng', N'users', 1, CAST(N'2026-09-17T14:32:38.2485314' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (9, N'roles.view', N'Xem Quản lý vai trò', N'Tự tạo quyền view cho Quản lý vai trò', N'roles', 1, CAST(N'2026-09-17T14:32:38.2508561' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (10, N'roles.create', N'Thêm Quản lý vai trò', N'Tự tạo quyền create cho Quản lý vai trò', N'roles', 1, CAST(N'2026-09-17T14:32:38.2533070' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (11, N'roles.update', N'Sửa Quản lý vai trò', N'Tự tạo quyền update cho Quản lý vai trò', N'roles', 1, CAST(N'2026-09-17T14:32:38.2569320' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (12, N'roles.delete', N'Xóa Quản lý vai trò', N'Tự tạo quyền delete cho Quản lý vai trò', N'roles', 1, CAST(N'2026-09-17T14:32:38.2588603' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (13, N'permissions.view', N'Xem Phân quyền', N'Tự tạo quyền view cho Phân quyền', N'permissions', 1, CAST(N'2026-09-17T14:32:38.2618288' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (14, N'permissions.create', N'Thêm Phân quyền', N'Tự tạo quyền create cho Phân quyền', N'permissions', 1, CAST(N'2026-09-17T14:32:38.2647459' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (15, N'permissions.update', N'Sửa Phân quyền', N'Tự tạo quyền update cho Phân quyền', N'permissions', 1, CAST(N'2026-09-17T14:32:38.2671024' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (16, N'permissions.delete', N'Xóa Phân quyền', N'Tự tạo quyền delete cho Phân quyền', N'permissions', 1, CAST(N'2026-09-17T14:32:38.2706459' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (17, N'staff.view', N'Xem Quản lý nhân viên', N'Tự tạo quyền view cho Quản lý nhân viên', N'staff', 1, CAST(N'2026-09-17T14:32:38.2728652' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (18, N'staff.create', N'Thêm Quản lý nhân viên', N'Tự tạo quyền create cho Quản lý nhân viên', N'staff', 1, CAST(N'2026-09-17T14:32:38.2768645' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (19, N'staff.update', N'Sửa Quản lý nhân viên', N'Tự tạo quyền update cho Quản lý nhân viên', N'staff', 1, CAST(N'2026-09-17T14:32:38.2795112' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (20, N'staff.delete', N'Xóa Quản lý nhân viên', N'Tự tạo quyền delete cho Quản lý nhân viên', N'staff', 1, CAST(N'2026-09-17T14:32:38.2838375' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (21, N'veterinarians.view', N'Xem Quản lý bác sĩ thú y', N'Tự tạo quyền view cho Quản lý bác sĩ thú y', N'veterinarians', 1, CAST(N'2026-09-17T14:32:38.2862431' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [d
escription], [module], [is_active], [created_at], [updated_at]) VALUES (22, N'veterinarians.create', N'Thêm Quản lý bác sĩ thú y', N'Tự tạo quyền create cho Quản lý bác sĩ thú y', N'veterinarians', 1, CAST(N'2026-09-17T14:32:38.2886443' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (23, N'veterinarians.update', N'Sửa Quản lý bác sĩ thú y', N'Tự tạo quyền update cho Quản lý bác sĩ thú y', N'veterinarians', 1, CAST(N'2026-09-17T14:32:38.2921586' AS DateTime2), NULL)
INSERT [dbo].[permissions] ([id], [code], [name], [description], [module], [is_active], [created_at], [updated_at]) VALUES (24, N'veterinarians.delete', N'Xóa Quản lý bác sĩ thú y', N'Tự tạo quyền delete cho Quản lý bác sĩ thú y', N'veterinarians', 1, CAST(N'2026-09-17T14:32:38.2949761' AS DateTime2), NULL)
SET IDENTITY_INSERT [dbo].[permissions] OFF
GO
SET IDENTITY_INSERT [dbo].[pet_health_records] ON 

INSERT [dbo].[pet_health_records] ([id], [pet_id], [appointment_id], [vet_id], [weight_kg], [temperature], [diagnosis], [treatment], [visit_date]) VALUES (1, 1, 1, 1, CAST(6.20 AS Decimal(5, 2)), CAST(38.5 AS Decimal(4, 1)), N'Sức khỏe bình thường', N'Tiếp tục theo dõi', CAST(N'2026-09-17' AS Date))
INSERT [dbo].[pet_health_records] ([id], [pet_id], [appointment_id], [vet_id], [weight_kg], [temperature], [diagnosis], [treatment], [visit_date]) VALUES (2, 2, 2, 2, CAST(3.80 AS Decimal(5, 2)), CAST(38.2 AS Decimal(4, 1)), N'Cần tiêm phòng', N'Tiêm vắc-xin', CAST(N'2026-09-17' AS Date))
INSERT [dbo].[pet_health_records] ([id], [pet_id], [appointment_id], [vet_id], [weight_kg], [temperature], [diagnosis], [treatment], [visit_date]) VALUES (3, 3, 3, 3, CAST(22.50 AS Decimal(5, 2)), CAST(39.0 AS Decimal(4, 1)), N'Viêm da nhẹ', N'Vệ sinh và bôi thuốc', CAST(N'2026-09-17' AS Date))
INSERT [dbo].[pet_health_records] ([id], [pet_id], [appointment_id], [vet_id], [weight_kg], [temperature], [diagnosis], [treatment], [visit_date]) VALUES (4, 4, 4, 4, CAST(4.10 AS Decimal(5, 2)), CAST(38.4 AS Decimal(4, 1)), N'Không bất thường', N'Tư vấn chăm sóc', CAST(N'2026-09-17' AS Date))
INSERT [dbo].[pet_health_records] ([id], [pet_id], [appointment_id], [vet_id], [weight_kg], [temperature], [diagnosis], [treatment], [visit_date]) VALUES (5, 5, 5, 5, CAST(2.30 AS Decimal(5, 2)), CAST(38.7 AS Decimal(4, 1)), N'Cần tái khám', N'Tái khám sau 7 ngày', CAST(N'2026-09-17' AS Date))
SET IDENTITY_INSERT [dbo].[pet_health_records] OFF
GO
SET IDENTITY_INSERT [dbo].[pet_images] ON 

INSERT [dbo].[pet_images] ([id], [pet_id], [image_url], [is_avatar], [uploaded_at]) VALUES (1, 1, N'pet-01.jpg', 1, CAST(N'2026-09-17T21:27:35.893' AS DateTime))
INSERT [dbo].[pet_images] ([id], [pet_id], [image_url], [is_avatar], [uploaded_at]) VALUES (2, 2, N'pet-02.jpg', 1, CAST(N'2026-09-17T21:27:35.893' AS DateTime))
INSERT [dbo].[pet_images] ([id], [pet_id], [image_url], [is_avatar], [uploaded_at]) VALUES (3, 3, N'pet-03.jpg', 1, CAST(N'2026-09-17T21:27:35.893' AS DateTime))
INSERT [dbo].[pet_images] ([id], [pet_id], [image_url], [is_avatar], [uploaded_at]) VALUES (4, 4, N'pet-04.jpg', 1, CAST(N'2026-09-17T21:27:35.893' AS DateTime))
INSERT [dbo].[pet_images] ([id], [pet_id], [image_url], [is_avatar], [uploaded_at]) VALUES (5, 5, N'pet-05.jpg', 1, CAST(N'2026-09-17T21:27:35.893' AS DateTime))
SET IDENTITY_INSERT [dbo].[pet_images] OFF
GO
SET IDENTITY_INSERT [dbo].[pet_packages] ON 

INSERT [dbo].[pet_packages] ([id], [pet_id], [package_id], [start_date], [end_date], [status]) VALUES (1, 1, 1, CAST(N'2026-09-01' AS Date), CAST(N'2026-10-01' AS Date), 1)
INSERT [dbo].[pet_packages] ([id], [pet_id], [package_id], [start_date], [end_date], [status]) VALUES (2, 2, 2, CAST(N'2026-09-02' AS Date), CAST(N'2026-12-01' AS Date), 1)
INSERT [dbo].[pet_packages] ([id], [pet_id], [package_id], [start_date], [end_date], [status]) VALUES (3, 3, 3, CAST(N'2026-08-01' AS Date), CAST(N'2026-09-01' AS Date), 2)
INSERT [dbo].[pet_packages] ([id], [pet_id], [package_id], [start_date], [end_date], [status]) VALUES (4, 4, 4, CAST(N'2026-09-04' AS Date), CAST(N'2026-11-03' AS Date), 1)
INSERT [dbo].[pet_packages] ([id], [pet_id], [package_id], [start_date], [end_date], [status]) VALUES (5, 5, 5, CAST(N'2026-07-01' AS Date), CAST(N'2026-12-28' AS Date), 3)
SET IDENTITY_INSERT [dbo].[pet_packages] OFF
GO
SET IDENTITY_INSERT [dbo].[pets] ON 

INSERT [dbo].[pets] ([id], [user_id], [qr_token], [name], [species], [breed], [gender], [weight_kg], [created_at], [updated_at]) VALUES (1, 1, N'PET-QR-0001', N'Milu', N'Chó', N'Poodle', N'Male', CAST(6.20 AS Decimal(4, 2)), CAST(N'2026-09-17T21:27:35.7666667' AS DateTime2), NULL)
INSERT [dbo].[pets] ([id], [user_id], [qr_token], [name], [species], [breed], [gender], [weight_kg], [created_at], [updated_at]) VALUES (2, 2, N'PET-QR-0002', N'Lucky', N'Mèo', N'Anh lông ngắn', N'Female', CAST(3.80 AS Decimal(4, 2)), CAST(N'2026-09-17T21:27:35.7666667' AS DateTime2), NULL)
INSERT [dbo].[pets] ([id], [user_id], [qr_token], [name], [species], [breed], [gender], [weight_kg], [created_at], [updated_at]) VALUES (3, 3, N'PET-QR-0003', N'Bông', N'Chó', N'Golden Retriever', N'Male', CAST(22.50 AS Decimal(4, 2)), CAST(N'2026-09-17T21:27:35.7666667' AS DateTime2), NULL)
INSERT [dbo].[pets] ([id], [user_id], [qr_token], [name], [species], [breed], [gender], [weight_kg], [created_at], [updated_at]) VALUES (4, 4, N'PET-QR-0004', N'Miu', N'Mèo', N'Munchkin', N'Female', CAST(4.10 AS Decimal(4, 2)), CAST(N'2026-09-17T21:27:35.7666667' AS DateTime2), NULL)
INSERT [dbo].[pets] ([id], [user_id], [qr_token], [name], [species], [breed], [gender], [weight_kg], [created_at], [updated_at]) VALUES (5, 5, N'PET-QR-0005', N'Kem', N'Thỏ', N'Thỏ trắng', N'Female', CAST(2.30 AS Decimal(4, 2)), CAST(N'2026-09-17T21:27:35.7666667' AS DateTime2), NULL)
SET IDENTITY_INSERT [dbo].[pets] OFF
GO
SET IDENTITY_INSERT [dbo].[products] ON 

INSERT [dbo].[products] ([id], [category_id], [product_name], [description], [unit], [import_price], [sell_price], [stock_quantity], [image_url], [status], [created_at], [updated_at]) VALUES (1, 1, N'Hạt cho chó', N'Thức ăn khô', N'kg', CAST(80000.00 AS Decimal(12, 2)), CAST(120000.00 AS Decimal(12, 2)), 50, N'food-dog.jpg', N'active', CAST(N'2026-09-17T21:27:35.673' AS DateTime), NULL)
INSERT [dbo].[products] ([id], [category_id], [product_name], [description], [unit], [import_price], [sell_price], [stock_quantity], [image_url], [status], [created_at], [updated_at]) VALUES (2, 1, N'Pate cho mèo', N'Pate đóng hộp', N'hộp', CAST(25000.00 AS Decimal(12, 2)), CAST(40000.00 AS Decimal(12, 2)), 80, N'food-cat.jpg', N'active', CAST(N'2026-09-17T21:27:35.673' AS DateTime), NULL)
INSERT [dbo].[products] ([id], [category_id], [product_name], [description], [unit], [import_price], [sell_price], [stock_quantity], [image_url], [status], [created_at], [updated_at]) VALUES (3, 2, N'Vòng cổ thú cưng', N'Vòng cổ da', N'cái', CAST(50000.00 AS Decimal(12, 2)), CAST(90000.00 AS Decimal(12, 2)), 30, N'collar.jpg', N'active', CAST(N'2026-09-17T21:27:35.673' AS DateTime), NULL)
INSERT [dbo].[products] ([id], [category_id], [product_name], [description], [unit], [import_price], [sell_price], [stock_quantity], [image_url], [status], [created_at], [updated_at]) VALUES (4, 3, N'Dung dịch vệ sinh', N'Dung dịch vệ sinh tai', N'chai', CAST(60000.00 AS Decimal(12, 2)), CAST(100000.00 AS Decimal(12, 2)), 25, N'cleaner.jpg', N'active', CAST(N'2026-09-17T21:27:35.673' AS DateTime), NULL)
INSERT [dbo].[products] ([id], [category_id], [product_name], [description], [unit], [import_price], [sell_price], [stock_quantity], [image_url], [status], [created_at], [updated_at]) VALUES (5, 5, N'Bóng cao su', N'Đồ chơi vận động', N'cái', CAST(30000.00 AS Decimal(12, 2)), CAST(55000.00 AS Decimal(12, 2)), 40, N'ball.jpg', N'active', CAST(N'2026-09-17T21:27:35.673' AS DateTime), NULL)
SET IDENTITY_INSERT [dbo].[products] OFF
GO
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 6, CAST(N'2026-09-17T15:01:42.1884262' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 7, CAST(N'2026-09-17T15:01:42.1884263' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 8, CAST(N'2026-09-17T15:01:42.1884265' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 9, CAST(N'2026-09-17T15:01:42.1884085' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 10, CAST(N'2026-09-17T15:01:42.1884086' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 11, CAST(N'2026-09-17T15:01:42.1884087' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 12, CAST(N'2026-09-17T15:01:42.1884107' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 13, CAST(N'2026-09-17T15:01:42.1884074' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 14, CAST(N'2026-09-17T15:01:42.1884080' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 15, CAST(N'2026-09-17T15:01:42.1884082' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 16, CAST(N'2026-09-17T15:01:42.1884083' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 17, CAST(N'2026-09-17T15:01:42.1884109' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 18, CAST(N'2026-09-17T15:01:42.1884257' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 19, CAST(N'2026-09-17T15:01:42.1884259' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 20, CAST(N'2026-09-17T15:01:42.1884260' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 21, CAST(N'2026-09-17T15:01:42.1884266' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 22, CAST(N'2026-09-17T15:01:42.1884268' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 23, CAST(N'2026-09-17T15:01:42.1884275' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (1, 24, CAST(N'2026-09-17T15:01:42.1884276' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (2, 4, CAST(N'2026-09-17T21:27:35.6172212' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (5, 5, CAST(N'2026-09-17T21:27:35.6172212' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 6, CAST(N'2026-09-17T15:01:24.8841399' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 7, CAST(N'2026-09-17T15:01:24.8841400' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 8, CAST(N'2026-09-17T15:01:24.8841402' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 9, CAST(N'2026-09-17T15:01:24.8840222' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 10, CAST(N'2026-09-17T15:01:24.8840224' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 11, CAST(N'2026-09-17T15:01:24.8840225' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 12, CAST(N'2026-09-17T15:01:24.8840341' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 13, CAST(N'2026-09-17T15:01:24.8839521' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 14, CAST(N'2026-09-17T15:01:24.8840213' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 15, CAST(N'2026-09-17T15:01:24.8840219' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 16, CAST(N'2026-09-17T15:01:24.8840221' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 17, CAST(N'2026-09-17T15:01:24.8840345' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 18, CAST(N'2026-09-17T15:01:24.8841393' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 19, CAST(N'2026-09-17T15:01:24.8841397' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 20, CAST(N'2026-09-17T15:01:24.8841398' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 21, CAST(N'2026-09-17T15:01:24.8841403' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 22, CAST(N'2026-09-17T15:01:24.8841404' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 23, CAST(N'2026-09-17T15:01:24.8841431' AS DateTime2))
INSERT [dbo].[role_permissions] ([role_id], [permission_id], [created_at]) VALUES (6, 24, CAST(N'2026-09-17T15:01:24.8841434' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[roles] ON 

INSERT [dbo].[roles] ([id], [name], [description], [status], [created_at], [updated_at]) VALUES (1, N'admin', N'Quản trị hệ thống', N'active', CAST(N'2026-09-17T21:27:35.5974589' AS DateTime2), CAST(N'2026-09-17T15:01:42.1805071' AS DateTime2))
INSERT [dbo].[roles] ([id], [name], [description], [status], [created_at], [updated_at]) VALUES (2, N'staff', N'Nhân viên', N'active', CAST(N'2026-09-17T21:27:35.5974589' AS DateTime2), NULL)
INSERT [dbo].[roles] ([id], [name], [description], [status], [created_at], [updated_at]) VALUES (3, N'member', N'Khách hàng', N'active', CAST(N'2026-09-17T21:27:35.5974589' AS DateTime2), NULL)
INSERT [dbo].[roles] ([id], [name], [description], [status], [created_at], [updated_at]) VALUES (4, N'receptionist', N'Lễ tân', N'active', CAST(N'2026-09-17T21:27:35.5974589' AS DateTime2), NULL)
INSERT [dbo].[roles] ([id], [name], [description], [status], [created_at], [updated_at]) VALUES (5, N'manager', N'Quản lý', N'active', CAST(N'2026-09-17T21:27:35.5974589' AS DateTime2), NULL)
INSERT [dbo].[roles] ([id], [name], [description], [status], [created_at], [updated_at]) VALUES (6, N'nguyenthiY', N'aaaaaaaaaaaaaaaaaaaaaaa', N'active', CAST(N'2026-09-17T14:39:06.4179488' AS DateTime2), CAST(N'2026-09-17T15:01:24.8437750' AS DateTime2))
SET IDENTITY_INSERT [dbo].[roles] OFF
GO
SET IDENTITY_INSERT [dbo].[service_packages] ON 

INSERT [dbo].[service_packages] ([id], [name], [description], [price], [duration_minutes], [status], [created_at], [validity_days]) VALUES (1, N'Gói cơ bản', N'Gói chăm sóc cơ bản', CAST(500000.00 AS Decimal(10, 2)), 90, 1, CAST(N'2026-09-17T21:27:35.653' AS DateTime), 30)
INSERT [dbo].[service_packages] ([id], [name], [description], [price], [duration_minutes], [status], [created_at], [validity_days]) VALUES (2, N'Gói tiêm phòng', N'Gói tiêm phòng định kỳ', CAST(700000.00 AS Decimal(10, 2)), 60, 1, CAST(N'2026-09-17T21:27:35.653' AS DateTime), 90)
INSERT [dbo].[service_packages] ([id], [name], [description], [price], [duration_minutes], [status], [created_at], [validity_days]) VALUES (3, N'Gói làm đẹp', N'Gói tắm và cắt tỉa', CAST(800000.00 AS Decimal(10, 2)), 150, 1, CAST(N'2026-09-17T21:27:35.653' AS DateTime), 30)
INSERT [dbo].[service_packages] ([id], [name], [description], [price], [duration_minutes], [status], [created_at], [validity_days]) VALUES (4, N'Gói sức khỏe', N'Gói khám và siêu âm', CAST(1000000.00 AS Decimal(10, 2)), 120, 1, CAST(N'2026-09-17T21:27:35.653' AS DateTime), 60)
INSERT [dbo].[service_packages] ([id], [name], [description], [price], [duration_minutes], [status], [created_at], [validity_days]) VALUES (5, N'Gói cao cấp', N'Gói chăm sóc toàn diện', CAST(1500000.00 AS Decimal(10, 2)), 240, 1, CAST(N'2026-09-17T21:27:35.653' AS DateTime), 180)
SET IDENTITY_INSERT [dbo].[service_packages] OFF
GO
SET IDENTITY_INSERT [dbo].[services] ON 

INSERT [dbo].[services] ([id], [name], [description], [price], [duration_minutes], [image_url], [status], [created_at]) VALUES (1, N'Khám tổng quát', N'Khám sức khỏe cơ bản', CAST(150000.00 AS Decimal(10, 2)), 30, N'service-general.jpg', 1, CAST(N'2026-09-17T21:27:35.647' AS DateTime))
INSERT [dbo].[services] ([id], [name], [description], [price], [duration_minutes], [image_url], [status], [created_at]) VALUES (2, N'Tiêm phòng', N'Tiêm phòng định kỳ', CAST(200000.00 AS Decimal(10, 2)), 20, N'service-vaccine.jpg', 1, CAST(N'2026-09-17T21:27:35.647' AS DateTime))
INSERT [dbo].[services] ([id], [name], [description], [price], [duration_minutes], [image_url], [status], [created_at]) VALUES (3, N'Tắm rửa', N'Vệ sinh và tắm rửa thú cưng', CAST(120000.00 AS Decimal(10, 2)), 45, N'service-grooming.jpg', 1, CAST(N'2026-09-17T21:27:35.647' AS DateTime))
INSERT [dbo].[services] ([id], [name], [description], [price], [duration_minutes], [image_url], [status], [created_at]) VALUES (4, N'Cắt tỉa lông', N'Cắt tỉa lông cơ bản', CAST(180000.00 AS Decimal(10, 2)), 60, N'service-trim.jpg', 1, CAST(N'2026-09-17T21:27:35.647' AS DateTime))
INSERT [dbo].[services] ([id], [name], [description], [price], [duration_minutes], [image_url], [status], [created_at]) VALUES (5, N'Siêu âm', N'Siêu âm chẩn đoán', CAST(350000.00 AS Decimal(10, 2)), 40, N'service-ultrasound.jpg', 1, CAST(N'2026-09-17T21:27:35.647' AS DateTime))
SET IDENTITY_INSERT [dbo].[services] OFF
GO
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (1, 1)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (1, 3)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (1, 4)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (2, 2)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (2, 5)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (3, 3)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (3, 4)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (4, 1)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (4, 2)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (4, 5)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (5, 1)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (5, 2)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (5, 3)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (5, 4)
INSERT [dbo].[service_package_services] ([package_id], [service_id]) VALUES (5, 5)
GO
SET IDENTITY_INSERT [dbo].[staff] ON 

INSERT [dbo].[staff] ([id], [user_id], [position], [hire_date], [status], [created_at], [updated_at]) VALUES (1, 2, N'Nhân viên tiếp nhận', CAST(N'2024-01-15' AS Date), N'active', CAST(N'2026-09-17T21:27:35.6362301' AS DateTime2), NULL)
INSERT [dbo].[staff] ([id], [user_id], [position], [hire_date], [status], [created_at], [updated_at]) VALUES (2, 4, N'Lễ tân', CAST(N'2024-03-01' AS Date), N'active', CAST(N'2026-09-17T21:27:35.6362301' AS DateTime2), NULL)
INSERT [dbo].[staff] ([id], [user_id], [position], [hire_date], [status], [created_at], [updated_at]) VALUES (3, 5, N'Quản lý', CAST(N'2023-06-10' AS Date), N'active', CAST(N'2026-09-17T21:27:35.6362301' AS DateTime2), NULL)
INSERT [dbo].[staff] ([id], [user_id], [position], [hire_date], [status], [created_at], [updated_at]) VALUES (4, 6, N'Thu ngân', CAST(N'2025-02-20' AS Date), N'active', CAST(N'2026-09-17T21:27:35.6362301' AS DateTime2), NULL)
INSERT [dbo].[staff] ([id], [user_id], [position], [hire_date], [status], [created_at], [updated_at]) VALUES (5, 7, N'Nhân viên chăm sóc khách hàng', CAST(N'2025-05-12' AS Date), N'active', CAST(N'2026-09-17T21:27:35.6362301' AS DateTime2), NULL)
SET IDENTITY_INSERT [dbo].[staff] OFF
GO
SET IDENTITY_INSERT [dbo].[suppliers] ON 

INSERT [dbo].[suppliers] ([id], [supplier_name], [phone], [email], [address], [created_at]) VALUES (1, N'Nhà cung cấp A', N'0911000001', N'supplier1@example.com', N'TP HCM', CAST(N'2026-09-17T21:27:35.663' AS DateTime))
INSERT [dbo].[suppliers] ([id], [supplier_name], [phone], [email], [address], [created_at]) VALUES (2, N'Nhà cung cấp B', N'0911000002', N'supplier2@example.com', N'Bình Dương', CAST(N'2026-09-17T21:27:35.663' AS DateTime))
INSERT [dbo].[suppliers] ([id], [supplier_name], [phone], [email], [address], [created_at]) VALUES (3, N'Nhà cung cấp C', N'0911000003', N'supplier3@example.com', N'Đồng Nai', CAST(N'2026-09-17T21:27:35.663' AS DateTime))
INSERT [dbo].[suppliers] ([id], [supplier_name], [phone], [email], [address], [created_at]) VALUES (4, N'Nhà cung cấp D', N'0911000004', N'supplier4@example.com', N'Long An', CAST(N'2026-09-17T21:27:35.663' AS DateTime))
INSERT [dbo].[suppliers] ([id], [supplier_name], [phone], [email], [address], [created_at]) VALUES (5, N'Nhà cung cấp E', N'0911000005', N'supplier5@example.com', N'Tây Ninh', CAST(N'2026-09-17T21:27:35.663' AS DateTime))
SET IDENTITY_INSERT [dbo].[suppliers] OFF
GO
SET IDENTITY_INSERT [dbo].[users] ON 

INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (1, 1, N'admin01', N'AQAAAAIAAYagAAAAEC5UvaIeISzPf+iRab/hyVbrKuKJkpiqbXX7t1e9r8uYlM7CFgxhahkf08x/Nucixg==', N'Nguyễn Tường Huy', N'an@example.com', N'0900000001', N'TP HCM', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), CAST(N'2026-09-17T14:28:23.0197167' AS DateTime2))
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (2, 2, N'staff01', N'demo_hash_02', N'Trần Thị Bình', N'binh@example.com', N'0900000002', N'TP HCM', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (3, 3, N'member01', N'demo_hash_03', N'Lê Văn Cường', N'cuong@example.com', N'0900000003', N'Bình Dương', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (4, 3, N'member02', N'demo_hash_04', N'Phạm Thị Dung', N'dung@example.com', N'0900000004', N'Đồng Nai', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (5, 5, N'manager01', N'demo_hash_05', N'Hoàng Văn Em', N'em@example.com', N'0900000005', N'Long An', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (6, 2, N'staff02', N'demo_hash_06', N'Võ Thị Hoa', N'hoa@example.com', N'0900000006', N'TP HCM', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (7, 4, N'receptionist01', N'demo_hash_07', N'Nguyễn Thị Lan', N'lan@example.com', N'0900000007', N'TP HCM', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (8, 2, N'vet01', N'demo_hash_08', N'Phạm Văn Minh', N'minh@example.com', N'0900000008', N'TP HCM', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (9, 2, N'vet02', N'demo_hash_09', N'Lê Thị Nga', N'nga@example.com', N'0900000009', N'Bình Dương', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (10, 2, N'vet03', N'demo_hash_10', N'Trần Văn Phúc', N'phuc@example.com', N'0900000010', N'Đồng Nai', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (11, 2, N'vet04', N'demo_hash_11', N'Hoàng Thị Quế', N'que@example.com', N'0900000011', N'Long An', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (12, 2, N'vet05', N'demo_hash_12', N'Đỗ Văn Sơn', N'son@example.com', N'0900000012', N'Tây Ninh', N'active', CAST(N'2026-09-17T21:27:35.6272450' AS DateTime2), NULL)
INSERT [dbo].[users] ([id], [role_id], [username], [password_hash], [full_name], [email], [phone_number], [address], [status], [created_at], [updated_at]) VALUES (13, 1, N'nguyenthiy1', N'AQAAAAIAAYagAAAAEDXu7IoyEcESIZAodteJ9hj7z/uZfv28u931AWskcfJints5xAPnv+27LnhjSSjoNw==', N'Nguyễn Thị Ý', N'akiendeptraiqua@thiy.com', N'0918222234', N'aaaaaaaaaaaaaaaaaaaaaaaaaaa', N'active', CAST(N'2026-09-17T14:32:02.9459672' AS DateTime2), NULL)
SET IDENTITY_INSERT [dbo].[users] OFF
GO
SET IDENTITY_INSERT [dbo].[vaccination_records] ON 

INSERT [dbo].[vaccination_records] ([id], [pet_id], [vaccine_name], [batch_number], [administered_date], [next_due_date], [vet_id]) VALUES (1, 1, N'Vắc-xin 5 bệnh', N'LOT-001', CAST(N'2026-01-10' AS Date), CAST(N'2027-01-10' AS Date), 1)
INSERT [dbo].[vaccination_records] ([id], [pet_id], [vaccine_name], [batch_number], [administered_date], [next_due_date], [vet_id]) VALUES (2, 2, N'Vắc-xin 4 bệnh', N'LOT-002', CAST(N'2026-02-11' AS Date), CAST(N'2027-02-11' AS Date), 2)
INSERT [dbo].[vaccination_records] ([id], [pet_id], [vaccine_name], [batch_number], [administered_date], [next_due_date], [vet_id]) VALUES (3, 3, N'Vắc-xin dại', N'LOT-003', CAST(N'2026-03-12' AS Date), CAST(N'2027-03-12' AS Date), 3)
INSERT [dbo].[vaccination_records] ([id], [pet_id], [vaccine_name], [batch_number], [administered_date], [next_due_date], [vet_id]) VALUES (4, 4, N'Vắc-xin 4 bệnh', N'LOT-004', CAST(N'2026-04-13' AS Date), CAST(N'2027-04-13' AS Date), 4)
INSERT [dbo].[vaccination_records] ([id], [pet_id], [vaccine_name], [batch_number], [administered_date], [next_due_date], [vet_id]) VALUES (5, 5, N'Vắc-xin thỏ', N'LOT-005', CAST(N'2026-05-14' AS Date), CAST(N'2027-05-14' AS Date), 5)
SET IDENTITY_INSERT [dbo].[vaccination_records] OFF
GO
SET IDENTITY_INSERT [dbo].[veterinarians] ON 

INSERT [dbo].[veterinarians] ([id], [user_id], [specialty], [license_no], [years_of_experience], [status], [created_at], [updated_at]) VALUES (1, 8, N'Nội khoa thú cưng', N'VET-0001', 8, N'active', CAST(N'2026-09-17T21:27:35.6422447' AS DateTime2), NULL)
INSERT [dbo].[veterinarians] ([id], [user_id], [specialty], [license_no], [years_of_experience], [status], [created_at], [updated_at]) VALUES (2, 9, N'Ngoại khoa', N'VET-0002', 6, N'active', CAST(N'2026-09-17T21:27:35.6422447' AS DateTime2), NULL)
INSERT [dbo].[veterinarians] ([id], [user_id], [specialty], [license_no], [years_of_experience], [status], [created_at], [updated_at]) VALUES (3, 10, N'Da liễu', N'VET-0003', 5, N'active', CAST(N'2026-09-17T21:27:35.6422447' AS DateTime2), NULL)
INSERT [dbo].[veterinarians] ([id], [user_id], [specialty], [license_no], [years_of_experience], [status], [created_at], [updated_at]) VALUES (4, 11, N'Nhận diện giống', N'VET-0004', 4, N'active', CAST(N'2026-09-17T21:27:35.6422447' AS DateTime2), NULL)
INSERT [dbo].[veterinarians] ([id], [user_id], [specialty], [license_no], [years_of_experience], [status], [created_at], [updated_at]) VALUES (5, 12, N'Tiêm phòng', N'VET-0005', 10, N'active', CAST(N'2026-09-17T21:27:35.6422447' AS DateTime2), NULL)
SET IDENTITY_INSERT [dbo].[veterinarians] OFF
GO
/****** Object:  Index [IX_appointments_date]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_appointments_date] ON [dbo].[appointments]
(
	[appointment_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_appointments_pet_id]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_appointments_pet_id] ON [dbo].[appointments]
(
	[pet_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_appointments_user_id]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_appointments_user_id] ON [dbo].[appointments]
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_BreedPredictionDetails_Breed]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[breed_prediction_details] ADD  CONSTRAINT [UQ_BreedPredictionDetails_Breed] UNIQUE NONCLUSTERED 
(
	[prediction_id] ASC,
	[predicted_breed] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ_BreedPredictionDetails_Rank]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[breed_prediction_details] ADD  CONSTRAINT [UQ_BreedPredictionDetails_Rank] UNIQUE NONCLUSTERED 
(
	[prediction_id] ASC,
	[rank_order] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_BreedPredictionDetails_PredictionId]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_BreedPredictionDetails_PredictionId] ON [dbo].[breed_prediction_details]
(
	[prediction_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_BreedPredictions_UserId]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_BreedPredictions_UserId] ON [dbo].[breed_predictions]
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ__cart_ite__6A850DF9C45C037A]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[cart_items] ADD UNIQUE NONCLUSTERED 
(
	[cart_id] ASC,
	[product_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__categori__5189E25599D99276]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[categories] ADD UNIQUE NONCLUSTERED 
(
	[category_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_chat_conversations_user_id]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_chat_conversations_user_id] ON [dbo].[chat_conversations]
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_feedbacks_user_id]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_feedbacks_user_id] ON [dbo].[feedbacks]
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ__import_d__D5850BC1B329C39E]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[import_details] ADD UNIQUE NONCLUSTERED 
(
	[receipt_id] ASC,
	[product_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ__invoices__465962288FA3F3F0]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[invoices] ADD UNIQUE NONCLUSTERED 
(
	[order_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_medical_prescriptions_record_id]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_medical_prescriptions_record_id] ON [dbo].[medical_prescriptions]
(
	[record_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_notifications_user_id]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_notifications_user_id] ON [dbo].[notifications]
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ__order_it__022945F70F9B4568]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[order_items] ADD UNIQUE NONCLUSTERED 
(
	[order_id] ASC,
	[product_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__payments__DD5740BEC23CB573]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[payments] ADD UNIQUE NONCLUSTERED 
(
	[transaction_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__permissi__357D4CF941E2CFFC]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[permissions] ADD UNIQUE NONCLUSTERED 
(
	[code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_pet_health_records_pet_id]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_pet_health_records_pet_id] ON [dbo].[pet_health_records]
(
	[pet_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PetImages_PetId]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_PetImages_PetId] ON [dbo].[pet_images]
(
	[pet_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_pet_packages_pet_id]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_pet_packages_pet_id] ON [dbo].[pet_packages]
(
	[pet_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__pets__2254D598BAD98A68]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[pets] ADD UNIQUE NONCLUSTERED 
(
	[qr_token] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_pets_user_id]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_pets_user_id] ON [dbo].[pets]
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ__staff__B9BE370E2A93FBB2]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[staff] ADD UNIQUE NONCLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__supplier__AB6E61645AF3786C]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[suppliers] ADD UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__users__AB6E616484B83536]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[users] ADD UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__users__F3DBC572D0AC3FD8]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[users] ADD UNIQUE NONCLUSTERED 
(
	[username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_vaccination_records_pet_id]    Script Date: 9/17/2026 10:26:57 PM ******/
CREATE NONCLUSTERED INDEX [IX_vaccination_records_pet_id] ON [dbo].[vaccination_records]
(
	[pet_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ__veterina__B9BE370E55F2101E]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[veterinarians] ADD UNIQUE NONCLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__veterina__BBBB6DA6808577AA]    Script Date: 9/17/2026 10:26:57 PM ******/
ALTER TABLE [dbo].[veterinarians] ADD UNIQUE NONCLUSTERED 
(
	[license_no] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[appointments] ADD  DEFAULT ((1)) FOR [status]
GO
ALTER TABLE [dbo].[breed_predictions] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[cart_items] ADD  DEFAULT ((1)) FOR [quantity]
GO
ALTER TABLE [dbo].[carts] ADD  DEFAULT ('active') FOR [status]
GO
ALTER TABLE [dbo].[carts] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[categories] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[chat_conversations] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[feedbacks] ADD  DEFAULT ('pending') FOR [status]
GO
ALTER TABLE [dbo].[feedbacks] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[import_receipts] ADD  DEFAULT (getdate()) FOR [receipt_date]
GO
ALTER TABLE [dbo].[import_receipts] ADD  DEFAULT ((0)) FOR [total_amount]
GO
ALTER TABLE [dbo].[invoices] ADD  DEFAULT (getdate()) FOR [invoice_date]
GO
ALTER TABLE [dbo].[invoices] ADD  DEFAULT ((0)) FOR [tax_amount]
GO
ALTER TABLE [dbo].[notifications] ADD  DEFAULT ((0)) FOR [is_read]
GO
ALTER TABLE [dbo].[notifications] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[orders] ADD  DEFAULT (getdate()) FOR [order_date]
GO
ALTER TABLE [dbo].[orders] ADD  DEFAULT ((0)) FOR [total_amount]
GO
ALTER TABLE [dbo].[orders] ADD  DEFAULT ('pending') FOR [status]
GO
ALTER TABLE [dbo].[payments] ADD  DEFAULT (getdate()) FOR [payment_date]
GO
ALTER TABLE [dbo].[payments] ADD  DEFAULT ('pending') FOR [status]
GO
ALTER TABLE [dbo].[permissions] ADD  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [dbo].[permissions] ADD  DEFAULT (sysdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[pet_health_records] ADD  DEFAULT (CONVERT([date],getdate())) FOR [visit_date]
GO
ALTER TABLE [dbo].[pet_images] ADD  DEFAULT ((0)) FOR [is_avatar]
GO
ALTER TABLE [dbo].[pet_images] ADD  DEFAULT (getdate()) FOR [uploaded_at]
GO
ALTER TABLE [dbo].[pet_packages] ADD  DEFAULT ((1)) FOR [status]
GO
ALTER TABLE [dbo].[pets] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[products] ADD  DEFAULT ((0)) FOR [stock_quantity]
GO
ALTER TABLE [dbo].[products] ADD  DEFAULT ('active') FOR [status]
GO
ALTER TABLE [dbo].[products] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[role_permissions] ADD  DEFAULT (sysdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[roles] ADD  DEFAULT (N'active') FOR [status]
GO
ALTER TABLE [dbo].[roles] ADD  DEFAULT (sysdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[service_packages] ADD  DEFAULT ((1)) FOR [status]
GO
ALTER TABLE [dbo].[service_packages] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[services] ADD  DEFAULT ((1)) FOR [status]
GO
ALTER TABLE [dbo].[services] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[staff] ADD  DEFAULT (N'active') FOR [status]
GO
ALTER TABLE [dbo].[staff] ADD  DEFAULT (sysdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[suppliers] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[users] ADD  DEFAULT (N'active') FOR [status]
GO
ALTER TABLE [dbo].[users] ADD  DEFAULT (sysdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[veterinarians] ADD  DEFAULT (N'active') FOR [status]
GO
ALTER TABLE [dbo].[veterinarians] ADD  DEFAULT (sysdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[appointments]  WITH CHECK ADD FOREIGN KEY([pet_id])
REFERENCES [dbo].[pets] ([id])
GO
ALTER TABLE [dbo].[appointments]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
GO
ALTER TABLE [dbo].[appointments]  WITH CHECK ADD FOREIGN KEY([vet_id])
REFERENCES [dbo].[veterinarians] ([id])
GO
ALTER TABLE [dbo].[appointments_services]  WITH CHECK ADD FOREIGN KEY([appointment_id])
REFERENCES [dbo].[appointments] ([id])
GO
ALTER TABLE [dbo].[appointments_services]  WITH CHECK ADD FOREIGN KEY([service_id])
REFERENCES [dbo].[services] ([id])
GO
ALTER TABLE [dbo].[service_package_services]  WITH CHECK ADD FOREIGN KEY([package_id])
REFERENCES [dbo].[service_packages] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[service_package_services]  WITH CHECK ADD FOREIGN KEY([service_id])
REFERENCES [dbo].[services] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[breed_prediction_details]  WITH CHECK ADD  CONSTRAINT [FK_BreedPredictionDetails_Predictions] FOREIGN KEY([prediction_id])
REFERENCES [dbo].[breed_predictions] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[breed_prediction_details] CHECK CONSTRAINT [FK_BreedPredictionDetails_Predictions]
GO
ALTER TABLE [dbo].[breed_predictions]  WITH CHECK ADD  CONSTRAINT [FK_BreedPredictions_Users] FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[breed_predictions] CHECK CONSTRAINT [FK_BreedPredictions_Users]
GO
ALTER TABLE [dbo].[cart_items]  WITH CHECK ADD FOREIGN KEY([cart_id])
REFERENCES [dbo].[carts] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[cart_items]  WITH CHECK ADD FOREIGN KEY([product_id])
REFERENCES [dbo].[products] ([id])
GO
ALTER TABLE [dbo].[carts]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
GO
ALTER TABLE [dbo].[chat_conversations]  WITH CHECK ADD  CONSTRAINT [FK_chat_conversations_users] FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[chat_conversations] CHECK CONSTRAINT [FK_chat_conversations_users]
GO
ALTER TABLE [dbo].[feedbacks]  WITH CHECK ADD  CONSTRAINT [FK_feedbacks_users] FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[feedbacks] CHECK CONSTRAINT [FK_feedbacks_users]
GO
ALTER TABLE [dbo].[import_details]  WITH CHECK ADD FOREIGN KEY([product_id])
REFERENCES [dbo].[products] ([id])
GO
ALTER TABLE [dbo].[import_details]  WITH CHECK ADD FOREIGN KEY([receipt_id])
REFERENCES [dbo].[import_receipts] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[import_receipts]  WITH CHECK ADD FOREIGN KEY([employee_id])
REFERENCES [dbo].[users] ([id])
GO
ALTER TABLE [dbo].[import_receipts]  WITH CHECK ADD FOREIGN KEY([supplier_id])
REFERENCES [dbo].[suppliers] ([id])
GO
ALTER TABLE [dbo].[invoices]  WITH CHECK ADD FOREIGN KEY([issued_by])
REFERENCES [dbo].[users] ([id])
GO
ALTER TABLE [dbo].[invoices]  WITH CHECK ADD FOREIGN KEY([order_id])
REFERENCES [dbo].[orders] ([id])
GO
ALTER TABLE [dbo].[medical_prescriptions]  WITH CHECK ADD FOREIGN KEY([record_id])
REFERENCES [dbo].[pet_health_records] ([id])
GO
ALTER TABLE [dbo].[notifications]  WITH CHECK ADD  CONSTRAINT [FK_notifications_appointments] FOREIGN KEY([appointment_id])
REFERENCES [dbo].[appointments] ([id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[notifications] CHECK CONSTRAINT [FK_notifications_appointments]
GO
ALTER TABLE [dbo].[notifications]  WITH CHECK ADD  CONSTRAINT [FK_notifications_users] FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[notifications] CHECK CONSTRAINT [FK_notifications_users]
GO
ALTER TABLE [dbo].[order_items]  WITH CHECK ADD FOREIGN KEY([order_id])
REFERENCES [dbo].[orders] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[order_items]  WITH CHECK ADD FOREIGN KEY([product_id])
REFERENCES [dbo].[products] ([id])
GO
ALTER TABLE [dbo].[orders]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
GO
ALTER TABLE [dbo].[payments]  WITH CHECK ADD FOREIGN KEY([order_id])
REFERENCES [dbo].[orders] ([id])
GO
ALTER TABLE [dbo].[pet_health_records]  WITH CHECK ADD FOREIGN KEY([appointment_id])
REFERENCES [dbo].[appointments] ([id])
GO
ALTER TABLE [dbo].[pet_health_records]  WITH CHECK ADD FOREIGN KEY([pet_id])
REFERENCES [dbo].[pets] ([id])
GO
ALTER TABLE [dbo].[pet_health_records]  WITH CHECK ADD FOREIGN KEY([vet_id])
REFERENCES [dbo].[veterinarians] ([id])
GO
ALTER TABLE [dbo].[pet_images]  WITH CHECK ADD  CONSTRAINT [FK_PetImages_Pets] FOREIGN KEY([pet_id])
REFERENCES [dbo].[pets] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[pet_images] CHECK CONSTRAINT [FK_PetImages_Pets]
GO
ALTER TABLE [dbo].[pet_packages]  WITH CHECK ADD FOREIGN KEY([package_id])
REFERENCES [dbo].[service_packages] ([id])
GO
ALTER TABLE [dbo].[pet_packages]  WITH CHECK ADD FOREIGN KEY([pet_id])
REFERENCES [dbo].[pets] ([id])
GO
ALTER TABLE [dbo].[pets]  WITH CHECK ADD  CONSTRAINT [FK_pets_users] FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[pets] CHECK CONSTRAINT [FK_pets_users]
GO
ALTER TABLE [dbo].[products]  WITH CHECK ADD FOREIGN KEY([category_id])
REFERENCES [dbo].[categories] ([id])
GO
ALTER TABLE [dbo].[role_permissions]  WITH CHECK ADD  CONSTRAINT [FK_role_permissions_permissions] FOREIGN KEY([permission_id])
REFERENCES [dbo].[permissions] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[role_permissions] CHECK CONSTRAINT [FK_role_permissions_permissions]
GO
ALTER TABLE [dbo].[role_permissions]  WITH CHECK ADD  CONSTRAINT [FK_role_permissions_roles] FOREIGN KEY([role_id])
REFERENCES [dbo].[roles] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[role_permissions] CHECK CONSTRAINT [FK_role_permissions_roles]
GO
ALTER TABLE [dbo].[staff]  WITH CHECK ADD  CONSTRAINT [FK_staff_users] FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
GO
ALTER TABLE [dbo].[staff] CHECK CONSTRAINT [FK_staff_users]
GO
ALTER TABLE [dbo].[users]  WITH CHECK ADD  CONSTRAINT [FK_users_roles] FOREIGN KEY([role_id])
REFERENCES [dbo].[roles] ([id])
GO
ALTER TABLE [dbo].[users] CHECK CONSTRAINT [FK_users_roles]
GO
ALTER TABLE [dbo].[vaccination_records]  WITH CHECK ADD FOREIGN KEY([pet_id])
REFERENCES [dbo].[pets] ([id])
GO
ALTER TABLE [dbo].[vaccination_records]  WITH CHECK ADD FOREIGN KEY([vet_id])
REFERENCES [dbo].[veterinarians] ([id])
GO
ALTER TABLE [dbo].[veterinarians]  WITH CHECK ADD  CONSTRAINT [FK_veterinarians_users] FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
GO
ALTER TABLE [dbo].[veterinarians] CHECK CONSTRAINT [FK_veterinarians_users]
GO
ALTER TABLE [dbo].[appointments_services]  WITH CHECK ADD CHECK  (([price_at_booking]>=(0)))
GO
ALTER TABLE [dbo].[breed_prediction_details]  WITH CHECK ADD CHECK  (([confidence]>=(0.0000) AND [confidence]<=(1.0000)))
GO
ALTER TABLE [dbo].[breed_predictions]  WITH CHECK ADD CHECK  (([confidence]>=(0.0000) AND [confidence]<=(1.0000)))
GO
ALTER TABLE [dbo].[cart_items]  WITH CHECK ADD CHECK  (([price]>=(0)))
GO
ALTER TABLE [dbo].[cart_items]  WITH CHECK ADD CHECK  (([quantity]>(0)))
GO
ALTER TABLE [dbo].[carts]  WITH CHECK ADD CHECK  (([status]='checked_out' OR [status]='active'))
GO
ALTER TABLE [dbo].[feedbacks]  WITH CHECK ADD CHECK  (([status]='resolved' OR [status]='pending'))
GO
ALTER TABLE [dbo].[import_details]  WITH CHECK ADD CHECK  (([import_price]>=(0)))
GO
ALTER TABLE [dbo].[import_details]  WITH CHECK ADD CHECK  (([quantity]>(0)))
GO
ALTER TABLE [dbo].[import_receipts]  WITH CHECK ADD CHECK  (([total_amount]>=(0)))
GO
ALTER TABLE [dbo].[invoices]  WITH CHECK ADD CHECK  (([tax_amount]>=(0)))
GO
ALTER TABLE [dbo].[invoices]  WITH CHECK ADD CHECK  (([total_amount]>=(0)))
GO
ALTER TABLE [dbo].[medical_prescriptions]  WITH CHECK ADD CHECK  (([duration_days]>(0)))
GO
ALTER TABLE [dbo].[notifications]  WITH CHECK ADD CHECK  (([type]='system' OR [type]='promotion' OR [type]='appointment'))
GO
ALTER TABLE [dbo].[order_items]  WITH CHECK ADD CHECK  (([price]>=(0)))
GO
ALTER TABLE [dbo].[order_items]  WITH CHECK ADD CHECK  (([quantity]>(0)))
GO
ALTER TABLE [dbo].[orders]  WITH CHECK ADD CHECK  (([status]='cancelled' OR [status]='completed' OR [status]='shipping' OR [status]='confirmed' OR [status]='pending'))
GO
ALTER TABLE [dbo].[orders]  WITH CHECK ADD CHECK  (([total_amount]>=(0)))
GO
ALTER TABLE [dbo].[payments]  WITH CHECK ADD CHECK  (([amount]>=(0)))
GO
ALTER TABLE [dbo].[payments]  WITH CHECK ADD CHECK  (([payment_method]='e_wallet' OR [payment_method]='credit_card' OR [payment_method]='bank_transfer' OR [payment_method]='cash'))
GO
ALTER TABLE [dbo].[payments]  WITH CHECK ADD CHECK  (([status]='refunded' OR [status]='failed' OR [status]='completed' OR [status]='pending'))
GO
ALTER TABLE [dbo].[pets]  WITH CHECK ADD CHECK  (([gender]='Female' OR [gender]='Male'))
GO
ALTER TABLE [dbo].[products]  WITH CHECK ADD CHECK  (([import_price]>=(0)))
GO
ALTER TABLE [dbo].[products]  WITH CHECK ADD CHECK  (([sell_price]>=(0)))
GO
ALTER TABLE [dbo].[products]  WITH CHECK ADD CHECK  (([status]='inactive' OR [status]='active'))
GO
ALTER TABLE [dbo].[products]  WITH CHECK ADD CHECK  (([stock_quantity]>=(0)))
GO
ALTER TABLE [dbo].[service_packages]  WITH CHECK ADD CHECK  (([duration_minutes]>(0)))
GO
ALTER TABLE [dbo].[service_packages]  WITH CHECK ADD CHECK  (([price]>=(0)))
GO
ALTER TABLE [dbo].[service_packages]  WITH CHECK ADD CHECK  (([validity_days]>(0)))
GO
ALTER TABLE [dbo].[services]  WITH CHECK ADD CHECK  (([duration_minutes]>(0)))
GO
ALTER TABLE [dbo].[services]  WITH CHECK ADD CHECK  (([price]>=(0)))
GO
USE [master]
GO
ALTER DATABASE [PetsManager] SET  READ_WRITE 
GO




-- 1) Tạo role (bỏ qua nếu đã tồn tại)
IF NOT EXISTS (SELECT 1 FROM roles WHERE name = N'admin')
    INSERT INTO roles (name, description, status) VALUES (N'admin', N'Quản trị hệ thống - toàn quyền', N'active');
 
IF NOT EXISTS (SELECT 1 FROM roles WHERE name = N'staff')
    INSERT INTO roles (name, description, status) VALUES (N'staff', N'Nhân viên', N'active');
 
IF NOT EXISTS (SELECT 1 FROM roles WHERE name = N'customer')
    INSERT INTO roles (name, description, status) VALUES (N'customer', N'Khách hàng', N'active');
GO
 
-- 2) Tạo tài khoản admin (nếu đã tồn tại thì cập nhật lại mật khẩu theo hash mới)
IF NOT EXISTS (SELECT 1 FROM users WHERE username = N'admin01')
BEGIN
    INSERT INTO users (role_id, username, password_hash, full_name, email, phone_number, address, status)
    SELECT
        r.id,
        N'admin01',
        N'AQAAAAEAAYagAAAAEBrHT5QHuvaFgqKXfEAwl7aJi1REVahgC8n/isILsiBYSLyTSxzNS7V2qUKzZkGbrg==',
        N'Quan tri vien',
        N'admin01@petcare.vn',
        N'0900000001',
        N'Ho Chi Minh',
        N'active'
    FROM roles r WHERE r.name = N'admin';
END
ELSE
BEGIN
    UPDATE users
    SET password_hash = N'AQAAAAEAAYagAAAAEBrHT5QHuvaFgqKXfEAwl7aJi1REVahgC8n/isILsiBYSLyTSxzNS7V2qUKzZkGbrg==',
        status = N'active'
    WHERE username = N'admin01';
END
GO
 
SELECT id, username, role_id, status FROM users WHERE username = N'admin01';
GO