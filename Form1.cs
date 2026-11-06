using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;


namespace Veml
{
    
    public partial class Form1 : Form
    {
        private StreamWriter currentStreamWriter;

        private string currentFileName;
        private string currentDateStr;
        private string deviceName;
        private string locationName;
        private Dictionary<string, StreamWriter> fileWriters = new Dictionary<string, StreamWriter>();

        public Form1(string device, string location)
        {
            InitializeComponent();

            // Сохраняем параметры сессии
            this.deviceName = device;
            this.locationName = location;

            textBox1.TextChanged += textBox1_TextChanged;

            timer1.Interval = 2000;
            timer1.Tick += Timer_Tick;
            timer1.Start();

            // Настройка даты и имени лог-файла
            currentDate = DateTime.Now.Date;
            currentDateStr = currentDate.ToString("dd_MM_yyyy");
            currentFileName = GenerateLogFileName(DateTime.Now);
            InitializeStreamWriter();

            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            // ====================================================================
            // --- ОБНОВЛЕННЫЙ БЛОК ДИНАМИЧЕСКИХ ЭЛЕМЕНТОВ УПРАВЛЕНИЯ (GUI) ---
            // ====================================================================

            // --- ЗАГРУЗКА НАСТРОЕК ПОЛЬЗОВАТЕЛЯ ПРИ СТАРТЕ ---

            // 1. Восстанавливаем шаблон лога, если он был сохранен ранее
            if (!string.IsNullOrEmpty(Properties.Settings.Default.UserLogTemplate))
            {
                logTemplate = Properties.Settings.Default.UserLogTemplate;
            }

            // 2. Восстанавливаем формулы пересчета
            if (Properties.Settings.Default.SavedFormulas != null && Properties.Settings.Default.SavedFormulas.Count > 0)
            {
                userFormulas.Clear(); // Очищаем формулы по умолчанию
                foreach (string formulaLine in Properties.Settings.Default.SavedFormulas)
                {
                    // Каждая строка записана в виде "Имя=Формула"
                    string[] parts = formulaLine.Split(new char[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        userFormulas[parts[0]] = parts[1];
                    }
                }
            }

            // --- КОНЕЦ БЛОКА ЗАГРУЗКИ ---


            // --- СВЯЗЫВАНИЕ ВИЗУАЛЬНЫХ ЭЛЕМЕНТОВ ИЗ КОНСТРУКТОРА ФОРМ ---

            // Задаем текст маски по умолчанию (или загруженный из настроек) в текстовое поле
            textBoxLogFormat.Text = logTemplate;

            // При изменении текста пользователем маска лога обновляется на лету
            textBoxLogFormat.TextChanged += (s, ev) => {
                logTemplate = textBoxLogFormat.Text;
            };

            // При смене параметра в выпадающем списке график мгновенно перестраивается за весь день
            comboBoxChartVar.SelectedIndexChanged += (s, ev) => {
                if (comboBoxChartVar.SelectedItem != null)
                {
                    RebuildChart(comboBoxChartVar.SelectedItem.ToString());
                }
            };

            // Открытие окна редактора формул при клике на визуальную кнопку
            btnOpenFormulaEditor.Click += (s, ev) => {
                using (FormulaEditorForm editor = new FormulaEditorForm(userFormulas))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        UpdateAvailableChartVariables(comboBoxChartVar);
                        MessageBox.Show("Новые формулы успешно применены!", "Конфигурация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            // Первичное наполнение выпадающего списка стартовыми переменными
            UpdateAvailableChartVariables(comboBoxChartVar);
            Form1_SizeChanged(this, EventArgs.Empty);

            // 🟢 ДОБАВЛЯЕМ ЭТУ СТРОКУ (принудительный пересчет при первом показе окна):
            this.Shown += (s, ev) => Form1_SizeChanged(this, EventArgs.Empty);
        }

        // ====================================================================
        // --- КОНЕЦ ОБНОВЛЕННОГО БЛОКА ---
        // ====================================================================

        private string GenerateLogFileName(DateTime date)
        {
            // Формат строго по вашему запросу: Дата_Город_Прибор.txt
            return $"{date.ToString("dd_MM_yyyy")}_{locationName}_{deviceName}.txt";
        }



        // private string portName;
        private SerialPort serialPort;
        // for chart 
       //  List<DateTime> timeData = new List<DateTime>();
       //  List<double> valueData = new List<double>();

         private string filePath;
        string[] ports = SerialPort.GetPortNames();
        bool isSerialPortOpen = false;
        string portName;
        double latitude, longitude;

        // --- НОВЫЙ ДИНАМИЧЕСКИЙ БЛОК ПЕРЕМЕННЫХ ---

        // Список всех телеметрических точек, принятых за текущие сутки (для перестроения графиков)
        private List<TelemetryPoint> dayHistory = new List<TelemetryPoint>();

        // Единый контекст: хранит текущие значения всех переменных (и сырых, и расчетных) в реальном времени
        private Dictionary<string, double> currentContext = new Dictionary<string, double>()
{
    { "latitude", 0 },
    { "longitude", 0 }
};

        // Список пользовательских формул (Имя расчетной переменной -> Математическое выражение).
        // Порядок важен: базовые формулы должны идти выше тех, которые их используют.
        private Dictionary<string, string> userFormulas = new Dictionary<string, string>()
{
    { "rawA_corr", "rawA - (Vis * 2.22) - (ir * 1.33)" },
    { "rawB_corr", "rawB - (Vis * 2.95) - (ir * 1.74)" },
    { "uvi_calc", "(rawA_corr * 0.00146) + (rawB_corr * 0.00259)" }
};

        // Настраиваемый шаблон логгирования. 
        // Теги вроде {rawA} или {uvi_calc} заменяются на цифры, а {math_formulas} — на список формул.
        private string logTemplate = "[{time}] Данные -> A: {rawA}, B: {rawB} | Расчет -> uvi: {uvi_calc} | Математика: [{math_formulas}]";

        // Имя переменной, которая в данный момент выводится на график. По умолчанию это "uvi_calc".
        private string selectedChartVariable = "uvi_calc";

        // --- КОНЕЦ НОВОГО БЛОКА ---




        private bool isReading = true;
        private DateTime currentDate;



        private void InitializeStreamWriter()
        {
            // Здесь тоже должно быть currentFileName!
            currentStreamWriter = new StreamWriter(currentFileName, true);
            currentStreamWriter.Flush();
            currentStreamWriter.Close();
        }


        private void CloseFile(string fileName)
        {
            if (fileWriters.ContainsKey(fileName))
            {
                fileWriters[fileName].Close();
                fileWriters[fileName].Dispose();
                fileWriters.Remove(fileName);


            }
        }

      
        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize the stream writer
            Form1_SizeChanged(this, EventArgs.Empty);
            //string currentFileName = DateTime.Now.ToString("dd_MM_yyyy") + ".txt";
            // currentStreamWriter = new StreamWriter(currentFileName, true); // true to append
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Close the stream writer when the application is shutting down
            if (currentStreamWriter != null)
            {
                // --- СОХРАНЕНИЕ ПОЛЬЗОВАТЕЛЬСКИХ НАСТРОЕК ---

                // 1. Сохраняем текущую маску логгирования
                Properties.Settings.Default.UserLogTemplate = logTemplate;

                // 2. Преобразуем словарь формул в StringCollection для сохранения
                if (Properties.Settings.Default.SavedFormulas == null)
                {
                    Properties.Settings.Default.SavedFormulas = new System.Collections.Specialized.StringCollection();
                }

                Properties.Settings.Default.SavedFormulas.Clear();
                foreach (var kvp in userFormulas)
                {
                    // Упаковываем в строку формата "Ключ=Значение"
                    Properties.Settings.Default.SavedFormulas.Add(kvp.Key + "=" + kvp.Value);
                }

                // Финальное физическое сохранение всех настроек на диск
                Properties.Settings.Default.Save();

                // --- КОНЕЦ БЛОКА СОХРАНЕНИЯ ---

                currentStreamWriter.Close();
                currentStreamWriter.Dispose();
            }
        }


        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string forsavedata = "";
            string filePath = "coordinates.txt";

            if (File.Exists(filePath))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string coordinates = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(coordinates))
                        {
                            string[] values = coordinates.Split(' ');
                            if (values.Length >= 2)
                            {
                                double.TryParse(values[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out latitude);
                                double.TryParse(values[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out longitude);
                            }
                        }
                    }
                }
                catch (Exception) { }
            }

