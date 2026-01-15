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

-- ================================================================
-- INVENTORY SERVICE
-- ================================================================

-- ================================================================
-- ORDER SERVICE
-- ================================================================

-- ================================================================
-- PAYMENT SERVICE
-- ================================================================
