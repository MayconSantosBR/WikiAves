using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WikiAves.Core.Util
{
    public static class ListExtensions
    {
        private static Random rng = new Random();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static string ConvertToCsv<T>(this IEnumerable<T> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var properties = typeof(T).GetProperties();

            using var stringWriter = new StringWriter();
            WriteCsvHeader(stringWriter, properties);
            WriteCsvData(stringWriter, data, properties);

            return stringWriter.ToString();
        }

        private static void WriteCsvHeader(TextWriter writer, System.Reflection.PropertyInfo[] properties)
        {
            var header = string.Join(",", properties.Select(p => p.Name));
            writer.WriteLine(header);
        }

        private static void WriteCsvData<T>(TextWriter writer, IEnumerable<T> data, System.Reflection.PropertyInfo[] properties)
        {
            foreach (var item in data)
            {
                var values = properties.Select(p => p.GetValue(item)?.ToString() ?? string.Empty);
                var line = string.Join(",", values);
                writer.WriteLine(line);
            }
        }
    }
}
