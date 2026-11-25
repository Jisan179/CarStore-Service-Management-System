-- ============================================================================
-- CarStore and Service Management System
-- Academic Course: 00784 – Object Oriented Programming 2
-- Institution: American International University–Bangladesh (AIUB)
-- Semester: Summer 2025–2026
-- Database Name: CarStoreDB
-- ============================================================================

-- WARNING: running this script by hand DELETES all existing data in CarStoreDB.
-- The application runs it automatically only when none of its tables exist.
-- Seed passwords below are replaced with salted hashes on the first app start.

-- 1. DATABASE CREATION
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'CarStoreDB')
BEGIN
    CREATE DATABASE [CarStoreDB];
    PRINT 'Database CarStoreDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database CarStoreDB already exists.';
END
GO

USE [CarStoreDB];
GO

-- 2. DROP EXISTING TABLES IN REVERSE ORDER OF DEPENDENCIES (IF RE-INITIALIZING)
IF OBJECT_ID('dbo.ServiceRecords', 'U') IS NOT NULL DROP TABLE dbo.ServiceRecords;
IF OBJECT_ID('dbo.ServiceList', 'U') IS NOT NULL DROP TABLE dbo.ServiceList;
IF OBJECT_ID('dbo.SalesRecord', 'U') IS NOT NULL DROP TABLE dbo.SalesRecord;
IF OBJECT_ID('dbo.CarsStock', 'U') IS NOT NULL DROP TABLE dbo.CarsStock;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- ============================================================================
-- 3. TABLE DEFINITIONS
-- ============================================================================

-- 3.1 USERS TABLE
-- Stores credentials and user profile information with role-based access control.
CREATE TABLE [dbo].[Users]
(
    [UserID] INT IDENTITY(1,1) PRIMARY KEY,
    [FullName] NVARCHAR(100) NOT NULL,
    [Username] NVARCHAR(50) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
    [Password] NVARCHAR(200) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL,
    [Role] VARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT 'Customer'
        CONSTRAINT CK_Users_Role CHECK ([Role] IN ('Admin', 'Employee', 'Customer')),
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Status DEFAULT 'Active'
        CONSTRAINT CK_Users_Status CHECK ([Status] IN ('Active', 'Inactive')),
    [CreatedAt] DATETIME NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETDATE()
);
GO

-- 3.2 CARSSTOCK TABLE
-- Inventory of vehicles for sale and servicing.
CREATE TABLE [dbo].[CarsStock]
(
    [CarID] INT IDENTITY(1,1) PRIMARY KEY,
    [Brand] NVARCHAR(50) NOT NULL,
    [Model] NVARCHAR(50) NOT NULL,
    [Year] INT NOT NULL CONSTRAINT CK_CarsStock_Year CHECK ([Year] BETWEEN 1990 AND 2030),
    [Price] DECIMAL(12,2) NOT NULL CONSTRAINT CK_CarsStock_Price CHECK ([Price] >= 0),
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_CarsStock_Status DEFAULT 'Available'
        CONSTRAINT CK_CarsStock_Status CHECK ([Status] IN ('Available', 'Reserved', 'Sold', 'Unavailable')),
    [Image_Path] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME NOT NULL CONSTRAINT DF_CarsStock_CreatedAt DEFAULT GETDATE()
);
GO

-- 3.3 SALESRECORD TABLE
-- Tracks vehicle purchase requests, assignments, prices, and status updates.
CREATE TABLE [dbo].[SalesRecord]
(
    [SalesID] INT IDENTITY(1,1) PRIMARY KEY,
    [CarID] INT NOT NULL CONSTRAINT FK_SalesRecord_CarID FOREIGN KEY REFERENCES dbo.CarsStock([CarID]) ON DELETE CASCADE,
    [CustomerID] INT NOT NULL CONSTRAINT FK_SalesRecord_CustomerID FOREIGN KEY REFERENCES dbo.Users([UserID]),
    [EmployeeID] INT NULL CONSTRAINT FK_SalesRecord_EmployeeID FOREIGN KEY REFERENCES dbo.Users([UserID]),
    [SaleDate] DATETIME NOT NULL CONSTRAINT DF_SalesRecord_SaleDate DEFAULT GETDATE(),
    [Price] DECIMAL(12,2) NOT NULL CONSTRAINT CK_SalesRecord_Price CHECK ([Price] >= 0),
    [SalesStatus] VARCHAR(50) NOT NULL CONSTRAINT DF_SalesRecord_SalesStatus DEFAULT 'Pending'
        CONSTRAINT CK_SalesRecord_SalesStatus CHECK ([SalesStatus] IN ('Pending', 'Confirmed', 'Cancelled'))
);
GO

