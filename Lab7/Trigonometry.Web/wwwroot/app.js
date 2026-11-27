const $ = id => document.getElementById(id);
for (let i = 0; i <= 12; i++) {
    const option = new Option(String(i), String(i));
    option.selected = i === 6;
    $('digits').add(option);
}
const number = (value, digits) => value === null ? 'не определён' : new Intl.NumberFormat('ru-RU', {
    minimumFractionDigits: digits,
    maximumFractionDigits: digits,
    useGrouping: false
}).format(value === 0 ? 0 : value);

function status(message, error = false) {
    $('status').textContent = message;
    $('status').classList.toggle('error', error);
}
async function request(url, options) {
    let response;
    try {
        response = await fetch(url, options);
    } catch {
        throw new Error('Нет соединения с сервером. Запустите приложение в терминале и обновите страницу.');
    }
    const body = await response.json();
    if (!response.ok) throw new Error(body.error || body.detail || 'Не удалось выполнить запрос. Проверьте параметры.');
    return body;
}
async function history() {
    const rows = await request('/api/history?storage=' + $('storage').value);
    const target = $('history');
    target.replaceChildren();
    if (!rows.length) {
        const p = document.createElement('p');
        p.className = 'empty';
        p.textContent = 'Здесь появятся последние 30 вычислений.';
        target.append(p);
        return;
    }
    const table = document.createElement('table');
    const head = document.createElement('thead');
    const header = document.createElement('tr');
    for (const label of ['Угол', 'Единицы', 'Знаки', 'sin', 'cos', 'tg', 'ctg']) {
        const th = document.createElement('th');
        th.textContent = label;
        header.append(th);
    }
    head.append(header);
    table.append(head);
    const body = document.createElement('tbody');
    for (const row of rows) {
        const tr = document.createElement('tr');
        for (const text of [row.angle, row.degrees ? 'градусы' : 'радианы', row.digits, ...['sin', 'cos', 'tan', 'cot'].map(key => number(row[key], row.digits))]) {
            const td = document.createElement('td');
            td.textContent = String(text);
            tr.append(td);
        }
        body.append(tr);
    }
    table.append(body);
    target.append(table);
}
$('form').addEventListener('submit', async event => {
    event.preventDefault();
    const raw = $('angle').value.trim();
    const angle = Number(raw.replace(',', '.'));
    if (!raw || !Number.isFinite(angle) || Math.abs(angle) > 1000000) {
        status('Введите угол от −1000000 до 1000000.', true);
        return;
    }
    $('calculate').disabled = true;
    status('Вычисляем…');
    try {
        const row = await request('/api/calculate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                angle,
                degrees: $('units').value === 'degrees',
                digits: Number($('digits').value),
                storage: $('storage').value
            })
        });
        for (const key of ['sin', 'cos', 'tan', 'cot']) $(key).textContent = number(row[key], row.digits);
        $('equation').textContent = row.angle + (row.degrees ? '°' : ' рад');
        status('Готово. Результат добавлен в историю.');
        await history();
    } catch (error) {
        status(error.message, true);
    } finally {
        $('calculate').disabled = false;
    }
});
$('storage').addEventListener('change', () => history().catch(error => status(error.message, true)));
$('refresh').addEventListener('click', () => history().catch(error => status(error.message, true)));
history().catch(error => status(error.message, true));
