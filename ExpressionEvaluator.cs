using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace Veml
{
    public static class ExpressionEvaluator
    {
        private static readonly DataTable _table = new DataTable();

        public static double Evaluate(string expression, Dictionary<string, double> variables)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expression)) return 0.0;

                string processedExpression = expression;

                // Сортируем ключи по длине (от длинных к коротким). 
                // Это важно, чтобы "rawA_corr" не заменился частично внутри слова "rawA"
                var sortedKeys = new List<string>(variables.Keys);
                sortedKeys.Sort((x, y) => y.Length.CompareTo(x.Length));

                foreach (var key in sortedKeys)
                {
                    // Ищем переменную как отдельное слово (\b) и заменяем на число
                    string pattern = @"\b" + Regex.Escape(key) + @"\b";
                    string valueStr = variables[key].ToString(System.Globalization.CultureInfo.InvariantCulture);
                    processedExpression = Regex.Replace(processedExpression, pattern, valueStr);
                }

                // Вычисляем математическое выражение с помощью встроенного механизма .NET
                object result = _table.Compute(processedExpression, string.Empty);
                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                // Если пользователь ошибся в формуле, пишем в консоль и возвращаем 0
                Console.WriteLine("Ошибка вычисления формулы [" + expression + "]: " + ex.Message);
                return 0.0;
            }
        }
    }
}
