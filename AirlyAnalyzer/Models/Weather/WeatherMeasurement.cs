namespace AirlyAnalyzer.Models.Weather
{
    public class WeatherMeasurement
    {
        public byte Month { get; set; }

        public byte Day { get; set; }

        public byte Hour { get; set; }

        public byte CloudCover { get; set; }

        public byte Humidity { get; set; }

        public float Pressure { get; set; }

        public float Rain6h { get; set; }

        public float Temperature { get; set; }

        public int Visibility { get; set; }

        public float WindSpeed { get; set; }

        public short WindDirection { get; set; }

        public float WindGust { get; set; }

        public short? AirlyCaqi { get; set; }
    }
}
