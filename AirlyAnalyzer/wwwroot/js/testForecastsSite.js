// Variables/objects from model
let airQualityForecasts = eval($('#forecastsSite').attr('airQualityForecasts'));
let forecastsDictionary = {}
let installationAddresses = {};
let forecastDates = [];

let charts = {}

let chartDivClass = "col-sm-12 col-lg-6 mb-5";

$(document).ready(function () {
  forecastDates = createForecastDates();
  installationAddresses = initInstallationAddresses();
  updateInstallationsSelect();
  initForecastsDictionary();
  createInitForecastCharts();
});

$('#airQualityInstallations').change(function () {
  let selectedInstallationId = $(this).val();
  updateForecastCharts(selectedInstallationId);
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

      forecastsDictionary[installationId]
        = splitForecastsBySource(installationForecasts);

      modifyForecastDates(installationForecasts);
      matchForecastsToChartScale(installationId);
    }
  }
}

function splitForecastsBySource(installationForecasts) {
  return installationForecasts.reduce(
    (forecastsBySource, item) => {
      const group = (forecastsBySource[item.Source] || []);
      group.push(item);
      forecastsBySource[item.Source] = group;
      return forecastsBySource;
    }, {});
}

function modifyForecastDates(installationForecasts) {
  for (let j = 0; j < installationForecasts?.length; j++) {
    installationForecasts[j].TillDateTime
      = new Date(installationForecasts[j].TillDateTime);
  }
}

function matchForecastsToChartScale(installationId) {
  for (let forecastSource in forecastsDictionary[installationId]) {
    let forecast = forecastsDictionary[installationId][forecastSource];

    if (forecast.length != forecastHoursNumber) {
      matchForecastToChartScale(forecast);
    }
  }
}

function matchForecastToChartScale(forecast) {
  for (let i = 0, j = 0; i < forecast.length && j < forecastDates.length;) {

    let currentForecastItemDate = forecast[i].TillDateTime;
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

function createForecastChart(forecast) {
  let forecastChart = new ForecastChart(forecastDates, forecast, chartDivClass);  
  return forecastChart.createForecastChartDiv();
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