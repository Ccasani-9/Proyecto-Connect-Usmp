// USMP Connect — utilidades compartidas por todos los módulos.

/** Token antiforgery (lo trae cualquier <form method="post"> del layout). */
function usmpToken() {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    return input ? input.value : '';
}

/** POST con antiforgery. Uso: await usmpPost('/Modulo/Accion', { id: 3 }) */
async function usmpPost(url, data = {}) {
    const body = new URLSearchParams(data);
    body.append('__RequestVerificationToken', usmpToken());
    return fetch(url, { method: 'POST', body });
}

/** Muestra un toast. tipo: 'ok' | 'error' */
function usmpToast(mensaje, tipo = 'ok') {
    const div = document.createElement('div');
    div.className = 'toast-u' + (tipo === 'error' ? ' error' : '');
    div.innerHTML = `<i class="bi ${tipo === 'error' ? 'bi-exclamation-circle-fill' : 'bi-check-circle-fill text-success'}"></i>`;
    div.append(document.createTextNode(mensaje));
    document.body.append(div);
    setTimeout(() => div.remove(), 4000);
}

document.addEventListener('DOMContentLoaded', () => {
    // Toasts de TempData
    document.querySelectorAll('[data-autohide]').forEach(t => setTimeout(() => t.remove(), 4500));

    // Al abrir Noticias, marcar como leídas
    const noti = document.getElementById('notificaciones');
    if (noti) {
        noti.addEventListener('shown.bs.dropdown', () => {
            const badge = noti.querySelector('[data-noti-count]');
            if (badge) { badge.remove(); usmpPost('/Notificaciones/MarcarLeidas'); }
        }, { once: true });
    }

    // Zonas de subida: <label class="dropzone"><input type="file" hidden data-dropzone> ... <span data-dropzone-text></span></label>
    // Si el input tiene data-preview="#idImg", muestra la vista previa de la imagen.
    document.querySelectorAll('input[type=file][data-dropzone]').forEach(input => {
        const zona = input.closest('.dropzone');
        const texto = zona?.querySelector('[data-dropzone-text]');
        const mostrar = () => {
            const f = input.files[0];
            if (!f) return;
            if (texto) texto.textContent = `${f.name} (${(f.size / 1024 / 1024).toFixed(2)} MB)`;
            const prev = input.dataset.preview && document.querySelector(input.dataset.preview);
            if (prev && f.type.startsWith('image/')) { prev.src = URL.createObjectURL(f); prev.hidden = false; }
        };
        input.addEventListener('change', mostrar);
        zona?.addEventListener('dragover', e => { e.preventDefault(); zona.classList.add('border-danger'); });
        zona?.addEventListener('dragleave', () => zona.classList.remove('border-danger'));
        zona?.addEventListener('drop', e => {
            e.preventDefault(); zona.classList.remove('border-danger');
            input.files = e.dataTransfer.files; mostrar();
        });
    });

    // Filtros por chips: <div class="chips" data-filtro="grupo"><button class="chip" data-valor="x">
    // Filtra elementos con data-grupo="x" (valor "todo" muestra todo).
    document.querySelectorAll('.chips[data-filtro]').forEach(chips => {
        chips.addEventListener('click', e => {
            const chip = e.target.closest('.chip');
            if (!chip) return;
            chips.querySelectorAll('.chip').forEach(c => c.classList.toggle('active', c === chip));
            const grupo = chips.dataset.filtro, valor = chip.dataset.valor;
            document.querySelectorAll(`[data-${grupo}]`).forEach(el => {
                if (el.closest('.chips')) return;
                el.hidden = valor !== 'todo' && el.dataset[grupo] !== valor;
            });
        });
    });

    iniciarCalculadora();
});

function iniciarCalculadora() {
    const calc = document.getElementById('calc');
    if (!calc) return;
    const num = sel => { const v = calc.querySelector(sel).value; return v === '' ? null : Number(v); };
    const out = k => calc.querySelector(`[data-calc="${k}"]`);

    const recalcular = () => {
        const comp = ['pp', 'ep', 'ef'].map(k => ({ k, nota: num(`[data-nota="${k}"]`), peso: (num(`[data-peso="${k}"]`) ?? 0) / 100 }));
        const totalPeso = comp.reduce((s, c) => s + c.peso, 0);
        if (Math.abs(totalPeso - 1) > 0.001) {
            out('titulo').textContent = 'Revisa los pesos';
            out('valor').textContent = '—';
            out('detalle').textContent = `Los pesos suman ${Math.round(totalPeso * 100)}%, deben sumar 100%.`;
            return;
        }
        const ef = comp[2];
        const otros = comp.slice(0, 2);
        if (otros.some(c => c.nota === null)) {
            out('titulo').textContent = 'Promedio final';
            out('valor').textContent = '—';
            out('detalle').textContent = 'Completa PP y EP.';
            return;
        }
        const parcial = otros.reduce((s, c) => s + c.nota * c.peso, 0);
        if (ef.nota === null) {
            const necesita = (10.5 - parcial) / ef.peso;
            out('titulo').textContent = 'Necesitas en el examen final';
            if (necesita <= 0) { out('valor').textContent = '0'; out('detalle').textContent = '¡Ya aprobaste! 🎉'; }
            else if (necesita > 20) { out('valor').textContent = '> 20'; out('detalle').textContent = 'No alcanza solo con el EF. Consulta el sustitutorio.'; }
            else { out('valor').textContent = necesita.toFixed(1); out('detalle').textContent = 'para llegar a 10.5'; }
            return;
        }
        const pf = parcial + ef.nota * ef.peso;
        out('titulo').textContent = 'Promedio final';
        out('valor').textContent = pf.toFixed(1);
        out('detalle').textContent = pf >= 10.5 ? 'Aprobado ✅' : 'Desaprobado — nota mínima 10.5';
    };
    calc.addEventListener('input', recalcular);
}
