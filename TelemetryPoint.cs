using System;
using System.Collections.Generic;

namespace Veml
{
    public class TelemetryPoint
    {
        // Время фиксации данных
        public DateTime Timestamp { get; set; }

        // Словарь всех значений на этот момент времени (например: "rawA"=200, "temp"=25, "uvi_calc"=0.05)
        public Dictionary<string, double> Values { get; set; }

        public TelemetryPoint(DateTime timestamp, Dictionary<string, double> values)
        {
            Timestamp = timestamp;
            // Создаем копию словаря текущих данных, чтобы они не перезаписались при следующем приеме с порта
            Values = new Dictionary<string, double>(values);
        }
    }
}
