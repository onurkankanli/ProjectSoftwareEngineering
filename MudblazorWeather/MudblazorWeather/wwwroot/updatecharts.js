function updateLineChart(gTitle, Series1) {

    var options = {
        series:Series1,
        chart: {
            height: '150%',
            width: '100%',
            type: 'line',
            toolbar: {
                show: false
            }
        },
        dataLabels: {
            enabled: false,
        },
        stroke: {
            curve: 'smooth'
        },
        title: {
            text: gTitle,
            align: 'left'
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent'],
                opacity: 0.5
            },
        },
        markers: {
            size: 0
        },
        xaxis: {
            categories: " ",
            title: {
                text: ' '
            }
        },
        yaxis: {
            title: {
                text: gTitle
            }
        }
    };

    //follow the same tag id for your visualization
    var chart = new ApexCharts(document.querySelector("#chart"+gTitle), options);
    chart.render();
    chart.updateSeries(Series1, false);
}