-- 3.4 SERVICELIST TABLE
-- Catalog of standardized automobile maintenance and repair services.
CREATE TABLE [dbo].[ServiceList]
(
    [ServiceID] INT IDENTITY(1,1) PRIMARY KEY,
    [ServiceName] NVARCHAR(200) NOT NULL,
    [Price] DECIMAL(12,2) NOT NULL CONSTRAINT CK_ServiceList_Price CHECK ([Price] >= 0)
);
GO

-- 3.5 SERVICERECORDS TABLE
-- Tracks vehicle maintenance requests, customer ownership, pricing, and progress.
-- Note: Intelligently extended with CustomerID and ServiceID to maintain full relational integrity
-- while preserving all columns defined in the original project specification.
CREATE TABLE [dbo].[ServiceRecords]
(
    [RecordID] INT IDENTITY(1,1) PRIMARY KEY,
    [CarID] INT NOT NULL CONSTRAINT FK_ServiceRecords_CarID FOREIGN KEY REFERENCES dbo.CarsStock([CarID]) ON DELETE CASCADE,
    [CustomerID] INT NOT NULL CONSTRAINT FK_ServiceRecords_CustomerID FOREIGN KEY REFERENCES dbo.Users([UserID]),
    [ServiceID] INT NULL CONSTRAINT FK_ServiceRecords_ServiceID FOREIGN KEY REFERENCES dbo.ServiceList([ServiceID]),
    [ServiceDate] DATETIME NOT NULL CONSTRAINT DF_ServiceRecords_ServiceDate DEFAULT GETDATE(),
    [ServiceDetails] NVARCHAR(MAX) NULL,
    [ServiceCost] DECIMAL(12,2) NOT NULL CONSTRAINT CK_ServiceRecords_ServiceCost CHECK ([ServiceCost] >= 0),
    [ServiceStatus] VARCHAR(50) NOT NULL CONSTRAINT DF_ServiceRecords_ServiceStatus DEFAULT 'Pending'
        CONSTRAINT CK_ServiceRecords_ServiceStatus CHECK ([ServiceStatus] IN ('Pending', 'In Progress', 'Completed', 'Cancelled'))
);
GO

-- ============================================================================
-- 4. PERFORMANCE INDEXES
-- ============================================================================
CREATE NONCLUSTERED INDEX IX_Users_Role_Status ON dbo.Users([Role], [Status]);
CREATE NONCLUSTERED INDEX IX_CarsStock_Status ON dbo.CarsStock([Status]);
CREATE NONCLUSTERED INDEX IX_SalesRecord_Status ON dbo.SalesRecord([SalesStatus]);
CREATE NONCLUSTERED INDEX IX_SalesRecord_CustomerID ON dbo.SalesRecord([CustomerID]);
CREATE NONCLUSTERED INDEX IX_ServiceRecords_Status ON dbo.ServiceRecords([ServiceStatus]);
CREATE NONCLUSTERED INDEX IX_ServiceRecords_CustomerID ON dbo.ServiceRecords([CustomerID]);
GO

-- ============================================================================
-- 5. SAMPLE DATA SEEDING
-- ============================================================================

-- 5.1 SEED USERS
-- (Credentials for testing clearly specified in README)
INSERT INTO [dbo].[Users] ([FullName], [Username], [Password], [Email], [Role], [Status])
VALUES
    (N'System Administrator', N'admin', N'Admin123!', N'admin@carstore.com', 'Admin', N'Active'),
    (N'Akbar Ali', N'employee1', N'Emp123!', N'akbar.ali@carstore.com', 'Employee', N'Active'),
    (N'Rahima Khatun', N'employee2', N'Emp123!', N'rahima.khatun@carstore.com', 'Employee', N'Active'),
    (N'Demo Customer', N'customer1', N'Cust123!', N'demo.customer@example.com', 'Customer', N'Active'),
    (N'Nusrat Jahan', N'customer2', N'Cust123!', N'nusrat.jahan@example.com', 'Customer', N'Active'),
    (N'Karim Uddin', N'customer3', N'Cust123!', N'karim.uddin@example.com', 'Customer', N'Active'),
    (N'Jamal Hossain', N'inactive_user', N'Pass123!', N'jamal.hossain@example.com', 'Customer', N'Inactive');
