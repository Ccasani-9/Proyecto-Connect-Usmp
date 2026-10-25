(() => {
    const input = document.getElementById('busquedaGlobal');
    if (!input) return;
    const root = input.closest('.busqueda-global');
    const panel = document.getElementById('busquedaResultados');
    const lista = document.getElementById('busquedaLista');
    const estado = document.getElementById('busquedaEstado');
    const limpiar = document.getElementById('busquedaLimpiar');
    let timer, request, version = 0, composing = false;
    const cerrar = () => { panel.hidden = true; input.setAttribute('aria-expanded', 'false'); };
    const mostrar = mensaje => { panel.hidden = false; input.setAttribute('aria-expanded', 'true'); estado.textContent = mensaje; };
    function cancelar() { clearTimeout(timer); request?.abort(); version++; }
    async function buscar(q, actual) {
        request = new AbortController();
        mostrar('Buscando…');
        lista.replaceChildren();
        try {
            const response = await fetch('/Buscar/Sugerencias?q=' + encodeURIComponent(q), { signal: request.signal });
            if (!response.ok) throw new Error('Búsqueda no disponible');
            const items = await response.json();
            if (actual !== version) return;
            mostrar(items.length ? `${items.length} resultados. Usa Tab o la flecha hacia abajo para explorarlos.` : 'No encontramos resultados. Prueba con otra palabra.');
            const grupos = new Map();
            for (const item of items) {
                if (!grupos.has(item.tipo)) {
                    const titulo = document.createElement('h2');
                    titulo.className = 'busqueda-grupo'; titulo.textContent = item.tipo;
                    lista.append(titulo);
                    const grupo = document.createElement('div'); lista.append(grupo); grupos.set(item.tipo, grupo);
                }
                // Los enlaces los decide el servidor; solo se admiten rutas internas.
                if (typeof item.url !== 'string' || !item.url.startsWith('/') || item.url.startsWith('//')) continue;
                const enlace = document.createElement('a'); enlace.className = 'busqueda-item'; enlace.href = item.url;
                const titulo = document.createElement('strong'); titulo.textContent = item.titulo;
                const detalle = document.createElement('span'); detalle.textContent = item.descripcion;
                enlace.append(titulo, detalle); grupos.get(item.tipo).append(enlace);
            }
        } catch (error) {
            if (actual !== version || error.name === 'AbortError') return;
            lista.replaceChildren(); mostrar('No pudimos cargar las sugerencias. Presiona Buscar para intentarlo nuevamente.');
        }
    }
    function actualizar() {
        cancelar(); limpiar.hidden = !input.value;
        const q = input.value.trim();
        lista.replaceChildren(); cerrar();
        if (composing || q.length < 2) return;
        const actual = version;
        timer = setTimeout(() => buscar(q, actual), 250);
    }
    input.addEventListener('input', actualizar);
    input.addEventListener('compositionstart', () => { composing = true; cancelar(); });
    input.addEventListener('compositionend', () => { composing = false; actualizar(); });
    limpiar.hidden = !input.value;
    limpiar.addEventListener('click', () => { input.value = ''; actualizar(); input.focus(); });
    root.addEventListener('keydown', event => {
        if (event.key === 'Escape') { cancelar(); cerrar(); input.focus(); }
        if (panel.hidden || !['ArrowDown', 'ArrowUp'].includes(event.key)) return;
        const enlaces = [...lista.querySelectorAll('a')]; if (!enlaces.length) return;
        event.preventDefault(); const current = enlaces.indexOf(document.activeElement);
        const next = current < 0 ? (event.key === 'ArrowDown' ? 0 : enlaces.length - 1)
            : (current + (event.key === 'ArrowDown' ? 1 : -1) + enlaces.length) % enlaces.length;
        enlaces[next].focus();
    });
    document.addEventListener('click', event => { if (!root.contains(event.target)) { cancelar(); cerrar(); } });
    root.querySelector('form').addEventListener('submit', event => { if (composing) event.preventDefault(); else cancelar(); });
})();
