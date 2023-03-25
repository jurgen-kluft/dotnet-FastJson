using Xunit;

public class JsonReaderTests
{
    public class TemperatureRange
    {
        public int High { get; set; }
        public int Low { get; set; }

        public static TemperatureRange ReadJson(FJson.Reader json)
        {
            var TempRange = new TemperatureRange();
            while (json.Read(out var key, out var value))
            {
                if (json.IsObjectEnd(key, value))
                    break;
                var t = int.Parse(json.FieldStr(value));
                if (json.IsFieldName(key, "high"))
                {
                    TempRange.High = t;
                }
                else
                {
                    TempRange.Low = t;
                }
            }

            return TempRange;
        }
    }

    public class Weather
    {
        public DateTime Date { get; set; }
        public int Temperature { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();
        public Dictionary<string, TemperatureRange> TemperatureRanges { get; set; } = new Dictionary<string, TemperatureRange>();
        public List<string> SummaryWords { get; set; } = new List<string>();

        public void Read(FJson.Reader json)
        {
            while (json.Read(out var key, out var value))
            {
                if (json.IsObjectEnd(key, value))
                    break;

                if (json.IsFieldName(key, "date"))
                {
                    Date = DateTime.Parse(json.FieldStr(value));
                }
                else if (json.IsFieldName(key, "TemperatureCelsius"))
                {
                    Temperature = int.Parse(json.FieldStr(value));
                }
                else if (json.IsFieldName(key, "Summary"))
                {
                    Summary = json.FieldStr(value).ToString();
                }
                else if (json.IsFieldName(key, "DatesAvailable"))
                {
                    ReadAvailableDates(json);
                }
                else if (json.IsFieldName(key, "TemperatureRanges"))
                {
                    ReadTemperatureRanges(json);
                }
                else if (json.IsFieldName(key, "SummaryWords"))
                {
                    ReadSummaryWords(json);
                }
            }

        }

        private void ReadAvailableDates(FJson.Reader json)
        {
            while (json.Read(out var key, out var value))
            {
                if (json.IsArrayEnd(key, value))
                    break;
                var date = DateTime.Parse(json.FieldStr(value));
                AvailableDates.Add(date);
            }
        }

        private void ReadTemperatureRanges(FJson.Reader json)
        {
            while (json.Read(out var key, out var value))
            {
                if (json.IsObjectEnd(key, value))
                    break;
                var temp = TemperatureRange.ReadJson(json);
                TemperatureRanges.Add(json.FieldStr(key).ToString(), temp);
            }
        }

        private void ReadSummaryWords(FJson.Reader json)
        {
            while (json.Read(out var key, out var value))
            {
                if (!json.IsArrayEnd(key, value))
                    break;
                var word = json.FieldStr(value);
                SummaryWords.Add(word.ToString());
            }
        }

    }

    [Fact]
    public void TestRead()
    {
        string jsonString = """
{
  "Date": "2019-08-01T00:00:00-07:00",
  "TemperatureCelsius": 25,
  "Summary": "Hot",
  "DatesAvailable": [
    "2019-08-01T00:00:00-07:00",
    "2019-08-02T00:00:00-07:00"
  ],
  "TemperatureRanges": {
    "Cold": {
      "High": 20,
      "Low": -10
    },
    "Hot": {
      "High": 60,
      "Low": 20
    }
            },
  "SummaryWords": [
    "Cool",
    "Windy",
    "Humid"
  ]
}
""";
        var jsonReader = new FJson.Reader();
        if (jsonReader.Begin(jsonString))
        {
            var weather = new Weather();
            weather.Read(jsonReader);
        }
    }
}
