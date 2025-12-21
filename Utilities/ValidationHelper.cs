using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MusicStoreCatalog.Utilities
{
    public static class ValidationHelper
    {
        public static void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == null) return;

            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public static void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == null || !(sender is System.Windows.Controls.TextBox textBox))
                return;

            string newText = textBox.Text + e.Text;
            Regex regex = new Regex(@"^[0-9]*[,.]?[0-9]*$");
            e.Handled = !regex.IsMatch(newText);
        }
    }
}