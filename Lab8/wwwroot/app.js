const $ = id => document.getElementById(id);
let active = null,
    last = null,
    timer = null;
const names = {
    warmup: 'Прогрев и подготовка данных',
    unique: 'Уникальные покупатели',
    report: 'Текстовый отчёт',
    sort: 'Сортировка сумм',
    totals: 'Расходы покупателей',
    done: 'Готово'
};
const fmt = (n, d = 2) => new Intl.NumberFormat('ru-RU', {
    maximumFractionDigits: d
}).format(n);
const bytes = n => n >= 1073741824 ? fmt(n / 1073741824) + ' ГиБ' : n >= 1048576 ? fmt(n / 1048576) + ' МиБ' : fmt(n / 1024) + ' КиБ';

function status(text, error = false) {
    $('status').textContent = text;
    $('status').classList.toggle('error', error);
}

function busy(value) {
    $('run').disabled = value;
    $('cancel').disabled = !value;
    $('count').disabled = value;
    $('repetitions').disabled = value;
    $('reference').disabled = value;
}
async function request(url, options) {
    let response;
    try {
        response = await fetch(url, options);
    } catch {
        throw new Error('Нет соединения с сервером. Запустите приложение в терминале и обновите страницу.');
    }
    const body = await response.json();
    if (!response.ok) throw new Error(body.error || body.detail || 'Не удалось выполнить запрос.');
    return body;
}

