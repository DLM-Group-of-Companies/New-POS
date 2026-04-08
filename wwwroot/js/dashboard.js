// ── Currency helpers ──────────────────────────────────────────────
let currencyCode;
let locale;

const officeSelect = document.querySelector('select[name="OfficeId"]');
const setCurrencyGlobals = () => {
    const selectedOption = officeSelect.options[officeSelect.selectedIndex];
    currencyCode = selectedOption.getAttribute('data-currency');
    locale = selectedOption.getAttribute('data-locale');
};

setCurrencyGlobals();

const formatCurrency = (value) => {
    return new Intl.NumberFormat(locale, {
        style: 'currency',
        currency: currencyCode,
        minimumFractionDigits: 2
    }).format(value);
};

officeSelect.addEventListener('change', setCurrencyGlobals);

function getSelectedOfficeSettings() {
    const office = document.querySelector('#OfficeId option:checked');
    return {
        locale: office.dataset.locale,
        currency: office.dataset.currency
    };
}

// ── Query params helper ───────────────────────────────────────────
function getQueryParams(extraParams = {}) {
    const startDate = document.getElementById("StartDate")?.value;
    const endDate   = document.getElementById("EndDate")?.value;
    const officeId  = document.getElementById("OfficeId")?.value;

    const params = new URLSearchParams();
    if (startDate) params.append("StartDate", startDate);
    if (endDate)   params.append("EndDate", endDate);
    if (officeId)  params.append("OfficeId", officeId);

    for (const [key, value] of Object.entries(extraParams)) {
        params.append(key, value);
    }
    return params.toString();
}

// ── Shared active button helper ───────────────────────────────────
function setActiveButton(button) {
    button.parentNode.querySelectorAll('.btn').forEach(btn => btn.classList.remove('active', 'btn-dark'));
    button.classList.add('active', 'btn-dark');
}

const toTitleCase = (str) => str.toLowerCase().replace(/\b\w/g, c => c.toUpperCase());

// ── KPIs ──────────────────────────────────────────────────────────
async function loadTopKpis() {
    try {
        const response = await fetch(`?handler=TopKpis&${getQueryParams()}`);
        const data = await response.json();
        document.getElementById('totalSalesToday').textContent  = formatCurrency(data.totalSales);
        document.getElementById('orderCountToday').textContent  = data.orderCount;
        document.getElementById('avgOrderValueToday').textContent = formatCurrency(data.averageOrderValue);
    } catch (err) {
        console.error('Failed to load KPIs:', err);
    }
}

// ── Top Customers Chart ───────────────────────────────────────────
let topCustomersChartInstance;

async function loadTopCustomersChart() {
    try {
        const response = await fetch(`?handler=TopCustomers&${getQueryParams()}`);
        const data = await response.json();
        const labels = data.map(x => x.customerName);
        const values = data.map(x => x.totalSpent);

        const chartEl = document.getElementById('topCustomersChart');
        chartEl.style.height = `${data.length * 40}px`;

        if (topCustomersChartInstance) topCustomersChartInstance.destroy();

        // Force layout flush then render on next frame
        void chartEl.parentElement.offsetHeight;
        requestAnimationFrame(() => {
            topCustomersChartInstance = new Chart(chartEl.getContext('2d'), {
                    type: 'bar',
                    data: {
                        labels,
                        datasets: [{
                            label: 'Total Spent',
                            data: values,
                            backgroundColor: 'rgba(54, 162, 235, 0.6)',
                            borderColor: 'rgba(54, 162, 235, 1)',
                            borderWidth: 1,
                            barPercentage: 0.7,
                            categoryPercentage: 0.8
                        }]
                    },
                    options: {
                        indexAxis: 'y',
                        responsive: true,
                        maintainAspectRatio: false,
                        interaction: { mode: 'nearest', intersect: true },
                        scales: { x: { beginAtZero: true } },
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                enabled: true,
                                callbacks: {
                                    label: ctx => {
                                        const settings = getSelectedOfficeSettings();
                                        return new Intl.NumberFormat(settings.locale, {
                                            style: 'currency',
                                            currency: settings.currency
                                        }).format(ctx.raw);
                                    }
                                }
                            }
                        },
                        animation: {
                            duration: 600,
                            easing: 'easeOutQuart'
                        }
                    }
                });
        });
    } catch (err) {
        console.error('Failed to load top customers chart:', err);
    }
}

