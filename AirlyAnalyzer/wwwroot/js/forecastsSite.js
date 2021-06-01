// Variables/objects from model
let airQualityForecasts = eval($('#forecastsSite').attr('airQualityForecasts'));

for (let i = 0; i < airQualityForecasts.length; i++) {
  let dateTime = new Date(airQualityForecasts[i].TillDateTime);
  let seconds = dateTime.getSeconds();
  seconds = seconds < 10 ? seconds = "0" + seconds : seconds;
  airQualityForecasts[i].TillDateTime
    = dateTime.getHours().toString() + ":" + seconds;
}

$(document).ready(function () {
  let margin = ({ top: 30, right: 0, bottom: 30, left: 40 })
  let height = 500;
  let width = 900;

  let x = d3.scaleBand()
    .domain(d3.range(airQualityForecasts.length))
    .range([margin.left, width - margin.right])
    .padding(0.1);

  let y = d3.scaleLinear()
    .domain([0, d3.max(airQualityForecasts, d => d.AirlyCaqi)]).nice()
    .range([height - margin.bottom, margin.top]);

  let xAxis = g => g
    .attr("transform", `translate(0,${height - margin.bottom})`)
    .call(d3.axisBottom(x).tickFormat(i => airQualityForecasts[i].TillDateTime)
      .tickSizeOuter(0));

  let yAxis = g => g
    .attr("transform", `translate(${margin.left},0)`)
    .call(d3.axisLeft(y))
    .call(g => g.append("text")
      .attr("x", -margin.left)
      .attr("y", 10)
      .attr("fill", "currentColor")
      .attr("text-anchor", "start")
      .attr("font-family", "sans-serif")
      .attr("font-weight", "bold")
      .attr("font-size", 10)
      .text("CAQI"));

  let color = "steelblue";

  const svg = d3.select("#mainDiv")
    .append("svg")
    .attr("viewBox", [0, 0, width, height]);

  svg.append("g")
    .attr("fill", color)
    .selectAll("rect")
    .data(airQualityForecasts)
    .join("rect")
    .attr("x", (d, i) => x(i))
    .attr("y", d => y(d.AirlyCaqi))
    .attr("height", d => y(0) - y(d.AirlyCaqi))
    .attr("width", x.bandwidth());

  svg.append("g")
    .call(xAxis);

  svg.append("g")
    .call(yAxis);
});