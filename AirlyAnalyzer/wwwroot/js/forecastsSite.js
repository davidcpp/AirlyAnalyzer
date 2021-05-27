let forecastsTable = {};
let columnSelectInputs = [];

$(document).ready(function () {
  forecastsTable = $('#forecasts').DataTable({
    responsive: true,
    scrollY: '45vh',
    paging: false,
    initComplete: function () {
      this.api().columns().every(function () {
        var column = this;
        var select = $('<select><option value=""></option></select>')
          .appendTo($(column.footer()).empty())
          .on('change', function () {
            var val = $.fn.dataTable.util.escapeRegex($(this).val());

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
});