function render(result, reference = false) {
    last = result;
    $('progress').style.width = '100%';
    $('meter').setAttribute('aria-valuenow', 100);
    if (reference) {
        $('count').value = result.count;
        $('repetitions').value = result.repetitions;
    }
    $('before').textContent = fmt(result.beforeMs) + ' мс';
    $('after').textContent = fmt(result.afterMs) + ' мс';
    $('speedup').textContent = fmt(result.beforeMs / result.afterMs) + '×';
    $('source').textContent = (reference ? 'Сохранённый проверенный замер' : 'Новый замер') + ' · ' + fmt(result.count, 0) + ' покупок · ' + (result.repetitions === 3 ? 'медиана 3 повторений' : '1 повторение') + ' · ' + new Date(result.measuredAt).toLocaleString('ru-RU');
    $('verification').hidden = false;
    $('verification').classList.toggle('failed', !result.equal);
    $('verification').textContent = result.equal ? 'Результаты совпали на всех этапах: проверены списки, полный текст отчёта, все отсортированные суммы и расходы каждого покупателя.' : 'Результаты отличаются — оптимизация требует проверки.';
    const s = result.summary;
    $('summary').textContent = 'Покупателей: ' + fmt(s.uniqueBuyers, 0) + ' · Символов в отчёте: ' + fmt(s.reportLength, 0) + ' · Медианная сумма: ' + fmt(s.medianAmount, 0) + ' · Лидер: ' + s.topBuyer + ' (' + fmt(s.topTotal, 0) + ')';
    $('chart').replaceChildren();
    for (const stage of result.stages) {
        const row = document.createElement('div');
        row.className = 'chart-row';
        const label = document.createElement('span');
        label.textContent = stage.name;
        const bars = document.createElement('div');
        bars.className = 'bars';
        const max = Math.max(...result.stages.flatMap(item => [item.beforeMs, item.afterMs]), 0.0001);
        for (const [key, css] of [
                ['beforeMs', 'before'],
                ['afterMs', 'after']
            ]) {
            const bar = document.createElement('div');
            bar.className = 'bar ' + css;
            bar.style.width = Math.max(0.3, stage[key] / max * 100) + '%';
            bar.title = fmt(stage[key], 3) + ' мс';
            bars.append(bar);
        }
        row.append(label, bars);
        $('chart').append(row);
    }
    const table = document.createElement('table');
    const header = document.createElement('tr');
    for (const title of ['Этап', 'До, мс', 'После, мс', 'Ускорение', 'Память до', 'Память после', 'Изменение']) {
        const th = document.createElement('th');
        th.textContent = title;
        header.append(th);
    }
    const head = document.createElement('thead');
    head.append(header);
    table.append(head);
    const body = document.createElement('tbody');
    for (const stage of result.stages) {
        const tr = document.createElement('tr');
        for (const value of [stage.name, fmt(stage.beforeMs, 3), fmt(stage.afterMs, 3), fmt(stage.speedup) + '×', bytes(stage.beforeBytes), bytes(stage.afterBytes), stage.change]) {
            const td = document.createElement('td');
            td.textContent = value;
            tr.append(td);
        }
        body.append(tr);
    }
    table.append(body);
    $('table').replaceChildren(table);
    $('export').disabled = false;
}
async function poll() {
    if (!active) return;
    try {
        const snapshot = await request('/api/runs/' + active);
        $('progress').style.width = snapshot.progress + '%';
        $('meter').setAttribute('aria-valuenow', snapshot.progress);
        if (snapshot.state === 'running') {
            status((names[snapshot.stage] || snapshot.stage) + '…');
            timer = setTimeout(poll, 500);
            return;
        }
        active = null;
        busy(false);
        if (snapshot.state === 'completed') {
            render(snapshot.result);
            status(snapshot.result.equal ? 'Готово. Все результаты совпадают.' : 'Обнаружено различие результатов.', !snapshot.result.equal);
        } else if (snapshot.state === 'cancelled') status('Расчёт отменён. Можно изменить параметры и запустить снова.');
        else status(snapshot.error || 'Ошибка расчёта.', true);
    } catch (error) {
        active = null;
        busy(false);
        status(error.message + ' Проверьте, запущен ли сервер.', true);
    }
}
$('form').addEventListener('submit', async event => {
    event.preventDefault();
    const count = Number($('count').value);
    if (!Number.isInteger(count) || count < 100 || count > 100000) {
        status('Введите целое число покупок от 100 до 100000.', true);
        return;
    }
    busy(true);
    status('Запускаем сравнение…');
    try {
        const snapshot = await request('/api/runs', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                count,
                repetitions: Number($('repetitions').value)
            })
        });
        active = snapshot.id;
        await poll();
    } catch (error) {
        busy(false);
        status(error.message, true);
    }
});
$('cancel').addEventListener('click', async () => {
    if (!active) return;
    $('cancel').disabled = true;
    try {
        await request('/api/runs/' + active + '/cancel', {
            method: 'POST'
        });
        status('Отмена запрошена…');
    } catch (error) {
        status(error.message, true);
    }
});
$('reference').addEventListener('click', async () => {
    try {
        render(await request('/api/reference'), true);
        status('Открыт сохранённый замер. Для измерения на вашем Mac нажмите «Запустить сравнение».');
    } catch {
        status('Готовый замер пока не сохранён. Запустите сравнение.', true);
    }
});
$('export').addEventListener('click', () => {
    if (!last) return;
    const rows = [
        ['Этап', 'До, мс', 'После, мс', 'Ускорение', 'Память до, байт', 'Память после, байт', 'Результаты совпадают', 'Изменение'], ...last.stages.map(s => [s.name, s.beforeMs, s.afterMs, s.speedup, s.beforeBytes, s.afterBytes, s.equal, s.change])
    ];
    const csv = '\uFEFF' + rows.map(row => row.map(value => '"' + String(value).replaceAll('"', '""') + '"').join(';')).join('\n');
    const url = URL.createObjectURL(new Blob([csv], {
        type: 'text/csv;charset=utf-8'
    }));
    const a = document.createElement('a');
    a.href = url;
    a.download = 'lab8-measurements.csv';
    a.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
});
request('/api/active').then(payload => {
    const snapshot = payload.run;
    if (snapshot) {
        active = snapshot.id;
        busy(true);
        poll();
    }
}).catch(() => status('Не удалось связаться с сервером.', true));
