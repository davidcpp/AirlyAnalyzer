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
        option.text = trimTime(requestDates[i - 1]);
        option.value = i.toString();
        select.add(option);
      }
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

function updateForecastErrorsTable(day) {
  if (day <= 0) {
    return;
  }

  $.get(errorsInDayUri + day.toString(), null, 'json')
    .done((forecastErrors) => {
      forecastErrorsTable.clear();

      for (let i = 0; i < forecastErrors.length; i++) {
        let forecastError = [
          forecastErrors[i].installationId,
          trimLocalTimeOffset(forecastErrors[i].fromDateTime),
          trimLocalTimeOffset(forecastErrors[i].tillDateTime),
          forecastErrors[i].airlyCaqiPctError,
          forecastErrors[i].airlyCaqiError,
          trimLocalTimeOffset(forecastErrors[i].requestDateTime),
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

function trimLocalTimeOffset(localDateTime) {
  let plusCharacter = localDateTime.indexOf('+');
  localDateTime = localDateTime.slice(0, plusCharacter);
  return localDateTime.replace('T', ' ');
}

function trimTime(dateTime) {
  let tCharacter = dateTime.indexOf('T');
  return dateTime.slice(0, tCharacter);
}

$('#forecastErrorDays').change(function () {
  let choosedDay = parseInt($(this).val());
  updateForecastErrorsTable(choosedDay);
});