GO

-- 5.2 SEED CARS STOCK (demo prices in Bangladeshi Taka)
INSERT INTO [dbo].[CarsStock] ([Brand], [Model], [Year], [Price], [Status], [Image_Path])
VALUES
    (N'Toyota', N'Corolla Altis', 2022, 3500000.00, N'Available', N'toyota_corolla.jpg'),
    (N'Toyota', N'Camry Hybrid', 2023, 6500000.00, N'Available', N'toyota_camry.jpg'),
    (N'Honda', N'Civic RS', 2023, 4800000.00, N'Available', N'honda_civic.jpg'),
    (N'Honda', N'Vezel e:HEV', 2022, 3800000.00, N'Available', N'honda_vezel.jpg'),
    (N'Nissan', N'X-Trail e-POWER', 2021, 4200000.00, N'Available', N'nissan_xtrail.jpg'),
    (N'BMW', N'3 Series 330i', 2024, 12500000.00, N'Available', N'bmw_3series.jpg'),
    (N'Mercedes-Benz', N'C-Class C300', 2023, 11800000.00, N'Reserved', N'mercedes_cclass.jpg'),
    (N'Hyundai', N'Tucson AWD', 2022, 4500000.00, N'Sold', N'hyundai_tucson.jpg'),
    (N'Audi', N'A4 Quattro', 2023, 8500000.00, N'Available', N'audi_a4.jpg'),
    (N'Ford', N'Mustang GT', 2022, 15000000.00, N'Available', N'ford_mustang.jpg');
GO

-- 5.3 SEED SERVICE CATALOG (demo prices in Bangladeshi Taka)
INSERT INTO [dbo].[ServiceList] ([ServiceName], [Price])
VALUES
    (N'Full Synthetic Oil & Filter Change', 4500.00),
    (N'Engine Diagnostic & Health Check', 2500.00),
    (N'Complete Brake Pad & Rotor Service', 8500.00),
    (N'Tire Rotation, Balancing & Wheel Alignment', 3500.00),
    (N'AC Evaporator Clean & Refrigerant Recharge', 5500.00),
    (N'Full Comprehensive Vehicle Inspection & Service', 12000.00),
    (N'Automatic Transmission Fluid Flush', 9000.00),
    (N'Battery Replacement & Charging System Test', 11000.00);
GO

-- 5.4 SEED SALES RECORDS
INSERT INTO [dbo].[SalesRecord] ([CarID], [CustomerID], [EmployeeID], [SaleDate], [Price], [SalesStatus])
VALUES
    -- Past confirmed sale of CarID 8 (Hyundai Tucson) to the demo customer
    (8, 4, 2, DATEADD(DAY, -15, GETDATE()), 4500000.00, 'Confirmed'),
    -- Pending purchase request by the demo customer for CarID 7 (Mercedes C-Class)
    (7, 4, NULL, DATEADD(DAY, -2, GETDATE()), 11800000.00, 'Pending'),
    -- Cancelled request by Nusrat Jahan
    (1, 5, 3, DATEADD(DAY, -5, GETDATE()), 3500000.00, 'Cancelled');
GO

-- 5.5 SEED SERVICE RECORDS
INSERT INTO [dbo].[ServiceRecords] ([CarID], [CustomerID], [ServiceID], [ServiceDate], [ServiceDetails], [ServiceCost], [ServiceStatus])
VALUES
    -- Completed service for the demo customer on Hyundai Tucson (CarID 8)
    (8, 4, 1, DATEADD(DAY, -10, GETDATE()), N'Routine 8,000 km synthetic oil change with genuine filter replacement.', 4500.00, 'Completed'),
    -- In Progress service for Nusrat Jahan on Toyota Camry (CarID 2)
    (2, 5, 2, DATEADD(DAY, -1, GETDATE()), N'Check engine light illuminated; running full computer ECU diagnostic scan.', 2500.00, 'In Progress'),
    -- Pending service booking by Karim Uddin on Honda Civic (CarID 3)
    (3, 6, 3, DATEADD(DAY, 1, GETDATE()), N'Brake squeal inspection and front disc brake pad replacement.', 8500.00, 'Pending');
GO

PRINT '============================================================================';
PRINT 'CarStoreDB initialized with complete tables, constraints, and sample data.';
PRINT '============================================================================';
GO