// ── Repeat Customers Table ────────────────────────────────────────
async function loadRepeatCustomersTable() {
    try {
        const response = await fetch(`?handler=RepeatCustomers&${getQueryParams()}`);
        const data = await response.json();

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
    } catch (err) {
        console.error('Failed to load repeat customers:', err);
    }
}

// ── Sales Per Product Chart ───────────────────────────────────────
let productChart;

document.getElementById('btnRegular').addEventListener('click', function () {
    setActiveButton(this);
    loadSalesChart('regular');
});

document.getElementById('btnPromo').addEventListener('click', function () {
    setActiveButton(this);
    loadSalesChart('promo');
});

async function loadSalesChart(mode = 'regular') {
    try {
        const response = await fetch(`?handler=SalesChart&mode=${mode}&${getQueryParams()}`);
        const data = await response.json();
        const labels = data.map(x => x.productName);
        const sales  = data.map(x => x.totalSales);
        const backgroundColors = labels.map(name => getProductColor(name));

        if (productChart) productChart.destroy();

        void document.getElementById('salesPerProductChart').parentElement.offsetHeight;
        requestAnimationFrame(() => {
            productChart = new Chart(document.getElementById('salesPerProductChart'), {
                    type: 'pie',
                    data: {
                        labels,
                        datasets: [{ data: sales, backgroundColor: backgroundColors, borderColor: '#fff' }]
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { position: 'right' } },
                        animation: {
                            duration: 600,
                            easing: 'easeInOutCirc'
                        }
                    }
                });
        });
    } catch (err) {
        console.error("Failed to load sales chart:", err);
    }
}

function getProductColor(name) {
    switch (name.toLowerCase()) {
        case 'cryptomonadales': return 'rgba(0, 128, 0, 0.7)';
        case 'cleanse':        return 'rgba(255, 165, 0, 0.7)';
        case 'ppars plus':     return 'rgba(128, 0, 128, 0.7)';
        case 'coffee':         return 'rgba(139, 69, 19, 0.7)';
        default:               return getRandomColor();
    }
}

function getRandomColor() {
    const r = Math.floor(Math.random() * 155) + 100;
    const g = Math.floor(Math.random() * 155) + 100;
    const b = Math.floor(Math.random() * 155) + 100;
    return `rgba(${r}, ${g}, ${b}, 0.7)`;
}

// ── Top Salespeople Chart ─────────────────────────────────────────
let topSalespeopleChart = null;

document.getElementById('btnYearToDate').addEventListener('click', function () {
    setActiveButton(this);
    loadTopSalespeopleChart('YearToDate');
});

document.getElementById('btnMonthToDate').addEventListener('click', function () {
    setActiveButton(this);
    loadTopSalespeopleChart('MonthToDate');
});

async function loadTopSalespeopleChart(type = 'YearToDate') {
    try {
        const typeParam = type ? `type=${encodeURIComponent(type)}&` : "";
        const response  = await fetch(`?handler=TopSalespeople&${typeParam}${getQueryParams()}`);
        const data      = await response.json();
        const labels    = data.map(x => x.salesperson?.trim() || 'UNSPECIFIED');
        const values    = data.map(x => x.totalSales);

        if (topSalespeopleChart) topSalespeopleChart.destroy();

        const canvas = document.getElementById('topSalespeopleChart');
        canvas.style.height = `${data.length * 40}px`;

        void canvas.parentElement.offsetHeight;
        requestAnimationFrame(() => {
            topSalespeopleChart = new Chart(canvas, {
                    type: 'bar',
                    data: {
                        labels,
                        datasets: [{ data: values, backgroundColor: 'rgba(255, 159, 64, 0.6)' }]
                    },
                    options: {
                        indexAxis: 'y',
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: { x: { beginAtZero: true } },
                        plugins: {
                            legend: { display: false },
                            tooltip: { callbacks: { label: ctx => formatCurrency(ctx.raw) } }
                        },
                        animation: {
                            duration: 600,
                            easing: 'easeOutQuart'
                        }
                    }
                });
        });
    } catch (err) {
        console.error('Failed to load top salespeople chart:', err);
    }
}

