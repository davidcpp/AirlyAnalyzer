// Variables/objects from model
let airQualityForecasts = eval($('#forecastsSite').attr('airQualityForecasts'));
let forecastsDictionary = {}
let installationAddresses = {};
let chart = {}

let chartDivClass = "col-12";

forecastDates = createForecastDates();
installationAddresses = initInstallationAddresses(airQualityForecasts);
initForecastsDictionary();

$(document).ready(function () {
  let firstInstallationId = airQualityForecasts[0][0]?.InstallationId;

  let forecastChart = new ForecastChart(
    forecastDates, forecastsDictionary[firstInstallationId], chartDivClass);

  chart = forecastChart.createForecastChartDiv();
  updateInstallationsSelect();
});

$('#airQualityInstallations').change(function () {
  let selectedInstallationId = $(this).val();
  updateForecastChart(selectedInstallationId);
});

function initForecastsDictionary() {
  for (let i = 0; i < airQualityForecasts?.length; i++) {
    if (airQualityForecasts[i]?.length > 0) {
      forecastsDictionary[airQualityForecasts[i][0]?.InstallationId]
        = airQualityForecasts[i];
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

function updateForecastChart(selectedInstallationId) {
  if (selectedInstallationId != 0) {
    chart.remove();

    let forecastChart = new ForecastChart(
      forecastDates, forecastsDictionary[selectedInstallationId], chartDivClass);
    chart = forecastChart.createForecastChartDiv();
  }
}