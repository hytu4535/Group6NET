(function () {
    'use strict';

    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');

    function formatMoney(value) {
        return Number(value).toLocaleString('vi-VN', { maximumFractionDigits: 2 }) + 'đ';
    }

    function showToast(message, type) {
        var el = document.createElement('div');
        el.className = 'shop-toast alert alert-' + (type || 'success') + ' shadow';
        el.setAttribute('role', 'alert');
        el.textContent = message;
        document.body.appendChild(el);
        setTimeout(function () { el.remove(); }, 3500);
    }

    // Gọi API /api/client/...; trả về null nếu phiên đăng nhập hết hạn (đã chuyển hướng tới trang đăng nhập).
    async function callApi(method, url, body) {
        var headers = { 'Accept': 'application/json', 'RequestVerificationToken': tokenInput ? tokenInput.value : '' };
        if (body !== undefined) { headers['Content-Type'] = 'application/json'; }

        var response;
        try {
            response = await fetch(url, {
                method: method,
                headers: headers,
                credentials: 'same-origin',
                body: body !== undefined ? JSON.stringify(body) : undefined
            });
        } catch (e) {
            return { success: false, message: 'Không thể kết nối tới máy chủ.' };
        }

        var contentType = response.headers.get('content-type') || '';
        if (response.redirected || response.status === 401 || contentType.indexOf('json') === -1) {
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(location.pathname + location.search);
            return null;
        }

        return response.json();
    }

    function setBusy(button, busy) {
        if (!button) { return; }
        button.disabled = busy;
        button.classList.toggle('disabled', busy);
    }

    // ---- Thêm vào giỏ (danh sách + chi tiết sản phẩm) ----
    document.addEventListener('click', async function (event) {
        var button = event.target.closest('[data-add-to-cart]');
        if (!button) { return; }
        event.preventDefault();

        // Nút ở danh sách sản phẩm không có ô số lượng (mặc định 1); trang chi tiết trỏ tới ô nhập qua data-qty-input.
        var qtySelector = button.getAttribute('data-qty-input');
        var qtyInput = qtySelector ? document.querySelector(qtySelector) : null;
        var quantity = qtyInput ? parseInt(qtyInput.value, 10) || 1 : 1;

        setBusy(button, true);
        var result = await callApi('POST', '/api/client/cart/items', {
            productId: parseInt(button.getAttribute('data-add-to-cart'), 10),
            quantity: quantity
        });
        setBusy(button, false);

        if (result === null) { return; }
        var message = result.success ? result.message : (result.errors && result.errors[0]) || result.message;
        showToast(message, result.success ? 'success' : 'danger');
    });

    // ---- Trang giỏ hàng ----
    var cartTable = document.getElementById('cart-table');
    if (cartTable) {
        function applySummary(data) {
            var total = document.getElementById('cart-total');
            if (total) { total.textContent = formatMoney(data.cartTotal); }
            var count = document.getElementById('cart-count');
            if (count) { count.textContent = data.cartQuantity; }
        }

        cartTable.addEventListener('change', async function (event) {
            var input = event.target.closest('.qty-input');
            if (!input) { return; }

            var row = input.closest('tr');
            var previous = input.getAttribute('data-prev');
            var quantity = parseInt(input.value, 10);
            if (!quantity || quantity < 1) {
                input.value = previous;
                showToast('Số lượng phải lớn hơn 0.', 'danger');
                return;
            }

            input.disabled = true;
            var result = await callApi('PUT', '/api/client/cart/items/' + input.getAttribute('data-item-id'), { quantity: quantity });
            input.disabled = false;
            if (result === null) { return; }

            if (!result.success) {
                input.value = previous;
                showToast((result.errors && result.errors[0]) || result.message, 'danger');
                return;
            }

            input.setAttribute('data-prev', result.data.quantity);
            input.value = result.data.quantity;
            row.querySelector('.line-total').textContent = formatMoney(result.data.lineTotal);
            var issue = row.querySelector('.line-issue');
            if (issue) { issue.remove(); }
            applySummary(result.data);
        });

        cartTable.addEventListener('click', async function (event) {
            var button = event.target.closest('.remove-item');
            if (!button) { return; }
            if (!confirm('Xóa sản phẩm này khỏi giỏ hàng?')) { return; }

            setBusy(button, true);
            var result = await callApi('DELETE', '/api/client/cart/items/' + button.getAttribute('data-item-id'));
            setBusy(button, false);
            if (result === null) { return; }

            if (!result.success) {
                showToast((result.errors && result.errors[0]) || result.message, 'danger');
                return;
            }

            if (result.data.lineCount === 0) {
                window.location.reload();
                return;
            }

            button.closest('tr').remove();
            applySummary(result.data);
        });
    }

    // ---- Thanh toán: chặn gửi trùng ----
    var checkoutForm = document.getElementById('checkout-form');
    if (checkoutForm) {
        checkoutForm.addEventListener('submit', function () {
            var submit = checkoutForm.querySelector('[type="submit"]');
            if (submit) {
                setTimeout(function () { submit.disabled = true; submit.textContent = 'Đang xử lý...'; }, 0);
            }
        });
    }
})();
