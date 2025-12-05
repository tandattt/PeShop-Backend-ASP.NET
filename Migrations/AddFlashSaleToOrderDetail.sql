-- Migration: Add FlashSale fields to Order and OrderDetail tables
-- Date: 2025-11-28
-- Updated: 2025-11-30
-- Description: 
--   1. Thêm column FlashSaleProductId vào OrderDetail để track sản phẩm flash sale
--   2. Thêm column HasFlashSale vào Orders để đánh dấu order có flash sale

-- =====================================================
-- BƯỚC 1: Thêm column flash_sale_product_id vào order_detail
-- =====================================================
ALTER TABLE order_detail 
ADD COLUMN IF NOT EXISTS flash_sale_product_id VARCHAR(255) NULL AFTER variant_id;

-- Thêm foreign key constraint (bỏ qua nếu đã tồn tại)
-- ALTER TABLE order_detail
-- ADD CONSTRAINT fk_order_detail_flash_sale_product
-- FOREIGN KEY (flash_sale_product_id) 
-- REFERENCES flash_sale_product(id)
-- ON DELETE SET NULL
-- ON UPDATE CASCADE;

-- Thêm index để tối ưu query
CREATE INDEX IF NOT EXISTS idx_order_detail_flash_sale_product_id 
ON order_detail(flash_sale_product_id);

-- =====================================================
-- BƯỚC 2: Thêm column has_flash_sale vào orders
-- =====================================================
ALTER TABLE orders 
ADD COLUMN IF NOT EXISTS has_flash_sale BOOLEAN NULL;

-- Thêm index để tối ưu query
CREATE INDEX IF NOT EXISTS idx_orders_has_flash_sale 
ON orders(has_flash_sale);

-- =====================================================
-- BƯỚC 3: Cập nhật data cũ (optional)
-- =====================================================
-- Cập nhật has_flash_sale cho các orders cũ dựa trên order_detail
-- UPDATE orders o
-- SET o.has_flash_sale = EXISTS (
--     SELECT 1 FROM order_detail od 
--     WHERE od.order_id = o.id 
--     AND od.flash_sale_product_id IS NOT NULL
-- )
-- WHERE o.has_flash_sale IS NULL;
