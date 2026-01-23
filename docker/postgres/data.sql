-- ================================================================
-- IDENTITY SERVICE
-- ================================================================
-- Connect to identity_db
\c identity_db

-- Tạo Roles
INSERT INTO roles (role_id, role_name, description, createdat, updatedat, is_active)
VALUES
(gen_random_uuid(), 'Admin',    'Full access: manage users, roles, system settings and data.',          Now(), Now(), TRUE),
(gen_random_uuid(), 'Support',  'Handles customer inquiries, tickets, and troubleshooting.',         Now(), Now(), TRUE),
(gen_random_uuid(), 'Staff',    'Internal staff with limited management and operational permissions.', Now(), Now(), TRUE),
(gen_random_uuid(), 'Seller',   'Seller role: manage product listings, inventory and orders.',        Now(), Now(), TRUE),
(gen_random_uuid(), 'Customer', 'End user: browse products, place orders and manage their account.', Now(), Now(), TRUE)
ON CONFLICT DO NOTHING;

-- Tạo Accounts (Admin: 1, Staff: 1, Support: 2, Seller: 2, Customer: 2)
INSERT INTO accounts (account_id, username, password, email, name, phone_number, role_id, createdat, updatedat, is_active)
VALUES
(gen_random_uuid(), 'admin01', 'password1', 'admin@woodify.com', 'Admin User', '0901000001', (SELECT role_id FROM roles WHERE role_name = 'Admin'), Now(), Now(), TRUE),
(gen_random_uuid(), 'staff01', 'password2', 'staff01@woodify.com', 'Staff User', '0901000002', (SELECT role_id FROM roles WHERE role_name = 'Staff'), Now(), Now(), TRUE),
(gen_random_uuid(), 'support01', 'password3', 'support01@woodify.com', 'Support One', '0901000003', (SELECT role_id FROM roles WHERE role_name = 'Support'), Now(), Now(), TRUE),
(gen_random_uuid(), 'support02', 'password4', 'support02@woodify.com', 'Support Two', '0901000004', (SELECT role_id FROM roles WHERE role_name = 'Support'), Now(), Now(), TRUE),
(gen_random_uuid(), 'seller01', 'password5', 'seller01@gmail.com', 'Nguyễn Văn Seller', '0901000005', (SELECT role_id FROM roles WHERE role_name = 'Seller'), Now(), Now(), TRUE),
(gen_random_uuid(), 'seller02', 'password6', 'seller02@gmail.com', 'Trần Thị Seller', '0901000006', (SELECT role_id FROM roles WHERE role_name = 'Seller'), Now(), Now(), TRUE),
(gen_random_uuid(), 'customer01', 'password7', 'customer01@gmail.com', 'Lê Văn Customer', '0901000007', (SELECT role_id FROM roles WHERE role_name = 'Customer'), Now(), Now(), TRUE),
(gen_random_uuid(), 'customer02', 'password8', 'customer02@gmail.com', 'Phạm Thị Customer', '0901000008', (SELECT role_id FROM roles WHERE role_name = 'Customer'), Now(), Now(), TRUE)
ON CONFLICT DO NOTHING;

-- ================================================================
-- SHOP SERVICE
-- ================================================================

-- ================================================================
-- PRODUCT SERVICE
-- ================================================================
-- Connect to product_db
\c product_db

-- Tạo Categories (Danh mục đồ gỗ)
INSERT INTO category (category_id, name, description, parent_category_id, is_active, created_at, updated_at)
VALUES
(gen_random_uuid(), 'Nội thất phòng khách', 'Các sản phẩm nội thất gỗ dành cho phòng khách', NULL, TRUE, Now(), Now()),
(gen_random_uuid(), 'Nội thất phòng ngủ', 'Các sản phẩm nội thất gỗ dành cho phòng ngủ', NULL, TRUE, Now(), Now()),
(gen_random_uuid(), 'Nội thất phòng ăn', 'Các sản phẩm nội thất gỗ dành cho phòng ăn', NULL, TRUE, Now(), Now()),
(gen_random_uuid(), 'Nội thất văn phòng', 'Các sản phẩm nội thất gỗ dành cho văn phòng', NULL, TRUE, Now(), Now()),
(gen_random_uuid(), 'Đồ trang trí', 'Các sản phẩm trang trí từ gỗ', NULL, TRUE, Now(), Now()),
(gen_random_uuid(), 'Đồ gỗ ngoài trời', 'Các sản phẩm gỗ dùng ngoài trời', NULL, TRUE, Now(), Now())
ON CONFLICT DO NOTHING;

