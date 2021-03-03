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

$('#forecastErrorDays').change(function () {
  let choosedDate = $(this).val();
  updateForecastErrorsTable(choosedDate);
});

function updateDaysSelect() {
  $.get(requestDatesUri, null, 'json')
    .done((requestDates) => {
      let select = document.getElementById('forecastErrorDays');
      for (let i = 1; i <= requestDates.length; i++) {
        let option = document.createElement("option");
        let requestDate = new Date(requestDates[i - 1]);
        option.text = requestDate.toLocaleDateString();
        option.value = requestDates[i - 1];
        select.add(option);
      }
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

function updateForecastErrorsTable(requestDate) {
  if (requestDate <= 0) {
    return;
  }

  $.get(errorsInDayUri + requestDate, null, 'json')
    .done((forecastErrors) => {
      forecastErrorsTable.clear();

      for (let i = 0; i < forecastErrors.length; i++) {
        let forecastError = [
          forecastErrors[i].installationId,
          (new Date(forecastErrors[i].fromDateTime)).toLocaleString('tr-TR'),
          (new Date(forecastErrors[i].tillDateTime)).toLocaleString('tr-TR'),
          forecastErrors[i].airlyCaqiPct + '%',
          forecastErrors[i].airlyCaqi,
          (new Date(forecastErrors[i].requestDateTime)).toLocaleString('tr-TR'),
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