            currentContext["latitude"] = latitude;
            currentContext["longitude"] = longitude;

            while (isReading)
            {
                try
                {
                    if (serialPort == null || !serialPort.IsOpen) return;

                    // Читаем всю пришедшую строку целиком (по переносу строки от Serial.println)
                    string incomingLine = serialPort.ReadLine();
                    if (string.IsNullOrWhiteSpace(incomingLine)) return;

                    //🟢 ЖОПА 🟢
                    // 🟢 СОХРАНЯЕМ И МГНОВЕННО ВЫВОДИМ СЫРУЮ СТРОКУ НА ЭКРАН ДО ПАРСИНГА
                    string rawLineToDisplay = incomingLine.Trim();

                    Invoke(new Action(() => {
                        // Выводим в созданный нами желтый статус-бар в самом низу
                        toolStripRawDataLabel.Text = " | Сырой лог: " + rawLineToDisplay;
                    }));



                    // Гарантированно очищаем от точек с запятой, которые ломали парсинг чисел в конце строки
                    incomingLine = incomingLine.Replace(";", "").Trim();

                    // 1. УНИВЕРСАЛЬНЫЙ ДИНАМИЧЕСКИЙ ПАРСИНГ
                    string[] parameters = incomingLine.Split(',');
                    foreach (string parameter in parameters)
                    {
                        string[] keyValue = parameter.Trim().Split(':');
                        if (keyValue.Length == 2)
                        {
                            string key = keyValue[0].Trim();
                            string valueStr = keyValue[1].Trim().Replace(',', '.');

                            double val;
                            if (double.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
                            {
                                currentContext[key] = val; // Сохраняем любую переменную автоматически
                            }
                        }
                    }

                    // 2. ВЫЧИСЛЕНИЕ ЦЕПОЧКИ ПОЛЬЗОВАТЕЛЬСКИХ ФОРМУЛ
                    foreach (var formula in userFormulas)
                    {
                        double result = ExpressionEvaluator.Evaluate(formula.Value, currentContext);
                        currentContext[formula.Key] = result;
                    }

                    // 3. СОХРАНЕНИЕ ТОЧКИ В АРХИВ ТЕКУЩИХ СУТОК
                    DateTime now = DateTime.Now;
                    string nowDateStr = now.ToString("dd_MM_yyyy");

                    if (nowDateStr != currentDateStr)
                    {
                        currentDate = now.Date;
                        currentDateStr = nowDateStr;
                        currentFileName = GenerateLogFileName(now); // Файл пересоздастся с правильным форматом имени

                        dayHistory.Clear();

                        Invoke(new Action(() => {
                            foreach (var series in chart1.Series)
                            {
                                series.Points.Clear();
                            }
                        }));
                    }

                    dayHistory.Add(new TelemetryPoint(now, currentContext));

                    // 4. ДИНАМИЧЕСКАЯ СБОРКА ТЕКСТА ЛОГА ПО ШАБЛОНУ
                    string formattedLog = logTemplate;
                    formattedLog = formattedLog.Replace("{time}", now.ToString("yyyy.MM.dd HH:mm:ss.fff"));

                    List<string> formulaStrings = new List<string>();
                    foreach (var f in userFormulas)
                    {
                        formulaStrings.Add(f.Key + "=" + f.Value);
                    }
                    string allMathText = string.Join(" | ", formulaStrings);
                    formattedLog = formattedLog.Replace("{math_formulas}", allMathText);

                    foreach (var kvp in currentContext)
                    {
                        string placeholder = "{" + kvp.Key + "}";
                        formattedLog = formattedLog.Replace(placeholder, kvp.Value.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture));
                    }

                    forsavedata = formattedLog.Replace("\r\n", "").Replace("\n", "");
                    formattedLog += "\r\n";

                    // 5. ОБНОВЛЕНИЕ ИНТЕРФЕЙСА (GUI)
                    Invoke(new Action(() => {
                        // Динамическое обновление выпадающего списка при обнаружении новых сенсоров
                        UpdateAvailableChartVariables(comboBoxChartVar);

                       
                        textBox1.AppendText(formattedLog);


                        // Отрисовка текущей точки на графике в реальном времени
                        if (currentContext.ContainsKey(selectedChartVariable) && chart1.Series.Count > 0)
                        {
                            chart1.Series[0].Points.AddXY(now, currentContext[selectedChartVariable]);
                        }

                        if (textBox1.Lines.Length >= 50000) textBox1.Clear();
                    }));

                    // Сохранение в файл дня
                    SaveTextToFile(forsavedata);
                    break;
                }
                catch (Exception)
                {
                    break;
                }
            }
        }



