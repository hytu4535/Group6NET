document.addEventListener('DOMContentLoaded', function () {
    var toggleBtn = document.querySelector('.btn-toggle-sidebar');
    var sidebar = document.querySelector('.admin-sidebar');

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });

        document.querySelectorAll('.menu-toggle').forEach(function (toggle) {
            toggle.addEventListener('click', function (event) {
                event.preventDefault();
                var group = toggle.closest('.menu-group');
                if (group) {
                    group.classList.toggle('menu-open');
                }
            });
        });

        document.querySelectorAll('.admin-sidebar a:not(.menu-toggle)').forEach(function (link) {
            link.addEventListener('click', function () {
                if (window.innerWidth <= 900) {
                    sidebar.classList.remove('show');
                }
            });
        });
    }
});
