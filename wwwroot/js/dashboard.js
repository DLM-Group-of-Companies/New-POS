const Dashboard = (() => {

    // ── State ─────────────────────────────────────────────────────
    const state = {
        charts: {},
        currency: { code: 'USD', locale: 'en-US' }
    };

    // ── DOM References ────────────────────────────────────────────
    const ui = {
        officeSelect: document.querySelector('select[name="OfficeId"]'),
        startDate:    document.getElementById("StartDate"),
        endDate:      document.getElementById("EndDate"),
    };

    // ── Currency ──────────────────────────────────────────────────
    const setCurrencyGlobals = () => {
        const selected = ui.officeSelect.options[ui.officeSelect.selectedIndex];
        state.currency.code   = selected.getAttribute('data-currency');
        state.currency.locale = selected.getAttribute('data-locale');
    };

    const formatCurrency = (value) => {
        return new Intl.NumberFormat(state.currency.locale, {
            style: 'currency',
            currency: state.currency.code,
            minimumFractionDigits: 2
        }).format(value);
    };

    // ── Query Params ──────────────────────────────────────────────
    const getParams = (extra = {}) => {
        const params = new URLSearchParams();
        if (ui.startDate?.value)    params.append('StartDate', ui.startDate.value);
        if (ui.endDate?.value)      params.append('EndDate', ui.endDate.value);
        if (ui.officeSelect?.value) params.append('OfficeId', ui.officeSelect.value);
        Object.entries(extra).forEach(([k, v]) => params.append(k, v));
        return params.toString();
    };

    // ── Fetch Helper ──────────────────────────────────────────────
    async function fetchData(handler, extra = {}) {
        try {
            const response = await fetch(`?handler=${handler}&${getParams(extra)}`);
            if (!response.ok) throw new Error(`Fetch failed: ${response.status}`);
            return await response.json();
        } catch (err) {
            console.error(`Error loading ${handler}:`, err);
            return null;
        }
    }

    // ── Chart Engine ──────────────────────────────────────────────
    const ROUND_CHARTS = ['pie', 'doughnut'];

    function syncChart(id, config) {
        const canvas = document.getElementById(id);
        if (!canvas) return;

        const isRound = ROUND_CHARTS.includes(config.type);

        // Dynamic height for horizontal bar charts only
        if (config.options?.indexAxis === 'y' && config.data.labels) {
            canvas.style.height = `${config.data.labels.length * 40}px`;
        }

        if (state.charts[id]) {
            const chart = state.charts[id];
            chart.data.labels   = config.data.labels;
            chart.data.datasets = config.data.datasets;

            if (config.options?.plugins?.title) {
                chart.options.plugins.title = config.options.plugins.title;
            }

            chart.update('default');
        } else {
            state.charts[id] = new Chart(canvas, {
                type: config.type,
                data: config.data,
                options: {
                    responsive: true,
                    maintainAspectRatio: isRound,
                    animation: { duration: 700, easing: 'easeOutQuart' },
                    ...config.options
                }
            });
        }
    }

    // ── KPIs ──────────────────────────────────────────────────────
    async function loadKpis() {
        const data = await fetchData('TopKpis');
        if (!data) return;
        document.getElementById('totalSalesToday').textContent    = formatCurrency(data.totalSales);
        document.getElementById('orderCountToday').textContent    = data.orderCount;
        document.getElementById('avgOrderValueToday').textContent = formatCurrency(data.averageOrderValue);
    }

    // ── Top Customers Chart ───────────────────────────────────────
    async function loadCustomers() {
        const data = await fetchData('TopCustomers');
        if (!data) return;

        syncChart('topCustomersChart', {
            type: 'bar',
            data: {
                labels: data.map(x => x.customerName),
                datasets: [{
                    label: 'Total Spent',
                    data: data.map(x => x.totalSpent),
                    backgroundColor: 'rgba(54, 162, 235, 0.6)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1,
                    barPercentage: 0.7,
                    categoryPercentage: 0.8
                }]
            },
            options: {
                indexAxis: 'y',
                interaction: { mode: 'nearest', intersect: true },
                scales: { x: { beginAtZero: true } },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        enabled: true,
                        callbacks: { label: ctx => formatCurrency(ctx.raw) }
                    }
                }
            }
        });
    }

    // ── Repeat Customers Table ────────────────────────────────────
    async function loadRepeatCustomers() {
        const data = await fetchData('RepeatCustomers');
        if (!data) return;

        if ($.fn.DataTable.isDataTable('#repeatCustomersTable')) {
            $('#repeatCustomersTable').DataTable().clear().destroy();
        }

        const tbody = document.querySelector('#repeatCustomersTable tbody');
        tbody.innerHTML = data.map(row => `
            <tr>
                <td>${row.customerName}</td>
                <td>${row.orderCount}</td>
                <td>${new Date(row.lastOrder).toLocaleDateString()}</td>
            </tr>
        `).join('');

        $('#repeatCustomersTable').DataTable({
            order: [[1, 'desc']],
            pageLength: 10,
            lengthChange: false,
            searching: false,
            info: false
        });
    }

    // ── Sales Per Product Pie ─────────────────────────────────────
    async function loadSalesPie(mode = 'regular') {
        const data = await fetchData('SalesChart', { mode });
        if (!data) return;

        syncChart('salesPerProductChart', {
            type: 'pie',
            data: {
                labels: data.map(x => x.productName),
                datasets: [{
                    data: data.map(x => x.totalSales),
                    backgroundColor: data.map(x => getProductColor(x.productName)),
                    borderColor: '#fff'
                }]
            },
            options: {
                plugins: { legend: { position: 'right' } }
            }
        });
    }

    // ── Top Salespeople Chart ─────────────────────────────────────
    async function loadSalespeople(type = 'YearToDate') {
        const data = await fetchData('TopSalespeople', { type });
        if (!data) return;

        syncChart('topSalespeopleChart', {
            type: 'bar',
            data: {
                labels: data.map(x => x.salesperson?.trim() || 'UNSPECIFIED'),
                datasets: [{
                    data: data.map(x => x.totalSales),
                    backgroundColor: 'rgba(255, 159, 64, 0.6)'
                }]
            },
            options: {
                indexAxis: 'y',
                scales: { x: { beginAtZero: true } },
                plugins: {
                    legend: { display: false },
                    tooltip: { callbacks: { label: ctx => formatCurrency(ctx.raw) } }
                }
            }
        });
    }

    // ── Sales Trend Chart ─────────────────────────────────────────
    async function loadTrend() {
        const data = await fetchData('SalesTrend');
        if (!data) return;

        syncChart('salesTrendChart', {
            type: 'line',
            data: {
                labels: data.map(x => x.date),
                datasets: [{
                    label: 'Total Sales',
                    data: data.map(x => x.total),
                    borderColor: 'rgb(126,239,203)',
                    backgroundColor: 'rgba(126,239,203, 0.1)',
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                scales: { y: { beginAtZero: true } }
            }
        });
    }

    // ── Product Trend Chart ───────────────────────────────────────
    async function loadProductTrend(year = new Date().getFullYear(), type = 'Regular') {
        const data = await fetchData('CountPerProduct', { year, type });
        if (!data) return;

        const labels = [...new Set(data.map(d => `${d.year}-${String(d.month).padStart(2, '0')}`))];

        const products = {};
        data.forEach(d => {
            if (!products[d.productName]) {
                products[d.productName] = Array(labels.length).fill(0);
            }
            const idx = labels.indexOf(`${d.year}-${String(d.month).padStart(2, '0')}`);
            products[d.productName][idx] = d.totalQuantity;
        });

        const datasets = Object.entries(products).map(([name, counts], i) => ({
            label: name,
            data: counts,
            borderColor: `hsl(${i * 60}, 70%, 50%)`,
            backgroundColor: `hsl(${i * 60}, 70%, 50%)`,
            fill: false
        }));

        syncChart('productTrendChart', {
            type: 'line',
            data: { labels, datasets },
            options: {
                interaction: { mode: 'index', intersect: false },
                plugins: { title: { display: true, text: `Monthly Product Sales Trend (${type})` } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });
    }

    // ── Staff Purchases Table ─────────────────────────────────────
    async function loadStaffPurchases() {
        const data = await fetchData('StaffCustomerProducts');
        if (!data) return;

        if ($.fn.DataTable.isDataTable('#staffCustomerProductsTable')) {
            $('#staffCustomerProductsTable').DataTable().clear().destroy();
        }

        const tbody = document.querySelector('#staffCustomerProductsTable tbody');
        tbody.innerHTML = data.map(row => `
            <tr>
                <td>${toTitleCase(row.customerName)}</td>
                <td>${row.productName}</td>
                <td>${row.totalQuantity}</td>
                <td>${formatCurrency(row.totalAmount)}</td>
            </tr>
        `).join('');

        $('#staffCustomerProductsTable').DataTable({
            pageLength: 10,
            lengthChange: false,
            searching: false
        });
    }

    // ── Load All ──────────────────────────────────────────────────
    function loadAll() {
        loadKpis();
        loadCustomers();
        loadRepeatCustomers();
        loadSalesPie();
        loadSalespeople();
        loadTrend();
        loadStaffPurchases();
        loadProductTrend(new Date().getFullYear(), 'Regular');
    }

    // ── Utilities ─────────────────────────────────────────────────
    const toTitleCase = (str) => str.toLowerCase().replace(/\b\w/g, c => c.toUpperCase());

    function setActiveBtn(btn) {
        btn.parentNode.querySelectorAll('.btn').forEach(b => b.classList.remove('active', 'btn-dark'));
        btn.classList.add('active', 'btn-dark');
    }

    function getProductColor(name) {
        const map = {
            'cryptomonadales': 'rgba(0, 128, 0, 0.7)',
            'cleanse':         'rgba(255, 165, 0, 0.7)',
            'ppars plus':      'rgba(128, 0, 128, 0.7)',
            'coffee':          'rgba(139, 69, 19, 0.7)'
        };
        return map[name.toLowerCase()] || `hsla(${Math.random() * 360}, 60%, 50%, 0.7)`;
    }

    // ── Init ──────────────────────────────────────────────────────
    function init() {
        setCurrencyGlobals();

        // Office change
        ui.officeSelect?.addEventListener('change', () => {
            setCurrencyGlobals();
            loadAll();
        });

        // Sales pie toggles
        document.getElementById('btnRegular')?.addEventListener('click', function () {
            setActiveBtn(this);
            loadSalesPie('regular');
        });
        document.getElementById('btnPromo')?.addEventListener('click', function () {
            setActiveBtn(this);
            loadSalesPie('promo');
        });

        // Salespeople toggles
        document.getElementById('btnYearToDate')?.addEventListener('click', function () {
            setActiveBtn(this);
            loadSalespeople('YearToDate');
        });
        document.getElementById('btnMonthToDate')?.addEventListener('click', function () {
            setActiveBtn(this);
            loadSalespeople('MonthToDate');
        });

        // Product trend toggles
        document.getElementById('btnProdRegular')?.addEventListener('click', function () {
            setActiveBtn(this);
            loadProductTrend(new Date().getFullYear(), 'Regular');
        });
        document.getElementById('btnProdPromo')?.addEventListener('click', function () {
            setActiveBtn(this);
            loadProductTrend(new Date().getFullYear(), 'Promo');
        });

        // Apply filters with date validation
        document.getElementById("generateReportBtn")?.addEventListener("click", () => {
            const startInput = ui.startDate;
            const endInput   = ui.endDate;

            if (startInput?.value && endInput?.value) {
                const start = new Date(startInput.value + "T00:00:00");
                const end   = new Date(endInput.value + "T00:00:00");

                if (end < start) {
                    const newStart = new Date(end.getFullYear(), end.getMonth(), 1);
                    startInput.value = newStart.toISOString().split("T")[0];
                }
            }

            loadAll();
        });

        loadAll();
    }

    return { init };

})();

document.addEventListener("DOMContentLoaded", Dashboard.init);