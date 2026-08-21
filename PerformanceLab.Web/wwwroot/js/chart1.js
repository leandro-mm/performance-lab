// chart1.js - Versão sem module (compatível com script tag normal)
let memoryChart = null;

function initializeMemoryChart(dotNetRef) {
    const canvas = document.getElementById('memoryChart');
    if (!canvas) {
        console.warn('Canvas memoryChart não encontrado');
        return;
    }
    
    const ctx = canvas.getContext('2d');
    
    memoryChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [
                {
                    label: 'Memory Used (MB)',
                    data: [],
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    tension: 0.1,
                    yAxisID: 'y'
                },
                {
                    label: 'CPU Usage (%)',
                    data: [],
                    borderColor: 'rgb(255, 99, 132)',
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    tension: 0.1,
                    yAxisID: 'y1'
                }
            ]
        },
        options: {
            responsive: true,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            plugins: {
                legend: {
                    position: 'top',
                }
            },
            scales: {
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                    title: {
                        display: true,
                        text: 'Memory (MB)'
                    }
                },
                y1: {
                    type: 'linear',
                    display: true,
                    position: 'right',
                    title: {
                        display: true,
                        text: 'CPU (%)'
                    },
                    grid: {
                        drawOnChartArea: false,
                    },
                    min: 0,
                    max: 100
                }
            }
        }
    });
    
    // Store reference for updates
    window.memoryChart = memoryChart;
    window.memoryChartDotNetRef = dotNetRef;
    console.log('✅ Chart inicializado');
}

function updateMemoryChart(labels, memoryData, cpuData) {
    if (!window.memoryChart) {
        console.warn('Chart não inicializado');
        return;
    }
    
    window.memoryChart.data.labels = labels;
    window.memoryChart.data.datasets[0].data = memoryData;
    window.memoryChart.data.datasets[1].data = cpuData;
    window.memoryChart.update();
    console.log('✅ Chart atualizado com', labels.length, 'pontos');
}

// Tornar funções globais
window.initializeMemoryChart = initializeMemoryChart;
window.updateMemoryChart = updateMemoryChart;