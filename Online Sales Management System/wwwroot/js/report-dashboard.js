/* File: wwwroot/js/report-dashboard.js */

function initComparisonChart(canvasId, labels, dataSales, dataPurchases) {
    const ctx = document.getElementById(canvasId);

    if (!ctx) return; // Nếu không tìm thấy canvas thì dừng

    new Chart(ctx.getContext('2d'), {
        type: 'bar', // Loại biểu đồ cột
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Revenue (Sales)',
                    data: dataSales,
                    backgroundColor: '#10b981', // Màu xanh
                    borderRadius: 4,
                    barPercentage: 0.6,
                    categoryPercentage: 0.8
                },
                {
                    label: 'Expenses (Purchases)',
                    data: dataPurchases,
                    backgroundColor: '#ef4444', // Màu đỏ
                    borderRadius: 4,
                    barPercentage: 0.6,
                    categoryPercentage: 0.8
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    labels: {
                        usePointStyle: true,
                        padding: 20,
                        font: { family: "'Segoe UI', sans-serif" }
                    }
                },
                tooltip: {
                    mode: 'index',
                    intersect: false,
                    backgroundColor: 'rgba(255, 255, 255, 0.9)',
                    titleColor: '#1a202c',
                    bodyColor: '#4a5568',
                    borderColor: '#e2e8f0',
                    borderWidth: 1,
                    callbacks: {
                        label: function (context) {
                            let label = context.dataset.label || '';
                            if (label) {
                                label += ': ';
                            }
                            if (context.parsed.y !== null) {
                                // Định dạng tiền tệ (ví dụ: 1,000,000)
                                label += new Intl.NumberFormat().format(context.parsed.y);
                            }
                            return label;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { color: '#f1f5f9', drawBorder: false },
                    ticks: { font: { size: 11 }, color: '#9ca3af' }
                },
                x: {
                    grid: { display: false },
                    ticks: { font: { size: 11 }, color: '#9ca3af' }
                }
            }
        }
    });
}