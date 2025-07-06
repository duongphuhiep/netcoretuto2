/**
 * @param {HTMLDialogElement} element
 */
export function showModal(element) {
    if (!element) {
        console.warn('unable to showModal on non-exist element');
        return;
    }
    console.info('showModal begin');
    element.showModal();
    console.info('showModal end');
}

/**
 * @param {HTMLElement} element
 * @param {number|undefined} delay
 */
export function focus(element, delay = undefined) {
    if (!element) {
        console.warn('unable to focus on non-exist element');
        return;
    }
    console.info('focus begin');
    if (!delay) {
        element.focus();
        console.info('focus end');
        return;
    }
    setTimeout(() => {
        element.focus();
        console.info('focus end (delayed)', delay, element);
    }, delay);
}
