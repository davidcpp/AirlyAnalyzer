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

const chartSize = {
  margin: ({ top: 30, right: 60, bottom: 30, left: 60 }),
  height: 500,
  width: 900,
};

const yAxisTitle = {
  text: "CAQI",
  fontSize: 13,
  x: -chartSize.margin.left + 25,
};
yAxisTitle.y = yAxisTitle.fontSize;

let forecastHoursNumber = 24;

function createForecastDates() {
  let forecastDates = [];

  let initDate = new Date();
  initDate.setHours(initDate.getHours() + 1, 0, 0, 0);

  for (let i = 0; i < forecastHoursNumber; i++) {
    let currentDate = new Date(initDate);
    currentDate.setHours(currentDate.getHours() + i)
    forecastDates.push(currentDate);
  }

  return forecastDates;
}

function initInstallationAddresses() {
  let installationAddresses = [];

  for (let i = 0; i < airQualityForecasts?.length; i++) {
    let installationForecasts = airQualityForecasts[i];

    if (installationForecasts?.length > 0) {
      let installationId = installationForecasts[0]?.InstallationId;

      installationAddresses[installationId]
        = installationForecasts[0]?.InstallationAddress;
    }
  }

  return installationAddresses;
}

function createYAxisTitle() {
  let yTitleSvg = g => g
    .append("text")
    .attr("x", yAxisTitle.x)
    .attr("y", yAxisTitle.y)
    .attr("fill", "currentColor")
    .attr("text-anchor", "start")
    .attr("font-family", "sans-serif")
    .attr("font-weight", "bold")
    .attr("font-size", yAxisTitle.fontSize)
    .text(yAxisTitle.text);

  return yTitleSvg;
}

function createScales(forecastDates, forecast) {
  let x = d3.scaleBand()
    .domain(d3.range(forecastDates.length))
    .range([chartSize.margin.left, chartSize.width - chartSize.margin.right])
    .padding(0.1);

  let y = d3.scaleLinear()
    .domain([0, d3.max(forecast, d => d?.AirlyCaqi ?? 0)])
    .nice()
    .range([chartSize.height - chartSize.margin.bottom, chartSize.margin.top]);

  return { x, y };
}

function createAxes(x, y, forecastDates, yTitleSvg) {
  let xAxis = g => g
    .attr("transform", `translate(0,${chartSize.height - chartSize.margin.bottom})`)
    .call(d3.axisBottom(x)
      .tickFormat(i => forecastDates[i].getHours())
      .tickSizeOuter(0));

  let yAxis = g => g
    .attr("transform", `translate(${chartSize.margin.left},0)`)
    .call(d3.axisLeft(y))
    .call(yTitleSvg);

  return { xAxis, yAxis };
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