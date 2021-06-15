// Variables/objects from model
let airQualityForecasts = eval($('#forecastsSite').attr('airQualityForecasts'));
let forecastsDictionary = {}
let installationAddresses = {};
let chart = {}

for (let i = 0; i < airQualityForecasts.length; i++) {
  if (airQualityForecasts[i].length > 0) {
    installationAddresses[airQualityForecasts[i][0].InstallationId]
      = airQualityForecasts[i][0].InstallationAddress;
    forecastsDictionary[airQualityForecasts[i][0].InstallationId]
      = airQualityForecasts[i];
  }

  for (let j = 0; j < airQualityForecasts[i].length; j++) {
    let dateTime = new Date(airQualityForecasts[i][j].TillDateTime);
    let minutes = dateTime.getMinutes();
    minutes = minutes < 10 ? minutes = "0" + minutes : minutes;
    airQualityForecasts[i][j].TillDateTime
      = dateTime.getHours().toString() + ":" + minutes;
  }
}

$(document).ready(function () {
  chart = createForecastChart();
  updateInstallationsSelect();
});

$('#airQualityInstallations').change(function () {
  let selectedInstallationId = $(this).val();
  updateForecastChart(selectedInstallationId);
});

function updateInstallationsSelect() {
  let select = document.getElementById('airQualityInstallations');

  for (let installationId in installationAddresses) {
    let option = document.createElement("option");
    option.text = installationAddresses[installationId];
    option.value = installationId;
    select.add(option);
  }

  if (airQualityForecasts?.length > 0) {
    if (airQualityForecasts[0].length > 0) {
      select.value = airQualityForecasts[0][0].InstallationId;
    }
  }
}

function updateForecastChart(selectedInstallationId) {
  if (selectedInstallationId != 0) {
    chart.remove();
    chart = createForecastChart(selectedInstallationId);
  }
}

function createForecastChart(installationId) {
  let dataIsNotNullOrEmpty = airQualityForecasts != null
    && airQualityForecasts != undefined
    && airQualityForecasts.length != 0

  if (dataIsNotNullOrEmpty) {
    if (installationId == undefined) {
      installationId = airQualityForecasts[0][0].InstallationId;
    }

    const title = {
      text: "CAQI",
      fontSize: 13,
      x: -chartSize.margin.left + 25,
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
      .domain(d3.range(forecastsDictionary[installationId].length))
      .range([chartSize.margin.left, chartSize.width - chartSize.margin.right])
      .padding(0.1);

    let y = d3.scaleLinear()
      .domain([0, d3.max(forecastsDictionary[installationId], d => d.AirlyCaqi)])
      .nice()
      .range([chartSize.height - chartSize.margin.bottom, chartSize.margin.top]);

    let xAxis = g => g
      .attr("transform", `translate(0,${chartSize.height - chartSize.margin.bottom})`)
      .call(d3.axisBottom(x)
        .tickFormat(i => forecastsDictionary[installationId][i].TillDateTime)
        .tickSizeOuter(0));

    let yAxis = g => g
      .attr("transform", `translate(${chartSize.margin.left},0)`)
      .call(d3.axisLeft(y))
      .call(yTitle);

    const svg = d3.select("#mainDiv")
      .append("svg")
      .attr("viewBox", [0, 0, chartSize.width, chartSize.height]);

    svg.append("g")
      .selectAll("rect")
      .data(forecastsDictionary[installationId])
      .join("rect")
      .attr("x", (d, i) => x(i))
      .attr("y", d => y(d.AirlyCaqi))
      .attr("height", d => y(0) - y(d.AirlyCaqi))
      .attr("width", x.bandwidth())
      .attr("fill", d => getColorForCaqiRange(d.AirlyCaqi));

    svg.append("g")
      .call(xAxis);

    svg.append("g")
      .call(yAxis);

    return svg.node();
  }
}