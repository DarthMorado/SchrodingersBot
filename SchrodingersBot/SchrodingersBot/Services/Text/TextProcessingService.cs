using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Services.Text
{
    public interface ITextProcessingService
    {
        string ReplaceDecimalSepparator(string input);
    }

    public class TextProcessingService : ITextProcessingService
    {
        public TextProcessingService()
        {
            
        }

        public string ReplaceDecimalSepparator(string input)
        {
            string CultureName = Thread.CurrentThread.CurrentCulture.Name;
            CultureInfo ci = new CultureInfo(CultureName);
            var decimalSeparator = ci.NumberFormat.NumberDecimalSeparator;

            var sb = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsDigit(c) || c == ' ')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(decimalSeparator);
                }
            }

            return sb.ToString();
        }
    }
}
