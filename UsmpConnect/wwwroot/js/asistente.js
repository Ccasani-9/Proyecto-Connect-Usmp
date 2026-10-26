(() => {
    'use strict';
    const panel = document.getElementById('fia-chat');
    if (!panel) return;
    const abrir = document.getElementById('fia-chat-abrir');
    const mensajes = document.getElementById('fia-chat-mensajes');
    const formulario = document.getElementById('fia-chat-form');
    const campo = document.getElementById('fia-chat-mensaje');
    const enviar = document.getElementById('fia-chat-enviar');
    const detener = document.getElementById('fia-chat-detener');
    const nueva = document.getElementById('fia-chat-nueva');
    const estado = document.getElementById('fia-chat-estado');
    const error = document.getElementById('fia-chat-error');
    const confirmacion = document.getElementById('fia-chat-confirmacion');
    const bienvenida = mensajes.firstElementChild.cloneNode(true);
    let controlador = null;
    let cargado = false;
    let cargando = false;
    let limpiando = false;

    function mostrarError(texto) {
        error.textContent = texto;
        error.hidden = !texto;
    }
    function ocupado(valor) {
        enviar.disabled = valor;
        nueva.disabled = valor;
        detener.hidden = !valor;
        campo.readOnly = valor;
        estado.textContent = valor ? 'Preparando respuesta…' : '';
        formulario.setAttribute('aria-busy', String(valor));
    }
    function alFinal() { return mensajes.scrollHeight - mensajes.scrollTop - mensajes.clientHeight < 90; }
    function desplazar(seguir) { if (seguir) mensajes.scrollTop = mensajes.scrollHeight; }
    function agregar(rol, texto) {
        mensajes.querySelector('.fia-chat-bienvenida')?.remove();
        const articulo = document.createElement('article');
        articulo.className = `fia-chat-mensaje ${rol === 'usuario' ? 'fia-chat-usuario' : 'fia-chat-asistente'}`;
        const autor = document.createElement('strong');
        autor.className = 'fia-chat-autor';
        autor.textContent = rol === 'usuario' ? 'Tú' : 'Asistente FIA';
        const cuerpo = document.createElement('div');
        cuerpo.className = 'fia-chat-texto';
        cuerpo.textContent = texto;
        articulo.append(autor, cuerpo);
        mensajes.append(articulo);
        return { articulo, cuerpo };
    }
    function completar(articulo, tarjetas = [], fuentes = []) {
        if (tarjetas.length) {
            const lista = document.createElement('div');
            lista.className = 'fia-chat-tarjetas';
            lista.setAttribute('aria-label', 'Resultados encontrados');
            for (const tarjeta of tarjetas.slice(0, 8)) {
                if (typeof tarjeta.url !== 'string' || !/^\/(?!\/)/.test(tarjeta.url)) continue;
                const enlace = document.createElement('a');
                enlace.className = 'fia-chat-tarjeta';
                enlace.href = tarjeta.url;
                const tipo = document.createElement('small');
                tipo.textContent = tarjeta.tipo;
                const titulo = document.createElement('strong');
                titulo.textContent = tarjeta.titulo;
                const detalle = document.createElement('span');
                detalle.textContent = tarjeta.detalle;
                enlace.append(tipo, titulo, detalle);
                lista.append(enlace);
            }
            articulo.append(lista);
        }
        for (const fuente of fuentes.slice(0, 5)) {
            const detalle = document.createElement('details');
            detalle.className = 'fia-chat-fuente';
            const titulo = document.createElement('summary');
            titulo.textContent = `Fuente: ${fuente.fuente}${fuente.pagina ? ` · página ${fuente.pagina}` : ''}`;
            const texto = document.createElement('p');
            texto.textContent = fuente.texto;
            detalle.append(titulo, texto);
            articulo.append(detalle);
        }
    }
    async function respuestaValida(respuesta) {
        if (respuesta.redirected || respuesta.status === 401) throw new Error('Tu sesión terminó. Inicia sesión de nuevo.');
        if (!respuesta.ok) {
            const datos = await respuesta.json().catch(() => null);
            throw new Error(datos?.error || 'No se pudo completar la solicitud. Recarga la página e inténtalo de nuevo.');
        }
    }
    async function historial() {
        if (cargado || cargando) return;
        cargando = true;
        enviar.disabled = true;
        nueva.disabled = true;
        estado.textContent = 'Cargando conversación…';
        try {
            const respuesta = await fetch('/Asistente/Historial', { cache: 'no-store' });
            await respuestaValida(respuesta);
            const datos = await respuesta.json();
            if (datos.mensajes.length) mensajes.replaceChildren();
            for (const mensaje of datos.mensajes) {
                const fila = agregar(mensaje.rol, mensaje.texto);
                completar(fila.articulo, mensaje.tarjetas || [], mensaje.fuentes || []);
            }
            cargado = true;
            if (!datos.configurado) mostrarError('El asistente todavía no está disponible. Puedes utilizar el buscador de la plataforma.');
            desplazar(true);
        } catch (ex) { mostrarError(ex.message); }
        finally {
            cargando = false;
            enviar.disabled = false;
            nueva.disabled = false;
            estado.textContent = '';
        }
    }
    abrir.addEventListener('click', () => {
        panel.hidden = !panel.hidden;
        abrir.setAttribute('aria-expanded', String(!panel.hidden));
        if (!panel.hidden) { historial(); campo.focus(); }
    });
    function cerrar() {
        panel.hidden = true;
        abrir.setAttribute('aria-expanded', 'false');
        abrir.focus();
    }
    document.getElementById('fia-chat-cerrar').addEventListener('click', cerrar);
    panel.addEventListener('keydown', event => {
        if (event.key === 'Escape') { event.stopPropagation(); cerrar(); }
    });
    mensajes.addEventListener('click', event => {
        const sugerencia = event.target.closest('[data-fia-pregunta]');
        if (!sugerencia || controlador || cargando || limpiando) return;
        campo.value = sugerencia.dataset.fiaPregunta;
        campo.focus();
    });
    detener.addEventListener('click', () => controlador?.abort());
    nueva.addEventListener('click', () => {
        confirmacion.hidden = false;
        document.getElementById('fia-chat-cancelar-limpieza').focus();
    });
    document.getElementById('fia-chat-cancelar-limpieza').addEventListener('click', () => {
        confirmacion.hidden = true;
        nueva.focus();
    });
    document.getElementById('fia-chat-confirmar-limpieza').addEventListener('click', async event => {
        if (controlador || cargando || limpiando) return;
        limpiando = true;
        enviar.disabled = true;
        nueva.disabled = true;
        campo.readOnly = true;
        event.target.disabled = true;
        try {
            await respuestaValida(await usmpPost('/Asistente/Limpiar'));
            mensajes.replaceChildren(bienvenida.cloneNode(true));
            confirmacion.hidden = true;
            mostrarError('');
            campo.value = '';
            campo.focus();
        } catch (ex) { mostrarError(ex.message); }
        finally {
            limpiando = false;
            enviar.disabled = false;
            nueva.disabled = false;
            campo.readOnly = false;
            event.target.disabled = false;
        }
    });
    formulario.addEventListener('submit', async event => {
        event.preventDefault();
        if (controlador || cargando || limpiando) return;
        const pregunta = campo.value.trim();
        if (!pregunta || pregunta.length > 1000) {
            mostrarError('Escribe un mensaje de entre 1 y 1.000 caracteres.');
            campo.setAttribute('aria-invalid', 'true');
            campo.focus();
            return;
        }
        campo.removeAttribute('aria-invalid');
        mostrarError('');
        confirmacion.hidden = true;
        controlador = new AbortController();
        ocupado(true);
        agregar('usuario', pregunta);
        const fila = agregar('asistente', '');
        campo.value = '';
        desplazar(true);
        let termino = false;
        try {
            const body = new URLSearchParams({ mensaje: pregunta, __RequestVerificationToken: usmpToken() });
            const respuesta = await fetch('/Asistente/Preguntar', { method: 'POST', body, signal: controlador.signal });
            await respuestaValida(respuesta);
            if (!respuesta.body || !respuesta.headers.get('content-type')?.includes('application/x-ndjson')) throw new Error('No se pudo leer la respuesta.');
            const lector = respuesta.body.getReader();
            const decoder = new TextDecoder();
            let pendiente = '';
            function procesar(linea) {
                if (!linea.trim()) return;
                const dato = JSON.parse(linea);
                const seguir = alFinal();
                if (dato.tipo === 'texto') {
                    fila.cuerpo.textContent += dato.texto;
                    estado.textContent = 'Respondiendo…';
                } else if (dato.tipo === 'fin') {
                    completar(fila.articulo, dato.tarjetas, dato.fuentes);
                    termino = true;
                } else if (dato.tipo === 'error') throw new Error(dato.error);
                desplazar(seguir);
            }
            while (true) {
                const { done, value } = await lector.read();
                pendiente += decoder.decode(value || new Uint8Array(), { stream: !done });
                let salto;
                while ((salto = pendiente.indexOf('\n')) >= 0) {
                    procesar(pendiente.slice(0, salto));
                    pendiente = pendiente.slice(salto + 1);
                }
                if (done) break;
            }
            procesar(pendiente);
            if (!termino) throw new Error('La respuesta se interrumpió. Inténtalo de nuevo.');
        } catch (ex) {
            controlador.abort();
            fila.articulo.classList.add('fia-chat-incompleto');
            if (!fila.cuerpo.textContent) fila.cuerpo.textContent = 'No se generó una respuesta.';
            const aviso = document.createElement('small');
            aviso.textContent = 'Respuesta incompleta';
            fila.articulo.append(aviso);
            mostrarError(ex.name === 'AbortError' ? 'Respuesta detenida. Los mensajes incompletos no se conservan al recargar.' : ex.message);
            if (!campo.value) campo.value = pregunta;
        } finally {
            controlador = null;
            ocupado(false);
            if (termino) estado.textContent = 'Respuesta lista.';
            if (!panel.hidden) campo.focus();
        }
    });
})();
