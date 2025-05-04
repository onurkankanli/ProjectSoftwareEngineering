function generateHeatmap(gTitle, Series) {

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
                        color: '#FF001B',
                        name: 'Low',
                    },
                        {
                            from: -80,
                            to: -40,
                            color: '#FFB200',
                            name: 'Low-medium',
                        },
                        {
                            from: -40,
                            to: -1,
                            color: '#3AFF00',
                            name: 'High',
                        },
                        {
                            from: -1,
                            to: 0,
                            color: '#F6F6F6',
                            name: 'No data',
                        }
                    ]
                }
            }
        },
        title: {
            text: 'Signal Strength Chart'
        },
    };

    //follow the same tag id for your visualization
    var chart = new ApexCharts(document.querySelector("#chart"+gTitle), options);

    chart.render();
}