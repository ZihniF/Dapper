(() => {
    if (typeof Chart === 'undefined') {
        document.querySelector('.chart-data').open = true;
        return;
    }

    const data = JSON.parse(document.getElementById('chart-data').textContent);

    Chart.defaults.font.family = 'Segoe UI, sans-serif';
    Chart.defaults.color = '#76878a';

    const context = document.getElementById('trendChart').getContext('2d');
    const gradient = context.createLinearGradient(0, 0, 0, 300);

    gradient.addColorStop(0, 'rgba(29, 143, 119, .24)');
    gradient.addColorStop(1, 'rgba(29, 143, 119, 0)');

    new Chart(context, {
        type: 'line',
        data: {
            labels: data.months.map(month => month.Label),
            datasets: [
                {
                    data: data.months.map(month => month.Value / 1000000),
                    borderColor: '#188b73',
                    backgroundColor: gradient,
                    fill: true,
                    borderWidth: 2,
                    pointRadius: 0,
                    pointHoverRadius: 4,
                    tension: .25
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: context => {
                            const value = context.parsed.y.toLocaleString('tr-TR', {
                                maximumFractionDigits: 2
                            });

                            return value + ' GWh';
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        maxTicksLimit: 7
                    },
                    border: {
                        display: false
                    }
                },
                y: {
                    grid: {
                        color: '#edf1ee'
                    },
                    border: {
                        display: false
                    }
                }
            }
        }
    });

    new Chart(document.getElementById('categoryChart'), {
        type: 'doughnut',
        data: {
            labels: data.categories.map(category => category.Label),
            datasets: [
                {
                    data: data.categories.map(category => category.Value),
                    backgroundColor: ['#128971', '#a9d2a2', '#e9bf61'],
                    borderWidth: 5,
                    borderColor: '#fff',
                    hoverOffset: 4
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '77%',
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: context => {
                            const value = (context.parsed / 1000000).toLocaleString('tr-TR', {
                                maximumFractionDigits: 2
                            });

                            return context.label + ': ' + value + ' GWh';
                        }
                    }
                }
            }
        }
    });
})();
