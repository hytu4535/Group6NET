(() => {
    function clearField(control) {
        control.classList.remove('is-invalid', 'input-validation-error');
        control.setAttribute('aria-invalid', 'false');
        const error = document.querySelector(`[data-pet-error-for="${control.id || control.name}"]`);
        if (error) error.textContent = '';
    }

    function showFieldError(control, message) {
        control.classList.add('is-invalid');
        control.setAttribute('aria-invalid', 'true');
        const error = document.querySelector(`[data-pet-error-for="${control.id || control.name}"]`);
        if (error) error.textContent = message;
    }

    function validateField(control) {
        clearField(control);
        const value = control.value?.trim() ?? '';

        if (control.required && !value) {
            showFieldError(control, control.dataset.petRequiredMessage || 'Vui lòng nhập thông tin này.');
            return false;
        }

        if (value && control.dataset.petWhitespace === 'true' && !/\S/.test(value)) {
            showFieldError(control, 'Không được chỉ chứa khoảng trắng.');
            return false;
        }

        if (value && control.maxLength > 0 && value.length > control.maxLength) {
            showFieldError(control, `Không được vượt quá ${control.maxLength} ký tự.`);
            return false;
        }

        if (value && control.type === 'number') {
            const number = Number(value);
            const min = control.min === '' ? null : Number(control.min);
            const max = control.max === '' ? null : Number(control.max);
            if (!Number.isFinite(number) || (min !== null && number < min) || (max !== null && number > max)) {
                showFieldError(control, control.dataset.petRangeMessage || 'Giá trị không nằm trong khoảng cho phép.');
                return false;
            }
        }

        return true;
    }

    function setup(form) {
        if (!form) return;
        form.setAttribute('novalidate', '');

        const controls = [...form.querySelectorAll('[data-pet-field].pet-form-field')];
        controls.forEach(control => {
            control.addEventListener('input', () => validateField(control));
            control.addEventListener('change', () => validateField(control));

            const error = document.querySelector(`[data-pet-error-for="${control.id || control.name}"]`);
            if (error?.textContent.trim()) {
                control.classList.add('is-invalid');
                control.setAttribute('aria-invalid', 'true');
            }
        });
    }

    function validateForm(form) {
        const controls = [...form.querySelectorAll('[data-pet-field].pet-form-field')];
        let valid = true;
        controls.forEach(control => {
            if (!validateField(control)) valid = false;
        });
        if (!valid) {
            controls.find(control => control.classList.contains('is-invalid'))?.focus();
        }
        return valid;
    }

    function showServerErrors(form, errors) {
        if (!Array.isArray(errors)) return;
        const alias = {
            name: 'Name',
            species: 'Species',
            breed: 'Breed',
            gender: 'Gender',
            weightkg: 'WeightKg',
            userid: 'UserId',
            image: 'Image',
            avatarfile: 'AvatarFile'
        };

        errors.forEach(entry => {
            const separator = entry.indexOf(':');
            if (separator < 0) return;
            const key = entry.slice(0, separator).trim();
            const message = entry.slice(separator + 1).trim();
            const normalized = alias[key.toLowerCase()] || key;
            const control = form.querySelector(`[name="${normalized}"], #${normalized}`);
            if (control && message) showFieldError(control, message);
        });
    }

    window.PetFormValidation = { setup, validateForm, showServerErrors };
})();
