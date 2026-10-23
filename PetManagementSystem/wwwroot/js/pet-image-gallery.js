document.addEventListener('DOMContentLoaded', function () {
    var gallery = document.getElementById('petImageGallery');
    if (!gallery) {
        return;
    }

    var pageSize = 12;
    var currentPage = 1;
    var imagesUrl = gallery.dataset.imagesUrl;
    var petsUrl = gallery.dataset.petsUrl;
    var items = document.getElementById('petImageItems');
    var pagination = document.getElementById('galleryPagination');
    var summary = document.getElementById('gallerySummary');
    var emptyMessage = document.getElementById('galleryEmpty');
    var loading = document.getElementById('galleryLoading');
    var alert = document.getElementById('galleryAlert');
    var uploadSection = document.getElementById('petImageUploadSection');
    var uploadForm = document.getElementById('petImageUploadForm');
    var uploadInput = document.getElementById('uploadImage');
    var uploadPreview = document.getElementById('uploadPreview');
    var defaultImageUrl = '/images/pet-avatar-default.jpeg';

    if (gallery.dataset.canUpload !== 'true') {
        uploadSection.classList.add('d-none');
    }

    function showAlert(message, type) {
        alert.textContent = message;
        alert.className = 'alert alert-' + type;
    }

    function hideAlert() {
        alert.textContent = '';
        alert.className = 'alert d-none';
    }

    async function request(url, options) {
        var response = await fetch(url, options || {});
        var result;
        try {
            result = await response.json();
        } catch {
            throw new Error('Máy chủ trả về phản hồi không hợp lệ (HTTP ' + response.status + ').');
        }

        if (!response.ok || result.success !== true) {
            var details = Array.isArray(result.errors) ? result.errors.filter(Boolean).join(' ') : '';
            throw new Error(details || result.message || 'Yêu cầu không thành công.');
        }

        return result;
    }

    function tokenHeaders() {
        var token = document.querySelector('#petImageUploadForm input[name="__RequestVerificationToken"]');
        return token ? { 'RequestVerificationToken': token.value } : {};
    }

    function addPetOptions(pets) {
        var filter = document.getElementById('petFilter');
        var uploadSelect = document.getElementById('uploadPetId');
        pets.forEach(function (pet) {
            var label = pet.name + ' - ' + (pet.ownerName || 'Chưa có chủ nuôi');
            [filter, uploadSelect].forEach(function (select) {
                var option = document.createElement('option');
                option.value = String(pet.id);
                option.textContent = label;
                select.appendChild(option);
            });
        });
    }

    function createImageCard(image) {
        var column = document.createElement('div');
        column.className = 'col';

        var card = document.createElement('article');
        card.className = 'card h-100 shadow-sm';

        var preview = document.createElement('img');
        preview.className = 'card-img-top object-fit-cover';
        preview.src = image.imageUrl || defaultImageUrl;
        preview.alt = 'Ảnh ' + image.petName;
        preview.loading = 'lazy';
        preview.style.height = '190px';
        preview.addEventListener('error', function () {
            preview.src = defaultImageUrl;
        }, { once: true });
        card.appendChild(preview);

        var body = document.createElement('div');
        body.className = 'card-body';

        var title = document.createElement('h2');
        title.className = 'h6 card-title mb-1';
        title.textContent = image.petName || 'Thú cưng #' + image.petId;
        body.appendChild(title);

        var owner = document.createElement('p');
        owner.className = 'small text-muted mb-2';
        owner.textContent = image.ownerName || 'Chưa có chủ nuôi';
        body.appendChild(owner);

        var date = document.createElement('p');
        date.className = 'small text-muted mb-2';
        var uploadedAt = new Date(image.uploadedAt);
        date.textContent = Number.isNaN(uploadedAt.getTime())
            ? ''
            : 'Tải lên: ' + uploadedAt.toLocaleString('vi-VN');
        body.appendChild(date);

        if (image.isAvatar) {
            var badge = document.createElement('span');
            badge.className = 'badge text-bg-success';
            badge.textContent = 'Ảnh đại diện';
            body.appendChild(badge);
        }

        card.appendChild(body);

        var actions = document.createElement('div');
        actions.className = 'card-footer bg-white d-flex gap-2';
        if (gallery.dataset.canSetAvatar === 'true' && !image.isAvatar) {
            var avatarButton = document.createElement('button');
            avatarButton.type = 'button';
            avatarButton.className = 'btn btn-sm btn-outline-success';
            avatarButton.dataset.action = 'avatar';
            avatarButton.dataset.petId = String(image.petId);
            avatarButton.dataset.imageId = String(image.id);
            avatarButton.textContent = 'Đặt làm đại diện';
            actions.appendChild(avatarButton);
        }
        if (gallery.dataset.canDelete === 'true') {
            var deleteButton = document.createElement('button');
            deleteButton.type = 'button';
            deleteButton.className = 'btn btn-sm btn-outline-danger';
            deleteButton.dataset.action = 'delete';
            deleteButton.dataset.petId = String(image.petId);
            deleteButton.dataset.imageId = String(image.id);
            deleteButton.textContent = 'Xóa ảnh';
            actions.appendChild(deleteButton);
        }
        if (actions.childElementCount > 0) {
            card.appendChild(actions);
        }

        column.appendChild(card);
        return column;
    }

    function renderPagination(page, pageCount) {
        pagination.replaceChildren();
        if (pageCount <= 1) {
            return;
        }

        function addPageButton(label, targetPage, disabled, active, ariaLabel) {
            var item = document.createElement('li');
            item.className = 'page-item' + (disabled ? ' disabled' : '') + (active ? ' active' : '');
            var button = document.createElement('button');
            button.type = 'button';
            button.className = 'page-link';
            button.textContent = label;
            if (ariaLabel) {
                button.setAttribute('aria-label', ariaLabel);
            }
            button.disabled = disabled || active;
            button.addEventListener('click', function () {
                currentPage = targetPage;
                loadImages();
            });
            item.appendChild(button);
            pagination.appendChild(item);
        }

        addPageButton('Trước', page - 1, page <= 1, false, 'Trang trước');
        var firstPage = Math.max(1, page - 2);
        var lastPage = Math.min(pageCount, page + 2);
        for (var number = firstPage; number <= lastPage; number += 1) {
            addPageButton(String(number), number, false, number === page, 'Trang ' + number);
        }
        addPageButton('Sau', page + 1, page >= pageCount, false, 'Trang sau');
    }

    async function loadImages(preserveAlert) {
        loading.classList.remove('d-none');
        emptyMessage.classList.add('d-none');
        items.replaceChildren();
        if (!preserveAlert) {
            hideAlert();
        }

        var query = new URLSearchParams({
            page: String(currentPage),
            pageSize: String(pageSize)
        });
        var search = document.getElementById('imageSearch').value.trim();
        var petId = document.getElementById('petFilter').value;
        var isAvatar = document.getElementById('avatarFilter').value;
        if (search) query.set('searchKeyword', search);
        if (petId) query.set('petId', petId);
        if (isAvatar) query.set('isAvatar', isAvatar);

        try {
            var result = await request(imagesUrl + '?' + query.toString());
            var pageCount = Math.ceil((result.totalItems || 0) / result.pageSize);
            if ((result.totalItems > 0 && result.page > pageCount) ||
                (result.totalItems === 0 && result.page !== 1)) {
                currentPage = pageCount || 1;
                await loadImages();
                return;
            }

            currentPage = result.page;
            result.data.forEach(function (image) {
                items.appendChild(createImageCard(image));
            });
            summary.textContent = result.totalItems === 0
                ? 'Tổng cộng: 0 ảnh'
                : 'Hiển thị ' + ((result.page - 1) * result.pageSize + 1) + '–' +
                    Math.min(result.page * result.pageSize, result.totalItems) +
                    ' trong tổng số ' + result.totalItems + ' ảnh';
            emptyMessage.classList.toggle('d-none', result.data.length > 0);
            renderPagination(result.page, pageCount);
        } catch (error) {
            summary.textContent = '';
            showAlert(error.message, 'danger');
        } finally {
            loading.classList.add('d-none');
        }
    }

    document.getElementById('petImageFilters').addEventListener('submit', function (event) {
        event.preventDefault();
        currentPage = 1;
        loadImages();
    });

    uploadInput.addEventListener('change', function () {
        var file = uploadInput.files && uploadInput.files[0];
        if (!file) {
            uploadPreview.src = defaultImageUrl;
            return;
        }
        uploadPreview.src = URL.createObjectURL(file);
    });

    uploadForm.addEventListener('submit', async function (event) {
        event.preventDefault();
        hideAlert();
        var button = document.getElementById('uploadButton');
        button.disabled = true;
        try {
            var formData = new FormData(uploadForm);
            var result = await request(imagesUrl, {
                method: 'POST',
                headers: tokenHeaders(),
                body: formData
            });
            uploadForm.reset();
            uploadPreview.src = defaultImageUrl;
            currentPage = 1;
            await loadImages(true);
            if (!alert.classList.contains('alert-danger')) {
                showAlert(result.message, 'success');
            }
        } catch (error) {
            showAlert(error.message, 'danger');
        } finally {
            button.disabled = false;
        }
    });

    items.addEventListener('click', async function (event) {
        var button = event.target.closest('button[data-action]');
        if (!button) {
            return;
        }

        var petId = button.dataset.petId;
        var imageId = button.dataset.imageId;
        if (button.dataset.action === 'delete' &&
            !window.confirm('Bạn có chắc muốn xóa ảnh này không?')) {
            return;
        }

        button.disabled = true;
        try {
            var isAvatarAction = button.dataset.action === 'avatar';
            var result = await request(
                imagesUrl + '/' + encodeURIComponent(petId) + '/' + encodeURIComponent(imageId) +
                    (isAvatarAction ? '/avatar' : ''),
                {
                    method: isAvatarAction ? 'PUT' : 'DELETE',
                    headers: tokenHeaders()
                });
            await loadImages(true);
            if (!alert.classList.contains('alert-danger')) {
                showAlert(result.message, 'success');
            }
        } catch (error) {
            button.disabled = false;
            showAlert(error.message, 'danger');
        }
    });

    async function loadPets() {
        try {
            var result = await request(petsUrl);
            addPetOptions(result.data);
        } catch (error) {
            showAlert(error.message, 'danger');
            uploadSection.classList.add('d-none');
        }
    }

    Promise.all([loadPets(), loadImages()]);
});
