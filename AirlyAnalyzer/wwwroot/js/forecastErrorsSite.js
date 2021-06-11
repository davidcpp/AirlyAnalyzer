const webApiUri = 'api/ForecastErrorsApi/';
const errorsInDayUri = webApiUri + 'GetErrorsInDay/';
const requestDatesUri = webApiUri + 'GetRequestDates';

let forecastErrorsTable = {};
let columnSelectInputs = [];

$(document).ready(function () {
  forecastErrorsTable = $('#forecastErrors').DataTable({
    responsive: true,
    scrollY: '45vh',
    paging: false,
    initComplete: function () {
      this.api().columns().every(function () {
        let column = this;
        let select = $('<select><option value=""></option></select>')
          .appendTo($(column.footer()).empty())
          .on('change', function () {
            let val = $.fn.dataTable.util.escapeRegex($(this).val());

            column.search(val ? '^' + val + '$' : '', true, false)
                  .draw();
          });

        column.data().unique().sort().each(function (d) {
          select.append('<option value="' + d + '">' + d + '</option>')
        });

        columnSelectInputs.push(select);
      });
    }
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
          forecastErrors[i].period,
          forecastErrors[i].class,
          (new Date(forecastErrors[i].tillDateTime)).toLocaleString('tr-TR'),
          forecastErrors[i].source,
          forecastErrors[i].installationAddress,
          forecastErrors[i].airlyCaqiPct + '%',
          forecastErrors[i].airlyCaqi,
        ]

        forecastErrorsTable.row.add(forecastError);
      }

      forecastErrorsTable.draw('full-reset');
      updateColumnSelectInputs();
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

function updateColumnSelectInputs() {
  forecastErrorsTable.columns().every(function (index) {
    let column = this;

    // Refresh searching to blank strings after table update 
    // - to show all received data in table 
    column.search('', true, false)
          .draw();

    columnSelectInputs[index].empty();
    columnSelectInputs[index].append('<option value=""></option>');

    column.data().unique().sort().each(function (d) {
      columnSelectInputs[index]
        .append('<option value="' + d + '">' + d + '</option>')
    });
  });
}