        private void Ser_Click(object sender, EventArgs e)    // COM-порт
        {
            string[] ports = SerialPort.GetPortNames();
            Serial.DropDownItems.Clear();
           
            foreach (string port in ports)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(port);
                item.Click += new EventHandler(portToolStripMenuItem_Click);
                Serial.DropDownItems.Add(item);
            }
        }

        private void portToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
             portName = item.Text;
            isSerialPortOpen = true;
            toolStripStatusLabel1.Text = portName + " открыт";
            serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            serialPort.Open();

           
            

            // Do something with the selected port name...
        }


        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            // Защита от вылета при сворачивании окна в панель задач
            if (this.ClientSize.Height < 200 || this.ClientSize.Width < 200) return;

            // 1. Нижняя линия управления (выпадающий список, маска лога и кнопка математики)
            // Она всегда жестко зафиксирована в самом низу формы, над статус-баром
            int controlY = this.ClientSize.Height - statusStrip1.Height - 35;

            Control cb = Controls.Find("comboBoxChartVar", true).FirstOrDefault();
            Control tb = Controls.Find("textBoxLogFormat", true).FirstOrDefault();
            Control btn = Controls.Find("btnOpenFormulaEditor", true).FirstOrDefault();

            if (cb != null) cb.Location = new Point(18, controlY);

            if (tb != null)
            {
                tb.Location = new Point(150, controlY);
                tb.Width = (int)(this.Width - 330);
            }

            if (btn != null && tb != null)
            {
                btn.Location = new Point(tb.Location.X + tb.Width + 10, controlY - 2);
            }

            // 2. Умное распределение размеров под выбранный режим отображения
            if (chart1.Visible && textBox1.Visible)
            {
                // РЕЖИМ 1: Комбинированный (График сверху, текст 150пкс снизу)
                textBox1.Location = new Point(18, controlY - 160);
                textBox1.Width = (int)(this.Width - 40);
                textBox1.Height = 150;

                chart1.Location = new Point(18, menuStrip1.Height + 5);
                chart1.Width = textBox1.Width;
                int calculatedChartHeight = textBox1.Location.Y - chart1.Location.Y - 10;
                chart1.Height = Math.Max(calculatedChartHeight, 100);
            }
            else if (chart1.Visible)
            {
                // РЕЖИМ 2: Только График (Растянут во весь экран до самой панели управления)
                chart1.Location = new Point(18, menuStrip1.Height + 5);
                chart1.Width = (int)(this.Width - 40);
                chart1.Height = Math.Max(controlY - chart1.Location.Y - 10, 100);
            }
            else if (textBox1.Visible)
            {
                // РЕЖИМ 3: Только Текст (Растянут во весь экран до самой панели управления)
                textBox1.Location = new Point(18, menuStrip1.Height + 5);
                textBox1.Width = (int)(this.Width - 40);
                textBox1.Height = Math.Max(controlY - textBox1.Location.Y - 10, 100);
            }
        }






        private void закрытьПортToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseSerialPort();
            toolStripStatusLabel1.Text = "Порт закрыт";
            isSerialPortOpen = false;
        }

                   
        private void CloseSerialPort()
{
    isReading = false; // Stop reading from the port
    Thread.Sleep(1000);
    try
    {
        if (serialPort.IsOpen)
        {
            serialPort.Close(); // Close the port
        }
    }

    catch (IOException ex)
    {
        // Handle the exception here
        Console.WriteLine("An error occurred while closing the serial port: " + ex.Message);
    }
    catch (Exception ex)
    {
        // Handle the exception here
        Console.WriteLine("An error occurred: " + ex.Message);
    }
}
        

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (isSerialPortOpen)
            {
                CloseSerialPort();
                isSerialPortOpen = false;
               
                stopToolStripMenuItem.Text = "Пуск"; // 🟢 НАШЕ МЕНЮ
                toolStripStatusLabel1.Text = portName + " закрыт";
            }
            else
            {
                isReading = true;
                serialPort.Open();
                isSerialPortOpen = true;
                stopToolStripMenuItem.Text = "Стоп"; // 🟢 НАШЕ МЕНЮ
                toolStripStatusLabel1.Text = portName + " открыт";
            }


        }

   
        

        private void save_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save text file";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFileDialog1.FileName;
                string text = textBox1.Text;
                SaveTextToFile(text);
            }
        }

        private void SaveTextToFile(string text)
        {
            // Используем нашу готовую глобальную переменную currentFileName, 
            // в которой уже лежат и Дата, и Город, и Прибор!
            string filePath = this.currentFileName;

            try
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.WriteLine(text);
                    writer.Flush();
                    writer.Close();
                }

                Console.WriteLine("Data saved to file: " + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while saving data to file: " + ex.Message);
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //SaveTextToFile(textBox1.Text);
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpForm helpForm = new HelpForm();
            helpForm.Show();
        }

        private void координатыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 coordForm = new Form3();
            coordForm.Show();
        }

        private void текстToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart1.Visible = false;
            textBox1.Visible = true;
            Form1_SizeChanged(this, EventArgs.Empty); // Принудительно пересчитываем размеры
        }

        private void графикToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart1.Visible = true;
            textBox1.Visible = false;
            Form1_SizeChanged(this, EventArgs.Empty); // Принудительно пересчитываем размеры
        }

        // 🟢 НОВЫЙ МЕТОД: Возвращает режим "График сверху + 150 пикселей текста снизу"
        private void комбинированныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart1.Visible = true;
            textBox1.Visible = true;
            Form1_SizeChanged(this, EventArgs.Empty); // Принудительно пересчитываем размеры
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
       //     chart1.Series["UVI"].Points.DataBindXY(timeData, valueData);
        }

        private void RebuildChart(string variableName)
        {
            // Запоминаем, какую переменную мы теперь отслеживаем на графике
            selectedChartVariable = variableName;

            // Защита от потоковых конфликтов: выполняем операции с графиком в основном потоке интерфейса
            if (chart1.InvokeRequired)
            {
                chart1.Invoke(new Action(() => RebuildChart(variableName)));
                return;
            }

            // 1. Полностью очищаем старые точки с графика
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }

            // 2. Переименовываем отображаемую серию (легенду) под имя выбранного параметра
            chart1.Series[0].Name = variableName;

            // 3. Проходим по всей истории точек, накопленной за текущий день
            foreach (TelemetryPoint point in dayHistory)
            {
                // Если в этой исторической точке есть данные для выбранной переменной — выводим на график
                if (point.Values.ContainsKey(variableName))
                {
                    double value = point.Values[variableName];
                    chart1.Series[0].Points.AddXY(point.Timestamp, value);
                }
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызываем код, который раньше висел на обычной кнопке
            button2_Click_1(this, EventArgs.Empty);
        }

        private void UpdateAvailableChartVariables(ComboBox comboBox)
        {
            if (comboBox.InvokeRequired)
            {
                comboBox.Invoke(new Action(() => UpdateAvailableChartVariables(comboBox)));
                return;
            }

            // Сохраняем текущий выбранный элемент, чтобы он не сбросился
            string currentSelected = comboBox.SelectedItem?.ToString();

            comboBox.Items.Clear();

            // Добавляем абсолютно все ключи, которые сейчас есть в системе (и с датчиков, и из формул)
            foreach (var key in currentContext.Keys)
            {
                // Исключаем системные служебные координаты, чтобы не загромождать список графика
                if (key != "latitude" && key != "longitude")
                {
                    comboBox.Items.Add(key);
                }
            }

            // Возвращаем выделение на место, если этот элемент всё ещё существует
            if (!string.IsNullOrEmpty(currentSelected) && comboBox.Items.Contains(currentSelected))
            {
                comboBox.SelectedItem = currentSelected;
            }
            else if (comboBox.Items.Count > 0)
            {
                // Если ничего не было выбрано, выбираем по умолчанию наш uvi_calc или первый элемент
                if (comboBox.Items.Contains("uvi_calc"))
                    comboBox.SelectedItem = "uvi_calc";
                else
                    comboBox.SelectedIndex = 0;
            }
        }
           
}    
    }

