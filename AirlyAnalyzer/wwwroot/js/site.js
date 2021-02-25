const webApiUri = 'api/ForecastErrorsApi/';
const errorsInDayUri = webApiUri + 'GetErrorsInDay/';
const numberOfDaysUri = webApiUri + 'GetNumberOfDays';

let forecastErrorsTable = {};

$(document).ready(function () {
  forecastErrorsTable = $('#forecastErrors').DataTable({
    scrollY: 400,
    paging: false,
  });
  updateDaysSelect();
});

function updateDaysSelect() {
  $.get(numberOfDaysUri, null, 'json')
    .done((numberOfDays) => {
      var select = document.getElementById('forecastErrorDays');
      for (let i = 1; i <= numberOfDays; i++) {
        let option = document.createElement("option");
        option.text = i.toString();
        option.value = i.toString();
        select.add(option);
      }
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

function updateForecastErrorsTable(day) {
  $.get(errorsInDayUri + day.toString(), null, 'json')
    .done((forecastErrors) => {
      forecastErrorsTable.clear();

      for (let i = 0; i < forecastErrors.length; i++) {
        let forecastError = [
          forecastErrors[i].installationId,
          forecastErrors[i].fromDateTime,
          forecastErrors[i].tillDateTime,
          forecastErrors[i].airlyCaqiPctError,
          forecastErrors[i].airlyCaqiError,
          forecastErrors[i].requestDateTime,
          forecastErrors[i].errorType,
        ]

        forecastErrorsTable.row.add(forecastError);
      }

      forecastErrorsTable.draw('full-reset');
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

$('#forecastErrorDays').change(function () {
  let choosedDay = parseInt($(this).val());
  updateForecastErrorsTable(choosedDay);
});