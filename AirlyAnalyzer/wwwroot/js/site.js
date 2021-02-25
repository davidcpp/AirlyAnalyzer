const webApiUri = 'api/ForecastErrorsApi/';
const errorsInDayUri = webApiUri + 'GetErrorsInDay/';
const requestDatesUri = webApiUri + 'GetRequestDates';

let forecastErrorsTable = {};

$(document).ready(function () {
  forecastErrorsTable = $('#forecastErrors').DataTable({
    scrollY: '45vh',
    paging: false,
  });
  updateDaysSelect();
});

function updateDaysSelect() {
  $.get(requestDatesUri, null, 'json')
    .done((requestDates) => {
      var select = document.getElementById('forecastErrorDays');
      for (let i = 1; i <= requestDates.length; i++) {
        let option = document.createElement("option");
        option.text = requestDates[i - 1];
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