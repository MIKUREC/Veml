using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Veml
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (SessionForm sessionForm = new SessionForm())
            {
                if (sessionForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // Мы берем свойства DeviceName и LocationName из sessionForm 
                // и передаем их в конструктор Form1
                Application.Run(new Form1(sessionForm.DeviceName, sessionForm.LocationName));
            }

        }
    }
}
