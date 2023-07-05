using System.Text.RegularExpressions;

namespace WikiAvesCore.Util
{
    public static class StringExtensions
    {
        public static string UseRegex(this string text, string expression, int position = 1)
        {
            Regex regex = new Regex(expression, RegexOptions.None);
            return regex.Match(text).Groups[position].Value.ToString();
        }

        public static string OnlyNumbers(this string text)
        {
            return new String(text.Where(char.IsDigit).ToArray());
        }
    }
}
