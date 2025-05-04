function updateHeatmap(gTitle, Series) {

    var options = {
        series: Series,
        chart: {
            height: 350,
            type: 'heatmap',
        },
        dataLabels: {
            enabled: false
        },
        plotOptions: {
            heatmap: {
                colorScale: {
                    ranges: [{
                        from: -300,
                        to: -80,
                        color: '#FFFFFF',
                        name: 'low',
                    },
                        {
                            from: -80,
                            to: -40,
                            color: '#bcbcbc',
                            name: 'low-medium',
                        },
                        {
                            from: -40,
                            to: 0,
                            color: '#000000',
                            name: 'high',
                        }
                    ]
                }
            }
        },
        title: {
            text: 'HeatMap Chart'
        },
    };

    //follow the same tag id for your visualization
    var chart = new ApexCharts(document.querySelector("#chart"+gTitle), options);
    chart.render();
    chart.updateSeries(Series1, false);
}