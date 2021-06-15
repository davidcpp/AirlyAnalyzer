const caqiRanges = {
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

let forecastHoursNumber = 24;

// Variables/objects from model
let airQualityForecasts = eval($('#forecastsSite').attr('airQualityForecasts'));
let forecastsDictionary = {}
let installationAddresses = {};
let forecastDates = [];

let charts = {}

$(document).ready(function () {
  fillNextForecastDates();
  initInstallationAddresses();
  updateInstallationsSelect();
  initForecastsDictionary();
  createInitForecastCharts();
});

$('#airQualityInstallations').change(function () {
  let selectedInstallationId = $(this).val();
  updateForecastCharts(selectedInstallationId);
});

function fillNextForecastDates() {
  let initDate = new Date();
  initDate.setHours(initDate.getHours() + 1, 0, 0, 0);

  for (let i = 0; i < forecastHoursNumber; i++) {
    let currentDate = new Date(initDate);
    currentDate.setHours(currentDate.getHours() + i)
    forecastDates.push(currentDate);
  }
}

function initInstallationAddresses() {
  for (let i = 0; i < airQualityForecasts?.length; i++) {
    let installationForecasts = airQualityForecasts[i];

    if (installationForecasts?.length > 0) {
      let installationId = installationForecasts[0]?.InstallationId;

      installationAddresses[installationId]
        = installationForecasts[0]?.InstallationAddress;
    }
  }
}

function updateInstallationsSelect() {
  let select = document.getElementById('airQualityInstallations');

  for (let installationId in installationAddresses) {
    let option = document.createElement("option");
    option.text = installationAddresses[installationId];
    option.value = installationId;
    select.add(option);
  }

  if (airQualityForecasts?.length > 0) {
    if (airQualityForecasts[0]?.length > 0) {
      select.value = airQualityForecasts[0][0]?.InstallationId;
    }
  }
}

function initForecastsDictionary() {
  for (let i = 0; i < airQualityForecasts?.length; i++) {
    let installationForecasts = airQualityForecasts[i];

    if (installationForecasts?.length > 0) {
      let installationId = installationForecasts[0]?.InstallationId;

      const forecastsBySource = installationForecasts.reduce(
        (forecastsBySource, item) => {
          const group = (forecastsBySource[item.Source] || []);
          group.push(item);
          forecastsBySource[item.Source] = group;
          return forecastsBySource;
        }, {});

      forecastsDictionary[installationId] = forecastsBySource;

      for (let forecastSource in forecastsDictionary[installationId]) {
        let forecast = forecastsDictionary[installationId][forecastSource];

        if (forecast.length != forecastHoursNumber) {
          matchForecastToChartScale(forecast);
        }
      }
    }

    for (let j = 0; j < installationForecasts?.length; j++) {
      let dateTime = new Date(installationForecasts[j].TillDateTime);
      let seconds = dateTime.getSeconds();
      seconds = seconds < 10 ? seconds = "0" + seconds : seconds;
      installationForecasts[j].TillDateTime
        = dateTime.getHours().toString() + ":" + seconds;
    }
  }
}

function matchForecastToChartScale(forecast) {
  for (let i = 0, j = 0; i < forecast.length && j < forecastDates.length;) {

    let currentForecastItemDate = new Date(forecast[i].TillDateTime);
    let currentScaleItemDate = forecastDates[i];

    // remove unnecessary past forecasts
    if (currentForecastItemDate < currentScaleItemDate) {
      forecast.splice(i, 1);
    }
    // fill with zero value objects 
    else if (currentForecastItemDate > currentScaleItemDate) {
      let blankForecast = createBlankForecast(currentForecastItemDate, forecast, i);
      forecast.splice(i, 1, blankForecast);
    }
    else {
      i++;
      j++;
    }
  }
}

function createBlankForecast(currentForecastItemDate, forecast, i) {
  let prevDate = currentForecastItemDate;
  prevDate.setHours(prevDate.getUTCHours() - 1);

  let prevFromDate = prevDate;
  prevFromDate.setHours(prevFromDate.getUTCHours() - 1);

  let blankForecast = {
    InstallationId: forecast[i].InstallationId,
    FromDateTime: prevFromDate,
    TillDateTime: prevDate,
    AirlyCaqi: 0,
    Pm25: 0,
    Pm10: 0,
    RequestDateTime: forecast[i].RequestDateTime,
    InstallationAddress: forecast[i].InstallationAddress,
    Source: forecast[i].Source,
  };

  return blankForecast;
}

function createInitForecastCharts() {
  let firstInstallationId = airQualityForecasts[0][0]?.InstallationId;

  for (let forecastSource in forecastsDictionary[firstInstallationId]) {
    charts[forecastSource] = createForecastChart(
      forecastsDictionary[firstInstallationId][forecastSource]);
  }
}

function updateForecastCharts(selectedInstallationId) {
  if (selectedInstallationId != 0) {
    for (let forecastSource in charts) {
      charts[forecastSource].remove();
    }

    for (let forecastSource in forecastsDictionary[selectedInstallationId]) {
      charts[forecastSource] = createForecastChart(
        forecastsDictionary[selectedInstallationId][forecastSource]);
    }
  }
}

function createForecastChart(forecast) {
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
    .domain(d3.range(forecastDates.length))
    .range([chart.margin.left, chart.width - chart.margin.right])
    .padding(0.1);

  let y = d3.scaleLinear()
    .domain([0, d3.max(forecast, d => d?.AirlyCaqi ?? 0)])
    .nice()
    .range([chart.height - chart.margin.bottom, chart.margin.top]);

  let xAxis = g => g
    .attr("transform", `translate(0,${chart.height - chart.margin.bottom})`)
    .call(d3.axisBottom(x)
      .tickFormat(i => forecastDates[i].getHours())
      .tickSizeOuter(0));

  let yAxis = g => g
    .attr("transform", `translate(${chart.margin.left},0)`)
    .call(d3.axisLeft(y))
    .call(yTitle);

  const chartDiv = d3.select("#mainDiv")
    .append("div")
    .attr("id", forecast[0].Source)
    .attr("class", "col-sm-12 col-lg-6 mb-5")

  const svg = chartDiv
    .append("svg")
    .attr("viewBox", [0, 0, chart.width, chart.height]);

  svg.append("g")
    .selectAll("rect")
    .data(forecast)
    .join("rect")
    .attr("x", (d, i) => x(i))
    .attr("y", d => y(d?.AirlyCaqi ?? 0))
    .attr("height", d => y(0) - y(d?.AirlyCaqi ?? 0))
    .attr("width", x.bandwidth())
    .attr("fill", d => getColorForCaqiRange(d?.AirlyCaqi ?? 0));

  svg.append("g")
    .call(xAxis);

  svg.append("g")
    .call(yAxis);

  return chartDiv.node();
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