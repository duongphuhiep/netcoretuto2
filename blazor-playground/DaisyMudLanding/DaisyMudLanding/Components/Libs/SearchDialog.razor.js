function showModal5() {
    console.info('showModal5 is called');
    const dialog = document.getElementById('my_modal_5');
    if (dialog) {
        const searchInput = document.getElementById('searchInput');
        dialog.showModal();
        setTimeout(() => {
            searchInput.focus();
        }, 100);
    } else {
        console.warn('my_modal_5 dialog not found');
    }
}