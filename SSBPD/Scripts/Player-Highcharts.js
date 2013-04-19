var chart1; // globally available
var drawn = false;
function drawChart() {
    if (drawn) {
        return;
    }
    chart1 = new Highcharts.Chart({
        chart: {
            renderTo: 'eloHistory',
            type: 'line'
        },
        title: {
            text: 'Elo over time'
        },
        xAxis: {
            type: 'datetime'
        },
        yAxis: {
            title: {
                text: 'Elo'
            }
        },
        series: [{
            name: name,
            data: getData(eloData)
        }]
    });
    drawn = true;
}

function getData(eloData) {
    var data = eloData.data;
    var out = []
    var len = data.length;
    for (var i = 0; i < len; i++){
        var date = Date.UTC(data[i][0].year, data[i][0].month, data[i][0].day);
        out.push([date, data[i][1]]);
    }
    return out;
}