-- Migration: Add Permission and RolePermission tables
-- Requirements: 1.1, 1.2, 1.3

-- Permission table
CREATE TABLE IF NOT EXISTS permission (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    created_by VARCHAR(36),
    updated_at DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    updated_by VARCHAR(36)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- RolePermission table (Many-to-Many)
CREATE TABLE IF NOT EXISTS role_permission (
    id INT AUTO_INCREMENT PRIMARY KEY,
    role_id VARCHAR(36) NOT NULL,
    permission_id INT NOT NULL,
    created_at DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    created_by VARCHAR(36),
    updated_at DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    updated_by VARCHAR(36),
    CONSTRAINT FK_role_permission_role FOREIGN KEY (role_id) REFERENCES role(id) ON DELETE CASCADE,
    CONSTRAINT FK_role_permission_permission FOREIGN KEY (permission_id) REFERENCES permission(id) ON DELETE CASCADE,
    CONSTRAINT UQ_role_permission UNIQUE (role_id, permission_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Seed default permissions
INSERT INTO permission (name, description) VALUES 
('view', 'Quyền xem dữ liệu'),
('manage', 'Quyền tạo và sửa dữ liệu'),
('delete', 'Quyền xóa dữ liệu')
ON DUPLICATE KEY UPDATE description = VALUES(description);

-- Seed default role-permission mappings
-- Admin: all permissions (view, manage, delete)
-- Shop: view + manage
-- User: view only

-- Get permission IDs
SET @view_id = (SELECT id FROM permission WHERE name = 'view');
SET @manage_id = (SELECT id FROM permission WHERE name = 'manage');
SET @delete_id = (SELECT id FROM permission WHERE name = 'delete');

-- Get role IDs (assuming roles already exist)
SET @admin_id = (SELECT id FROM role WHERE name = 'Admin' LIMIT 1);
SET @shop_id = (SELECT id FROM role WHERE name = 'Shop' LIMIT 1);
SET @user_id = (SELECT id FROM role WHERE name = 'User' LIMIT 1);

-- Admin: all permissions
INSERT IGNORE INTO role_permission (role_id, permission_id) 
SELECT @admin_id, @view_id WHERE @admin_id IS NOT NULL AND @view_id IS NOT NULL;
INSERT IGNORE INTO role_permission (role_id, permission_id) 
SELECT @admin_id, @manage_id WHERE @admin_id IS NOT NULL AND @manage_id IS NOT NULL;
INSERT IGNORE INTO role_permission (role_id, permission_id) 
SELECT @admin_id, @delete_id WHERE @admin_id IS NOT NULL AND @delete_id IS NOT NULL;

-- Shop: view + manage
INSERT IGNORE INTO role_permission (role_id, permission_id) 
SELECT @shop_id, @view_id WHERE @shop_id IS NOT NULL AND @view_id IS NOT NULL;
INSERT IGNORE INTO role_permission (role_id, permission_id) 
SELECT @shop_id, @manage_id WHERE @shop_id IS NOT NULL AND @manage_id IS NOT NULL;

-- User: view only
INSERT IGNORE INTO role_permission (role_id, permission_id) 
SELECT @user_id, @view_id WHERE @user_id IS NOT NULL AND @view_id IS NOT NULL;
