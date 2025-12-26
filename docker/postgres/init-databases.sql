-- ================================================================
-- WOODIFY MICROSERVICES - DATABASE INITIALIZATION
-- ================================================================
-- Script này tự động chạy khi PostgreSQL container khởi tạo lần đầu
-- Tạo các database riêng biệt cho từng microservice
-- ================================================================

-- Tạo extension uuid-ossp cho database mặc định
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ================================================================
-- TẠO CÁC DATABASE CHO TỪNG SERVICE
-- ================================================================

-- Identity Service Database (User, Account)
CREATE DATABASE identity_db;

-- Shop Service Database (Shop, Follow)
CREATE DATABASE shop_db;

-- Product Service Database (Product, Certificate)
CREATE DATABASE product_db;

-- Inventory Service Database (Stock)
CREATE DATABASE inventory_db;

-- Order Service Database (Cart, Order)
CREATE DATABASE order_db;

-- Payment Service Database (Payment, Wallet)
CREATE DATABASE payment_db;

-- ================================================================
-- CẤP QUYỀN CHO USER
-- ================================================================
GRANT ALL PRIVILEGES ON DATABASE identity_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE shop_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE product_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE inventory_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE order_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE payment_db TO woodify;

-- ================================================================
-- TẠO EXTENSION uuid-ossp CHO TỪNG DATABASE
-- ================================================================
\c identity_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c shop_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c product_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c inventory_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c order_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c payment_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ================================================================
-- HOÀN TẤT
-- ================================================================
-- Các database đã được tạo. EF Core Migrations sẽ tự động tạo tables.
