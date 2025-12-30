-- ================================================================
-- IDENTITY SERVICE
-- ================================================================
\c identity_db -- Connect to identity_db

-- Tạo Roles
INSERT INTO roles (role_id, role_name, description, createdat, updatedat, is_active)
VALUES
('a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'Admin', 'Administrator role', NOW(), NOW(), true),
('a1b2c3d4-e5f6-7890-abcd-ef1234567891', 'User', 'Regular user role', NOW(), NOW(), true),
('a1b2c3d4-e5f6-7890-abcd-ef1234567892', 'Seller', 'Seller role', NOW(), NOW(), true),
('a1b2c3d4-e5f6-7890-abcd-ef1234567893', 'Moderator', 'Moderator role', NOW(), NOW(), true),
('a1b2c3d4-e5f6-7890-abcd-ef1234567894', 'Viewer', 'Viewer role', NOW(), NOW(), true)
ON CONFLICT DO NOTHING;

-- Tạo 10 Accounts
INSERT INTO accounts (
    account_id, 
    username, 
    password_hash, 
    email, 
    name, 
    phone_number, 
    role_id, 
    createdat, 
    updatedat, 
    is_active
) VALUES
('550e8400-e29b-41d4-a716-446655440001', 'user01', '$2a$12$E8q7L5u/ZNzVf0LsR8zNj.JJzAzVn8vUY0n.Q0Q.VVJ.QvJ8v7YE2', 'user01@gmail.com', 'Nguyễn Văn A', '0901000001', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', NOW(), NOW(), true),
('550e8400-e29b-41d4-a716-446655440002', 'user02', '$2a$12$E8q7L5u/ZNzVf0LsR8zNj.JJzAzVn8vUY0n.Q0Q.VVJ.QvJ8v7YE2', 'user02@gmail.com', 'Trần Thị B', '0901000002', 'a1b2c3d4-e5f6-7890-abcd-ef1234567891', NOW(), NOW(), true),
('550e8400-e29b-41d4-a716-446655440003', 'user03', '$2a$12$E8q7L5u/ZNzVf0LsR8zNj.JJzAzVn8vUY0n.Q0Q.VVJ.QvJ8v7YE2', 'user03@gmail.com', 'Lê Văn C', '0901000003', 'a1b2c3d4-e5f6-7890-abcd-ef1234567892', NOW(), NOW(), true),
('550e8400-e29b-41d4-a716-446655440004', 'user04', '$2a$12$E8q7L5u/ZNzVf0LsR8zNj.JJzAzVn8vUY0n.Q0Q.VVJ.QvJ8v7YE2', 'user04@gmail.com', 'Phạm Thị D', '0901000004', 'a1b2c3d4-e5f6-7890-abcd-ef1234567893', NOW(), NOW(), true),
('550e8400-e29b-41d4-a716-446655440005', 'user05', '$2a$12$E8q7L5u/ZNzVf0LsR8zNj.JJzAzVn8vUY0n.Q0Q.VVJ.QvJ8v7YE2', 'user05@gmail.com', 'Hoàng Văn E', '0901000005', 'a1b2c3d4-e5f6-7890-abcd-ef1234567894', NOW(), NOW(), true),
('550e8400-e29b-41d4-a716-446655440006', 'user06', '$2a$12$E8q7L5u/ZNzVf0LsR8zNj.JJzAzVn8vUY0n.Q0Q.VVJ.QvJ8v7YE2', 'user06@gmail.com', 'Vũ Thị F', '0901000006', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', NOW(), NOW(), true),
('550e8400-e29b-41d4-a716-446655440007', 'user07', '$2a$12$E8q7L5u/ZNzVf0LsR8zNj.JJzAzVn8vUY0n.Q0Q.VVJ.QvJ8v7YE2', 'user07@gmail.com', 'Đặng Văn G', '0901000007', 'a1b2c3d4-e5f6-7890-abcd-ef1234567891', NOW(), NOW(), true),
('550e8400-e29b-41d4-a716-446655440008', 'user08', '$2a$12$E8q7L5u/ZNzVf0LsR8zNj.JJzAzVn8vUY0n.Q0Q.VVJ.QvJ8v7YE2', 'user08@gmail.com', 'Bùi Thị H', '0901000008', 'a1b2c3d4-e5f6-7890-abcd-ef1234567892', NOW(), NOW(), true),
('550e8400-e29b-41d4-a716-446655440009', 'user09', '$2a$12$E8q7L5u/ZNzVf0LsR8zNj.JJzAzVn8vUY0n.Q0Q.VVJ.QvJ8v7YE2', 'user09@gmail.com', 'Đỗ Văn I', '0901000009', 'a1b2c3d4-e5f6-7890-abcd-ef1234567893', NOW(), NOW(), false),
('550e8400-e29b-41d4-a716-446655440010', 'user10', '$2a$12$E8q7L5u/ZNzVf0LsR8zNj.JJzAzVn8vUY0n.Q0Q.VVJ.QvJ8v7YE2', 'user10@gmail.com', 'Ngô Thị K', '0901000010', 'a1b2c3d4-e5f6-7890-abcd-ef1234567894', NOW(), NOW(), false)
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
