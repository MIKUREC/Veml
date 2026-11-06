using System;
using System.Windows.Forms;

namespace Veml
{
    public partial class SessionForm : Form
    {
        // Публичные свойства, которые затем заберет главное окно
        public string DeviceName { get; private set; }
        public string LocationName { get; private set; }

        public SessionForm()
        {
            InitializeComponent();

            // 1. При загрузке окна проверяем, были ли сохранены значения ранее.
            // Если да — автоматически подставляем их в текстовые поля.
            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastLocation))
            {
                textBoxCity.Text = Properties.Settings.Default.LastLocation;
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastDevice))
            {
                textBoxDevice.Text = Properties.Settings.Default.LastDevice;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(textBoxCity.Text) || string.IsNullOrWhiteSpace(textBoxDevice.Text))
            {
                MessageBox.Show("Пожалуйста, заполните оба поля перед началом работы.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Формируем чистые имена для файлов (заменяем пробелы на нижнее подчеркивание)
            LocationName = textBoxCity.Text.Trim().Replace(" ", "_");
            DeviceName = textBoxDevice.Text.Trim().Replace(" ", "_");

            // 2. Запоминаем текущие введенные значения в постоянную память
            Properties.Settings.Default.LastLocation = textBoxCity.Text.Trim();
            Properties.Settings.Default.LastDevice = textBoxDevice.Text.Trim();

            // Физически записываем файл конфигурации user.config на жесткий диск
            Properties.Settings.Default.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
