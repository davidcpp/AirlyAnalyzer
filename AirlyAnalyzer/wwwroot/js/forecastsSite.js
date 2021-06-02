// Variables/objects from model
let airQualityForecasts = eval($('#forecastsSite').attr('airQualityForecasts'));
let installationIds = {};

for (let i = 0; i < airQualityForecasts.length; i++) {
  if (airQualityForecasts[i].length > 0) {
    installationIds[airQualityForecasts[i][0].InstallationId] = true;
  }

  for (let j = 0; j < airQualityForecasts[i].length; j++) {
    let dateTime = new Date(airQualityForecasts[i][j].TillDateTime);
    let seconds = dateTime.getSeconds();
    seconds = seconds < 10 ? seconds = "0" + seconds : seconds;
    airQualityForecasts[i][j].TillDateTime
      = dateTime.getHours().toString() + ":" + seconds;
  }
}

$(document).ready(function () {
  let chart = createForecastChart();
  updateInstallationsSelect();
});

function updateInstallationsSelect() {
  let select = document.getElementById('airQualityInstallations');

  for (var installationId in installationIds) {
    let option = document.createElement("option");
    option.text = installationId;
    option.value = installationId;
    select.add(option);
  }
}

function createForecastChart() {
  let dataIsNotNullOrEmpty = airQualityForecasts != null
    && airQualityForecasts != undefined
    && airQualityForecasts.length != 0

  if (dataIsNotNullOrEmpty) {
    const chart = {
      margin: ({ top: 30, right: 0, bottom: 30, left: 40 }),
      height: 500,
      width: 900,
      color: "steelblue",
    };

    const title = {
      text: "CAQI",
      fontSize: 10,
      x: -chart.margin.left,
    };

    title.y = title.fontSize;

    let yTitle = g => g
      .append("text")
      .attr("x", title.x)
      .attr("y", title.y)
      .attr("fill", "currentColor")
      .attr("text-anchor", "start")
      .attr("font-family", "sans-serif")
      .attr("font-weight", "bold")
      .attr("font-size", title.fontSize)
      .text(title.text);

    let x = d3.scaleBand()
      .domain(d3.range(airQualityForecasts[0].length))
      .range([chart.margin.left, chart.width - chart.margin.right])
      .padding(0.1);

    let y = d3.scaleLinear()
      .domain([0, d3.max(airQualityForecasts[0], d => d.AirlyCaqi)]).nice()
      .range([chart.height - chart.margin.bottom, chart.margin.top]);

    let xAxis = g => g
      .attr("transform", `translate(0,${chart.height - chart.margin.bottom})`)
      .call(d3.axisBottom(x).tickFormat(i => airQualityForecasts[0][i].TillDateTime)
        .tickSizeOuter(0));

    let yAxis = g => g
      .attr("transform", `translate(${chart.margin.left},0)`)
      .call(d3.axisLeft(y))
      .call(yTitle);

    const svg = d3.select("#mainDiv")
      .append("svg")
      .attr("viewBox", [0, 0, chart.width, chart.height]);

    svg.append("g")
      .attr("fill", chart.color)
      .selectAll("rect")
      .data(dataIsNotNullOrEmpty ? airQualityForecasts[0] : [])
      .join("rect")
      .attr("x", (d, i) => x(i))
      .attr("y", d => y(d.AirlyCaqi))
      .attr("height", d => y(0) - y(d.AirlyCaqi))
      .attr("width", x.bandwidth());

    svg.append("g")
      .call(xAxis);

    svg.append("g")
      .call(yAxis);

    return svg.node();
  }
}