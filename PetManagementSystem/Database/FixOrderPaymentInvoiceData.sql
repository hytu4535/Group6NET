-- Đồng bộ dữ liệu cũ theo luồng Đơn hàng -> Thanh toán -> Hóa đơn (chạy tùy chọn, an toàn khi chạy lại nhiều lần).
-- Dữ liệu mới phát sinh sau khi cập nhật code đã tự đúng; script này chỉ sửa các đơn tạo trước đó.

-- 1) Đơn đã hủy thì các giao dịch đang chờ thanh toán chuyển sang "failed".
UPDATE p
SET p.status = 'failed'
FROM dbo.payments p
INNER JOIN dbo.orders o ON o.id = p.order_id
WHERE o.status = 'cancelled' AND p.status = 'pending';
GO

-- 2) Đơn tiền mặt đã hoàn thành: giao dịch tiền mặt đang chờ chuyển thành "completed".
UPDATE p
SET p.status = 'completed', p.payment_date = GETDATE()
FROM dbo.payments p
INNER JOIN dbo.orders o ON o.id = p.order_id
WHERE o.status = 'completed' AND p.status = 'pending' AND p.payment_method = 'cash';
GO

-- 3) Đơn chưa hủy, đã thanh toán đủ mà chưa có hóa đơn: tự lập hóa đơn (thuế 10% tổng tiền, issued_by = NULL).
INSERT INTO dbo.invoices (order_id, invoice_date, total_amount, tax_amount, issued_by)
SELECT o.id, GETDATE(), o.total_amount, ROUND(o.total_amount * 0.10, 2), NULL
FROM dbo.orders o
WHERE o.status <> 'cancelled'
  AND o.total_amount > 0
  AND NOT EXISTS (SELECT 1 FROM dbo.invoices i WHERE i.order_id = o.id)
  AND (SELECT COALESCE(SUM(p.amount), 0) FROM dbo.payments p WHERE p.order_id = o.id AND p.status = 'completed') >= o.total_amount;
GO