// ── Sales Trend Chart ─────────────────────────────────────────────
let trendChart;

async function loadSalesTrend() {
    try {
        const response = await fetch(`?handler=SalesTrend&${getQueryParams()}`);
        const data     = await response.json();
        const labels   = data.map(x => x.date);
        const totals   = data.map(x => x.total);

        if (trendChart) trendChart.destroy();

        void document.getElementById("salesTrendChart").parentElement.offsetHeight;
        requestAnimationFrame(() => {
            trendChart = new Chart(document.getElementById("salesTrendChart"), {
                    type: 'line',
                    data: {
                        labels,
                        datasets: [{
                            label: 'Total Sales',
                            data: totals,
                            borderColor: 'rgb(126,239,203)',
                            backgroundColor: 'rgba(54, 162, 235, 0.2)',
                            tension: 0.4
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: { y: { beginAtZero: true } },
                        animation: {
                            duration: 600,
                            easing: 'easeInOutQuad'
                        }
                    }
                });
        });
    } catch (err) {
        console.error('Failed to load sales trend:', err);
    }
}

// ── Product Trend Chart ───────────────────────────────────────────
let productTrendChart;

async function loadProductTrend(year, type = 'Regular') {
    try {
        const res  = await fetch(`?handler=CountPerProduct&year=${year}&type=${type}`);
        const data = await res.json();

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

        if (productTrendChart) productTrendChart.destroy();

        void document.getElementById('productTrendChart').parentElement.offsetHeight;
        requestAnimationFrame(() => {
            productTrendChart = new Chart(document.getElementById('productTrendChart'), {
                    type: 'line',
                    data: { labels, datasets },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        interaction: { mode: 'index', intersect: false },
                        plugins: { title: { display: true, text: `Monthly Product Sales Trend (${type})` } },
                        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
                        animation: {
                            duration: 600,
                            easing: 'easeInOutQuad'
                        }
                    }
                });
        });
    } catch (err) {
        console.error('Failed to load product trend:', err);
    }
}

// ── Staff Customer Products Table ─────────────────────────────────
async function loadStaffCustomerProducts() {
    try {
        const response = await fetch(`?handler=StaffCustomerProducts&${getQueryParams()}`);
        const data = await response.json();

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
    } catch (err) {
        console.error('Failed to load staff purchases:', err);
    }
}

// ── Initialization ────────────────────────────────────────────────
document.addEventListener("DOMContentLoaded", () => {

    // Product trend toggle buttons
    const btnProdRegular = document.getElementById("btnProdRegular");
    const btnProdPromo   = document.getElementById("btnProdPromo");

    btnProdRegular.addEventListener("click", () => {
        setActiveButton(btnProdRegular);
        loadProductTrend(new Date().getFullYear(), "Regular");
    });

    btnProdPromo.addEventListener("click", () => {
        setActiveButton(btnProdPromo);
        loadProductTrend(new Date().getFullYear(), "Promo");
    });

    // Apply Filters button
    document.getElementById("generateReportBtn")?.addEventListener("click", () => {
        const startInput = document.getElementById("StartDate");
        const endInput   = document.getElementById("EndDate");

        const start = new Date(startInput.value + "T00:00:00");
        const end   = new Date(endInput.value + "T00:00:00");

        if (end < start) {
            const newStart = new Date(end.getFullYear(), end.getMonth(), 1);
            startInput.value = newStart.toISOString().split("T")[0];
        }

        loadAll();
    });

    // Initial load
    loadAll();
});

function loadAll() {
    loadTopKpis();
    loadTopCustomersChart();
    loadRepeatCustomersTable();
    loadSalesChart();
    loadTopSalespeopleChart();
    loadSalesTrend();
    loadStaffCustomerProducts();
    loadProductTrend(new Date().getFullYear(), "Regular");
}
