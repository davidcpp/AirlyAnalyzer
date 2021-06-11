﻿const caqiRanges = {
  veryLow: {
    max: 25,
    color: "limegreen",
  },
  low: {
    max: 50,
    color: "yellowgreen",
  },
  medium: {
    max: 75,
    color: "orange",
  },
  high: {
    max: 100,
    color: "orangered",
  },
  veryHigh: {
    max: 125,
    color: "mediumvioletred",
  },
  extreme: {
    color: "purple",
  },
}

// Variables/objects from model
let airQualityForecasts = eval($('#forecastsSite').attr('airQualityForecasts'));
let forecastsDictionary = {}
let installationAddresses = {};

let charts = {}

for (let i = 0; i < airQualityForecasts.length; i++) {
  if (airQualityForecasts[i].length > 0) {
    let installationId = airQualityForecasts[i][0].InstallationId;

    installationAddresses[installationId]
      = airQualityForecasts[i][0].InstallationAddress;

    const forecastsBySource = airQualityForecasts[i].reduce(
      (forecastsBySource, item) => {
        const group = (forecastsBySource[item.Source] || []);
        group.push(item);
        forecastsBySource[item.Source] = group;
        return forecastsBySource;
      }, {});

    forecastsDictionary[installationId] = forecastsBySource;
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
  let firstInstallationId = airQualityForecasts[0][0]?.InstallationId;

  for (var source in forecastsDictionary[firstInstallationId]) {
    charts[source] = {};
    charts[source] = createForecastChart(source);
  }

  updateInstallationsSelect();
});

$('#airQualityInstallations').change(function () {
  let selectedInstallationId = $(this).val();
  updateForecastCharts(selectedInstallationId);
});

function updateInstallationsSelect() {
  let select = document.getElementById('airQualityInstallations');

  for (var installationId in installationAddresses) {
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

function updateForecastCharts(selectedInstallationId) {
  for (var source in forecastsDictionary[selectedInstallationId]) {
    if (selectedInstallationId != 0) {
      charts[source].remove();
      charts[source] = createForecastChart(source, selectedInstallationId);
    }
  }
}

function createForecastChart(source, installationId) {
  let dataIsNotNullOrEmpty = airQualityForecasts != null
    && airQualityForecasts != undefined
    && airQualityForecasts.length != 0

  if (dataIsNotNullOrEmpty) {
    if (installationId == undefined) {
      installationId = airQualityForecasts[0][0].InstallationId;
    }

    const chart = {
      margin: ({ top: 30, right: 60, bottom: 30, left: 60 }),
      height: 500,
      width: 900,
    };

    const title = {
      text: "CAQI",
      fontSize: 13,
      x: -chart.margin.left + 25,
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
      .domain(d3.range(forecastsDictionary[installationId][source].length))
      .range([chart.margin.left, chart.width - chart.margin.right])
      .padding(0.1);

    let y = d3.scaleLinear()
      .domain(
        [0, d3.max(forecastsDictionary[installationId][source], d => d.AirlyCaqi)])
      .nice()
      .range([chart.height - chart.margin.bottom, chart.margin.top]);

    let xAxis = g => g
      .attr("transform", `translate(0,${chart.height - chart.margin.bottom})`)
      .call(d3.axisBottom(x)
        .tickFormat(i => forecastsDictionary[installationId][source][i].TillDateTime)
        .tickSizeOuter(0));

    let yAxis = g => g
      .attr("transform", `translate(${chart.margin.left},0)`)
      .call(d3.axisLeft(y))
      .call(yTitle);

    const chartDiv = d3.select("#mainDiv")
      .append("div")
      .attr("id", source)
      .attr("class", "col-12 col-sm-12 col-md-6 mb-5")

    const svg = chartDiv
      .append("svg")
      .attr("viewBox", [0, 0, chart.width, chart.height]);

    svg.append("g")
      .selectAll("rect")
      .data(forecastsDictionary[installationId][source])
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

    return chartDiv.node();
  }
}

function getColorForCaqiRange(caqi) {
  switch (true) {
    case (caqi <= caqiRanges.veryLow.max):
      return caqiRanges.veryLow.color;
      break;
    case (caqi <= caqiRanges.low.max):
      return caqiRanges.low.color;
      break;
    case (caqi <= caqiRanges.medium.max):
      return caqiRanges.medium.color;
      break;
    case (caqi <= caqiRanges.high.max):
      return caqiRanges.high.color;
      break;
    case (caqi <= caqiRanges.veryHigh.max):
      return caqiRanges.veryHigh.color;
      break;
    default:
      return caqiRanges.extreme.color;
      break;
  }
}