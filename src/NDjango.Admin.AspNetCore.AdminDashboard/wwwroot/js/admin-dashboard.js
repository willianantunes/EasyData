// Sidebar filter
document.addEventListener('DOMContentLoaded', function () {
    var searchInput = document.getElementById('sidebar-search');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            var filter = this.value.toLowerCase();
            var items = document.querySelectorAll('.sidebar-model-item');
            items.forEach(function (item) {
                var text = item.textContent.toLowerCase();
                item.style.display = text.indexOf(filter) !== -1 ? '' : 'none';
            });
        });
    }

    // Bulk action checkbox management
    var actionToggle = document.getElementById('action-toggle');
    var actionCheckboxes = document.querySelectorAll('.action-select');
    var actionCounter = document.querySelector('.action-counter');
    var changelistForm = document.getElementById('changelist-form');

    if (actionToggle) {
        actionToggle.addEventListener('change', function () {
            actionCheckboxes.forEach(function (cb) {
                cb.checked = actionToggle.checked;
                cb.closest('tr').classList.toggle('selected', cb.checked);
            });
            updateActionCounter();
        });
    }

    actionCheckboxes.forEach(function (cb) {
        cb.addEventListener('change', function () {
            this.closest('tr').classList.toggle('selected', this.checked);
            if (actionToggle) {
                actionToggle.checked = Array.from(actionCheckboxes).every(function (c) { return c.checked; });
            }
            updateActionCounter();
        });
    });

    function updateActionCounter() {
        if (!actionCounter) return;
        var checked = document.querySelectorAll('.action-select:checked').length;
        var total = actionCheckboxes.length;
        actionCounter.textContent = checked + ' of ' + total + ' selected';
    }

    if (changelistForm) {
        changelistForm.addEventListener('submit', function (e) {
            var select = changelistForm.querySelector('select[name="action"]');
            if (!select || !select.value) {
                e.preventDefault();
                return;
            }
            var selected = select.options[select.selectedIndex];
            var allowEmpty = selected.getAttribute('data-allow-empty') === 'true';
            var checkedCount = document.querySelectorAll('.action-select:checked').length;
            if (checkedCount === 0 && !allowEmpty) {
                e.preventDefault();
                alert('Please select at least one item.');
                return;
            }
        });
    }

    // Popup dismiss delegation — handles clicks on .popup-select links
    // rendered in the popup list view instead of inline onclick handlers.
    document.addEventListener('click', function (e) {
        var link = e.target.closest('.popup-select');
        if (!link) return;
        e.preventDefault();
        if (window.opener && typeof window.opener.dismissRelatedLookupPopup === 'function') {
            window.opener.dismissRelatedLookupPopup(window, link.getAttribute('data-pk'));
        }
    });
});

function showRelatedObjectLookupPopup(triggerLink) {
    var inputId = triggerLink.id.replace(/^lookup_/, '');
    var href = triggerLink.href;
    if (href.indexOf('?') === -1) href += '?';
    var win = window.open(href, 'lookup_' + inputId, 'height=500,width=800,resizable=yes,scrollbars=yes');
    if (win) {
        win.focus();
    }
    return false;
}

function dismissRelatedLookupPopup(win, chosenId) {
    var inputId = win.name.replace(/^lookup_/, '');
    var input = document.getElementById(inputId);
    if (input) {
        input.value = chosenId;
    }
    win.close();
}
