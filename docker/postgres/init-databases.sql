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

-- Account Service Database (Accounts, Roles)
CREATE DATABASE account_db;

-- Shop Service Database (Shops, Shop_Follower)
CREATE DATABASE shop_db;

-- Catalog Service Database (Category, Product_Category)
CREATE DATABASE catalog_db;

-- Product Service Database (Product_Master, Product_Version, Product_Media)
CREATE DATABASE product_db;

-- Certification Service Database (Appraisal_Certificate, Appraisal_Record, Certificate_Signature)
CREATE DATABASE certification_db;

-- Inventory Service Database (Inventory_SKU, Inventory_Batch)
CREATE DATABASE inventory_db;

-- Order Service Database (Carts, Cart_Items, Orders, Order_Items)
CREATE DATABASE order_db;

-- Payment Service Database (Wallet, Wallet_Transaction, Payment)
CREATE DATABASE payment_db;

-- Audit Service Database (Financial_Log)
CREATE DATABASE audit_db;

-- ================================================================
-- CẤP QUYỀN CHO USER
-- ================================================================
GRANT ALL PRIVILEGES ON DATABASE account_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE shop_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE catalog_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE product_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE certification_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE inventory_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE order_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE payment_db TO woodify;
GRANT ALL PRIVILEGES ON DATABASE audit_db TO woodify;

-- ================================================================
-- TẠO EXTENSION uuid-ossp CHO TỪNG DATABASE
-- ================================================================
\c account_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c shop_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c catalog_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c product_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c certification_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c inventory_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c order_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c payment_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c audit_db
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ================================================================
-- HOÀN TẤT
-- ================================================================
-- Các database đã được tạo. EF Core Migrations sẽ tự động tạo tables.
