const Dashboard = (() => {

    // ── State ─────────────────────────────────────────────────────
    const state = {
        charts: {},
        currency: { code: 'USD', locale: 'en-US' }
    };

    // ── DOM References ────────────────────────────────────────────
    const ui = {
        officeSelect: document.querySelector('select[name="OfficeId"]'),
        startDate: document.getElementById("StartDate"),
        endDate: document.getElementById("EndDate"),
    };

    // ── Currency ──────────────────────────────────────────────────
    const setCurrencyGlobals = () => {
        const selected = ui.officeSelect.options[ui.officeSelect.selectedIndex];
        state.currency.code = selected.getAttribute('data-currency');
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
        if (ui.startDate?.value) params.append('StartDate', ui.startDate.value);
        if (ui.endDate?.value) params.append('EndDate', ui.endDate.value);
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

        // Dynamic height for horizontal bar charts only - EXCEPT Top Sales People
        if (id !== 'topSalespeopleChart' && config.options?.indexAxis === 'y' && config.data.labels) {
            canvas.style.height = `${config.data.labels.length * 40}px`;
        }

        if (state.charts[id]) {
            const chart = state.charts[id];
            chart.data.labels = config.data.labels;
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
        document.getElementById('totalSalesToday').textContent = formatCurrency(data.totalSales);
        document.getElementById('orderCountToday').textContent = data.orderCount;
        document.getElementById('avgOrderValueToday').textContent = formatCurrency(data.averageOrderValue);
        document.getElementById('newCustomersToday').textContent = data.newCustomersCount;
    }

    // ── Sales Per Product Pie ─────────────────────────────────────
    async function loadSalesPie(mode = 'regular') {
        const data = await fetchData('SalesChart', { mode });
        if (!data) return;

        // Reset color map when switching modes to ensure fresh color assignment
        productColorMap.clear();
        colorIndex = 0;

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

        const canvas = document.getElementById('topSalespeopleChart');
        const referenceCanvas = document.getElementById('salesPerProductChart');
        if (canvas && referenceCanvas) {
            // Match height from Sales Per Products chart
            canvas.style.height = referenceCanvas.offsetHeight + 'px';
        }

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
                responsive: true,
                maintainAspectRatio: false, // Prevents automatic aspect ratio changes
                indexAxis: 'y',
                scales: {
                    x: {
                        beginAtZero: true,
                        suggestedMax: Math.max(...data.map(x => x.totalSales)) * 1.1 // Add 10% padding to max value
                    }
                },
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

    // ── Sales by Office Chart ─────────────────────────────────────
    async function loadSalesByOffice() {
        const data = await fetchData('SalesByOffice');
        if (!data) return;

        syncChart('salesByOfficeChart', {
            type: 'bar',
            data: {
                labels: data.map(x => x.officeName),
                datasets: [{
                    label: 'Total Sales',
                    data: data.map(x => x.totalSales),
                    backgroundColor: 'rgba(75, 192, 192, 0.6)'
                }]
            },
            options: {
                scales: { y: { beginAtZero: true } },
                plugins: { legend: { display: false } }
            }
        });
    }

    // ── Payment Methods Chart ─────────────────────────────────────
    async function loadPaymentMethods() {
        const data = await fetchData('PaymentMethods');
        if (!data) return;

        syncChart('paymentMethodsChart', {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.paymentMethod || 'Unknown'),
                datasets: [{
                    data: data.map(x => x.count),
                    backgroundColor: [
                        'rgba(255, 99, 132, 0.7)',
                        'rgba(54, 162, 235, 0.7)',
                        'rgba(255, 206, 86, 0.7)',
                        'rgba(75, 192, 192, 0.7)',
                        'rgba(153, 102, 255, 0.7)'
                    ]
                }]
            },
            options: {
                plugins: { legend: { position: 'right' } }
            }
        });
    }

    // ── Order Status Chart ────────────────────────────────────────
    async function loadOrderStatus() {
        const data = await fetchData('OrderStatus');
        if (!data) return;

        syncChart('orderStatusChart', {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.status),
                datasets: [{
                    data: data.map(x => x.count),
                    backgroundColor: [
                        'rgba(75, 192, 192, 0.7)',
                        'rgba(255, 99, 132, 0.7)'
                    ]
                }]
            },
            options: {
                plugins: { legend: { position: 'right' } }
            }
        });
    }

    // ── This Week vs Last Week ────────────────────────────────────
    async function loadThisWeekVsLastWeek() {
        const data = await fetchData('ThisWeekVsLastWeek');
        if (!data) return;

        document.getElementById('lastWeekSales').textContent = formatCurrency(data.lastWeek);
        document.getElementById('thisWeekSales').textContent = formatCurrency(data.thisWeek);
        const changeEl = document.getElementById('weekChange');
        const iconEl = document.getElementById('weekChangeIcon');
        const changePercent = data.change.toFixed(1);

        if (data.change >= 0) {
            changeEl.textContent = `+${changePercent}%`;
            changeEl.className = 'h4 fw-bold text-success';
            iconEl.innerHTML = '<i class="fas fa-arrow-up text-success"></i>';
        } else {
            changeEl.textContent = `${changePercent}%`;
            changeEl.className = 'h4 fw-bold text-danger';
            iconEl.innerHTML = '<i class="fas fa-arrow-down text-danger"></i>';
        }
    }

    // ── Low Stock Alerts ──────────────────────────────────────────
    async function loadLowStock() {
        const data = await fetchData('LowStock');
        const container = document.getElementById('lowStockAlerts');
        if (!data || !container) return;

        if (data.length === 0) {
            container.innerHTML = '<div class="list-group-item list-group-item-success"><span class="fw-bold">All products are well-stocked!</span></div>';
            return;
        }

        container.innerHTML = data.map(x => {
            const remaining = x.currentStock ?? 0;
            const badgeClass = remaining <= 10 ? 'bg-danger' : remaining <= 20 ? 'bg-warning' : 'bg-info';
            return `
                <div class="list-group-item">
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="fw-bold">${x.productName}</span>
                        <span class="badge ${badgeClass} rounded-pill">${remaining} left</span>
                    </div>
                </div>
            `;
        }).join('');
    }

    // ── Recent Orders Table ────────────────────────────────────────
    async function loadRecentOrders() {
        const data = await fetchData('RecentOrders');
        const tbody = document.querySelector('#recentOrdersTable tbody');
        if (!data || !tbody) return;

        tbody.innerHTML = data.map(row => `
            <tr>
                <td>#${row.id}</td>
                <td>${row.customerName || 'Walk-in'}</td>
                <td>${row.officeName || ''}</td>
                <td>${formatCurrency(row.amount)}</td>
                <td>${row.paymentMethod || 'N/A'}</td>
                <td>
                    <span class="badge ${row.status === 'Completed' ? 'bg-success' : 'bg-danger'} rounded-pill">
                        ${row.status}
                    </span>
                </td>
                <td>${new Date(row.date).toLocaleString()}</td>
            </tr>
        `).join('');
    }

    // ── Load All ──────────────────────────────────────────────────
    async function loadAll() {
        loadKpis();
        await loadSalesPie();
        loadSalespeople();
        loadTrend();
        loadSalesByOffice();
        loadPaymentMethods();
        loadOrderStatus();
        loadThisWeekVsLastWeek();
        loadLowStock();
        loadRecentOrders();
    }

    // ── Utilities ─────────────────────────────────────────────────
    function setActiveBtn(btn) {
        btn.parentNode.querySelectorAll('.btn').forEach(b => b.classList.remove('active', 'btn-dark'));
        btn.classList.add('active', 'btn-dark');
    }

    const PRODUCT_COLOR_PALETTE = [
        'rgba(75, 192, 192, 0.7)',    // Teal
        'rgba(255, 99, 132, 0.7)',    // Red
        'rgba(54, 162, 235, 0.7)',    // Blue
        'rgba(255, 159, 64, 0.7)',    // Orange
        'rgba(153, 102, 255, 0.7)',   // Purple
        'rgba(255, 205, 86, 0.7)',    // Yellow
        'rgba(201, 203, 207, 0.7)',   // Gray
        'rgba(0, 204, 150, 0.7)',     // Green
        'rgba(178, 113, 25, 0.7)',    // Brown
        'rgba(230, 126, 34, 0.7)',    // Amber
        'rgba(46, 204, 113, 0.7)',    // Emerald
        'rgba(155, 89, 182, 0.7)',    // Violet
        'rgba(52, 152, 219, 0.7)',    // Sky Blue
        'rgba(231, 76, 60, 0.7)',     // Red Orange
        'rgba(18, 137, 167, 0.7)',    // Cyan
        'rgba(244, 196, 48, 0.7)',    // Gold
        'rgba(93, 173, 226, 0.7)',    // Light Blue
        'rgba(253, 121, 168, 0.7)',   // Pink
        'rgba(174, 214, 241, 0.7)',   // Baby Blue
        'rgba(190, 144, 212, 0.7)'    // Lavender
    ];

    let colorIndex = 0;
    const productColorMap = new Map();

    function getProductColor(name) {
        const normalized = name.trim().toLowerCase();
        const defaultMap = {
            'cryptomonadales': 'rgba(0, 128, 0, 0.7)',
            'cleanse': 'rgba(255, 165, 0, 0.7)',
            'ppars plus': 'rgba(128, 0, 128, 0.7)',
            'coffee': 'rgba(139, 69, 19, 0.7)'
        };

        if (defaultMap[normalized]) {
            return defaultMap[normalized];
        }

        if (!productColorMap.has(normalized)) {
            const idx = colorIndex % PRODUCT_COLOR_PALETTE.length;
            productColorMap.set(normalized, PRODUCT_COLOR_PALETTE[idx]);
            colorIndex++;
        }

        return productColorMap.get(normalized);
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

        // Apply filters with date validation
        document.getElementById("generateReportBtn")?.addEventListener("click", () => {
            const startInput = ui.startDate;
            const endInput = ui.endDate;

            if (startInput?.value && endInput?.value) {
                const start = new Date(startInput.value + "T00:00:00");
                const end = new Date(endInput.value + "T00:00:00");

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