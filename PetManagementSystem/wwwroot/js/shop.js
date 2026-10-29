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

    // Cập nhật số lượng trên icon giỏ hàng ở thanh điều hướng (id="cartBadge" trong _Layout).
    function updateHeaderCart(quantity) {
        var badge = document.getElementById('cartBadge');
        if (!badge) { return; }
        var count = Number(quantity) || 0;
        badge.textContent = count > 99 ? '99+' : count;
        badge.classList.toggle('d-none', count === 0);
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
        if (result.success && result.data) { updateHeaderCart(result.data.cartQuantity); }
    });

    // ---- Trang giỏ hàng ----
    var cartTable = document.getElementById('cart-table');
    if (cartTable) {
        function applySummary(data) {
            var total = document.getElementById('cart-total');
            if (total) { total.textContent = formatMoney(data.cartTotal); }
            var count = document.getElementById('cart-count');
            if (count) { count.textContent = data.cartQuantity; }
            updateHeaderCart(data.cartQuantity);
        }

        // Chỉ cho bấm "Tiến hành thanh toán" khi không còn dòng nào báo lỗi (hết hàng, vượt tồn kho, ngừng bán).
        function refreshCheckoutState() {
            var hasIssue = cartTable.querySelector('.line-issue') !== null;
            var btn = document.getElementById('checkout-btn');
            var hint = document.getElementById('checkout-hint');
            if (btn) {
                btn.classList.toggle('disabled', hasIssue);
                btn.setAttribute('aria-disabled', hasIssue ? 'true' : 'false');
            }
            if (hint) { hint.classList.toggle('d-none', !hasIssue); }
        }

        // Các lần lưu số lượng đang chạy; nút "Tiến hành thanh toán" chờ chúng xong rồi mới chuyển trang.
        var pending = [];
        function track(promise) {
            pending.push(promise);
            var done = function () { pending = pending.filter(function (p) { return p !== promise; }); };
            promise.then(done, done);
            return promise;
        }

        // Lưu số lượng của một dòng. Vượt tồn kho thì tự điều chỉnh về số tối đa còn lại và LƯU luôn số đó,
        // để số hiển thị trên màn hình luôn đúng bằng số sẽ được đặt.
        // Trả về { ok: đã lưu thành công, adjusted: số lượng bị đổi so với số khách nhập }.
        async function commitQuantity(input) {
            var row = input.closest('tr');
            var previous = parseInt(input.getAttribute('data-prev'), 10);
            var stock = parseInt(input.getAttribute('data-stock'), 10);
            var quantity = parseInt(input.value, 10);

            if (!quantity || quantity < 1) {
                input.value = previous;
                showToast('Số lượng phải lớn hơn 0.', 'danger');
                return { ok: false, adjusted: true };
            }

            var adjusted = false;
            if (!isNaN(stock) && quantity > stock) {
                if (stock < 1) {
                    input.value = previous;
                    showToast('Sản phẩm này đã hết hàng.', 'danger');
                    return { ok: false, adjusted: true };
                }
                quantity = stock;
                input.value = stock;
                adjusted = true;
                showToast('Chỉ còn ' + stock + ' sản phẩm trong kho, số lượng đã được điều chỉnh về ' + stock + '.', 'warning');
            }

            if (quantity === previous) {
                return { ok: true, adjusted: adjusted };
            }

            input.disabled = true;
            var result = await callApi('PUT', '/api/client/cart/items/' + input.getAttribute('data-item-id'), { quantity: quantity });
            input.disabled = false;
            if (result === null) { return { ok: false, adjusted: true }; }

            if (!result.success) {
                input.value = previous;
                showToast((result.errors && result.errors[0]) || result.message, 'danger');
                return { ok: false, adjusted: true };
            }

            input.setAttribute('data-prev', result.data.quantity);
            input.value = result.data.quantity;
            row.querySelector('.line-total').textContent = formatMoney(result.data.lineTotal);
            var issue = row.querySelector('.line-issue');
            if (issue) { issue.remove(); }
            applySummary(result.data);
            refreshCheckoutState();
            return { ok: true, adjusted: adjusted };
        }

        cartTable.addEventListener('change', function (event) {
            var input = event.target.closest('.qty-input');
            if (!input) { return; }
            track(commitQuantity(input));
        });

        // Bấm "Tiến hành thanh toán": lưu nốt mọi số lượng chưa lưu, chỉ chuyển trang khi giỏ hàng khớp với những gì khách thấy.
        var checkoutBtn = document.getElementById('checkout-btn');
        if (checkoutBtn) {
            checkoutBtn.addEventListener('click', async function (event) {
                event.preventDefault();
                if (checkoutBtn.classList.contains('disabled')) { return; }

                var changed = false;
                (await Promise.all(pending.slice())).forEach(function (r) {
                    if (r && (!r.ok || r.adjusted)) { changed = true; }
                });

                var inputs = cartTable.querySelectorAll('.qty-input');
                for (var i = 0; i < inputs.length; i++) {
                    var input = inputs[i];
                    if (parseInt(input.value, 10) !== parseInt(input.getAttribute('data-prev'), 10)) {
                        var r = await track(commitQuantity(input));
                        if (!r.ok || r.adjusted) { changed = true; }
                    }
                }

                if (changed || cartTable.querySelector('.line-issue')) {
                    showToast('Số lượng trong giỏ hàng đã được cập nhật theo tồn kho. Vui lòng kiểm tra lại rồi bấm "Tiến hành thanh toán".', 'warning');
                    return;
                }

                window.location.href = checkoutBtn.href;
            });
        }

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
            refreshCheckoutState();
        });
    }

    // ---- Thanh toán: chặn gửi trùng ----
    var checkoutForm = document.getElementById('checkout-form');
    if (checkoutForm) {
        // Số điện thoại: tự bỏ khoảng trắng, dấu chấm, gạch ngang khi rời ô nhập ("0912 345 678" -> "0912345678").
        var phoneInput = checkoutForm.querySelector('input[name="Input.Phone"]');
        if (phoneInput) {
            phoneInput.addEventListener('blur', function () {
                phoneInput.value = phoneInput.value.replace(/[\s.\-()]/g, '');
            });
        }

        // Chọn địa chỉ có sẵn: điền vào ô địa chỉ; chọn "Nhập địa chỉ giao mới": xóa ô để nhập lại.
        var picker = document.getElementById('address-picker');
        var addressInput = checkoutForm.querySelector('input[name="Input.ShippingAddress"]');
        if (picker && addressInput) {
            picker.addEventListener('change', function (e) {
                var radio = e.target;
                if (!radio || radio.name !== 'address-choice') { return; }
                if (radio.getAttribute('data-new') === 'true') {
                    addressInput.value = '';
                    addressInput.focus();
                } else {
                    addressInput.value = radio.value;
                }
                if (window.jQuery && window.jQuery.fn.valid) { window.jQuery(addressInput).valid(); }
            });

            // Người dùng tự sửa ô địa chỉ: đồng bộ lại lựa chọn radio (khớp địa chỉ có sẵn thì chọn, ngược lại là "địa chỉ mới").
            addressInput.addEventListener('input', function () {
                var value = addressInput.value.trim().toLowerCase();
                var radios = picker.querySelectorAll('input[name="address-choice"]');
                var matched = null;
                var fresh = null;
                radios.forEach(function (r) {
                    if (r.getAttribute('data-new') === 'true') { fresh = r; }
                    else if (r.value.trim().toLowerCase() === value) { matched = r; }
                });
                var target = matched || fresh;
                if (target) { target.checked = true; }
            });
        }

        checkoutForm.addEventListener('submit', function (event) {
            var submit = checkoutForm.querySelector('[type="submit"]');
            if (!submit) { return; }
            // Chạy sau mọi handler khác (jquery-validate): nếu form không hợp lệ và bị chặn gửi thì không khóa nút.
            setTimeout(function () {
                if (event.defaultPrevented) { return; }
                submit.disabled = true;
                submit.textContent = 'Đang xử lý...';
            }, 0);
        });
    }
})();
