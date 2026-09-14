document.addEventListener('DOMContentLoaded', function () {
    var toggleBtn = document.querySelector('.btn-toggle-sidebar');
    var sidebar = document.querySelector('.admin-sidebar');
    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });
    }
});
