// Estado compartido de los formularios de acceso y cierre de sesión.
document.querySelectorAll('form[data-acceso]').forEach(form => {
    const button = form.querySelector('button[type="submit"]');
    const status = form.querySelector('[data-estado-acceso]');
    form.addEventListener('submit', event => {
        if (form.getAttribute('aria-busy') === 'true') {
            event.preventDefault();
            return;
        }
        if (!navigator.onLine) {
            event.preventDefault();
            status.textContent = 'Revisa tu conexión a internet y vuelve a intentarlo.';
            return;
        }
        if (window.jQuery && jQuery(form).data('validator') && !jQuery(form).valid()) return;
        form.setAttribute('aria-busy', 'true');
        button.disabled = true;
        status.textContent = 'Conectando… Si el sitio estaba en reposo, puede tardar un momento.';
    });
    window.addEventListener('pageshow', () => {
        form.removeAttribute('aria-busy');
        button.disabled = false;
        status.textContent = '';
    });
});
