using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicStoreCatalog.Utilities
{
    public static class PhoneFormatter
    {
        public static void PhoneBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public static void PhoneBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string text = textBox.Text.Replace(" ", "").Replace("-", "").Replace(")", "").Replace("(", "");

                if (text.Length >= 4)
                {
                    textBox.Text = FormatPhoneNumber(text);
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        public static string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length <= 4)
                return phone;

            string result = "+375 (";

            if (phone.Length > 4)
                result += phone.Substring(4, Math.Min(2, phone.Length - 4));
            if (phone.Length > 6)
                result += ")-" + phone.Substring(6, Math.Min(3, phone.Length - 6));
            if (phone.Length > 9)
                result += "-" + phone.Substring(9, Math.Min(2, phone.Length - 9));
            if (phone.Length > 11)
                result += "-" + phone.Substring(11, Math.Min(2, phone.Length - 11));

            return result;
        }
    }
}