-- Tạo Product Masters
INSERT INTO product_master (product_id, shop_id, category_id, global_sku, status, certified, current_version_id, avg_rating, review_count, created_at, updated_at)
VALUES
(gen_random_uuid(), (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller01'), (SELECT category_id FROM category WHERE name = 'Nội thất phòng khách'), 'WOOD-SOFA-001', 'Active', TRUE, NULL, 4.5, 25, Now(), Now()),
(gen_random_uuid(), (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller01'), (SELECT category_id FROM category WHERE name = 'Nội thất phòng ngủ'), 'WOOD-BED-001', 'Active', TRUE, NULL, 4.8, 42, Now(), Now()),
(gen_random_uuid(), (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller01'), (SELECT category_id FROM category WHERE name = 'Nội thất phòng ăn'), 'WOOD-TABLE-001', 'Active', TRUE, NULL, 4.2, 18, Now(), Now()),
(gen_random_uuid(), (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller02'), (SELECT category_id FROM category WHERE name = 'Nội thất văn phòng'), 'WOOD-DESK-001', 'Active', TRUE, NULL, 4.6, 33, Now(), Now()),
(gen_random_uuid(), (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller02'), (SELECT category_id FROM category WHERE name = 'Đồ trang trí'), 'WOOD-DECOR-001', 'Active', FALSE, NULL, 4.0, 12, Now(), Now()),
(gen_random_uuid(), (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller02'), (SELECT category_id FROM category WHERE name = 'Đồ gỗ ngoài trời'), 'WOOD-OUTDOOR-001', 'Draft', TRUE, NULL, 0, 0, Now(), Now())
ON CONFLICT DO NOTHING;

-- Tạo Product Versions
INSERT INTO product_version (version_id, product_id, title, description, price_cents, currency, sku, ar_available, created_by, created_at, updated_at)
VALUES
(gen_random_uuid(), (SELECT product_id FROM product_master WHERE global_sku = 'WOOD-SOFA-001'), 'Ghế Sofa Gỗ Óc Chó Cao Cấp', 'Ghế sofa được làm từ gỗ óc chó tự nhiên, thiết kế hiện đại, phù hợp phòng khách rộng. Kích thước: 2m x 0.9m x 0.8m', 25000000, 'VND', 'SOFA-OC-CHO-V1', TRUE, (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller01'), Now(), Now()),
(gen_random_uuid(), (SELECT product_id FROM product_master WHERE global_sku = 'WOOD-BED-001'), 'Giường Ngủ Gỗ Sồi 1m8', 'Giường ngủ gỗ sồi Mỹ nhập khẩu, có ngăn kéo tiện lợi. Kích thước: 1.8m x 2m', 18000000, 'VND', 'BED-SOI-1M8-V1', TRUE, (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller01'), Now(), Now()),
(gen_random_uuid(), (SELECT product_id FROM product_master WHERE global_sku = 'WOOD-TABLE-001'), 'Bàn Ăn Gỗ Teak 6 Ghế', 'Bộ bàn ăn 6 ghế gỗ teak cao cấp, thiết kế sang trọng. Kích thước bàn: 1.6m x 0.9m', 32000000, 'VND', 'TABLE-TEAK-6G-V1', FALSE, (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller01'), Now(), Now()),
(gen_random_uuid(), (SELECT product_id FROM product_master WHERE global_sku = 'WOOD-DESK-001'), 'Bàn Làm Việc Gỗ Ash Hiện Đại', 'Bàn làm việc gỗ ash phong cách Bắc Âu, có ngăn kéo và giá sách. Kích thước: 1.4m x 0.6m', 8500000, 'VND', 'DESK-ASH-MOD-V1', TRUE, (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller02'), Now(), Now()),
(gen_random_uuid(), (SELECT product_id FROM product_master WHERE global_sku = 'WOOD-DECOR-001'), 'Khung Gương Gỗ Hương Tròn', 'Khung gương trang trí được chạm khắc tinh xảo từ gỗ hương. Đường kính: 60cm', 4500000, 'VND', 'MIRROR-HUONG-V1', FALSE, (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller02'), Now(), Now()),
(gen_random_uuid(), (SELECT product_id FROM product_master WHERE global_sku = 'WOOD-OUTDOOR-001'), 'Bàn Ghế Sân Vườn Gỗ Lim', 'Bộ bàn ghế sân vườn 4 chỗ ngồi, gỗ lim chống mối mọt tự nhiên', 15000000, 'VND', 'OUTDOOR-LIM-4G-V1', FALSE, (SELECT account_id FROM identity_remote.accounts WHERE username = 'seller02'), Now(), Now())
ON CONFLICT DO NOTHING;

-- ================================================================
-- INVENTORY SERVICE
-- ================================================================

-- ================================================================
-- ORDER SERVICE
-- ================================================================

-- ================================================================
-- PAYMENT SERVICE
-- ================================================================
