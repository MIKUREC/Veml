using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Veml
{
    public class FormulaEditorForm : Form
    {
        private DataGridView dataGridView1;
        private Button btnSave;
        private Button btnAddRow;
        private Button btnDeleteRow;

        // Ссылка на словарь формул из главной формы
        private Dictionary<string, string> _formulas;

        public FormulaEditorForm(Dictionary<string, string> currentFormulas)
        {
            _formulas = currentFormulas;

            // 1. Сначала конструктор Windows Forms создает базовый дизайн
            InitializeComponent();

            // 2. ЗДЕСЬ (вне InitializeComponent) безопасно привязываем логику кнопок:

            // Логика кнопки "Добавить" (вынесена из InitializeComponent)
            btnAddRow.Click += BtnAddRow_Click;

            // Логика кнопки "Удалить"
            btnDeleteRow.Click += (s, e) => {
                if (dataGridView1.CurrentRow != null)
                    dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
            };

            // Логика кнопки "Применить"
            btnSave.Click += BtnSave_Click;

            // Загружаем текущие данные в таблицу
            LoadFormulas();
        }

        // Отдельный чистый метод для добавления строк, который не ломает визуальный редактор
        private void BtnAddRow_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView1);
            row.Cells[0].Value = "";
            row.Cells[1].Value = "";
            dataGridView1.Rows.Add(row);
        }

        private void InitializeComponent()
        {
            this.Text = "Редактор математических формул";
            this.Size = new Size(550, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(54, 54, 64);
            this.ForeColor = Color.PaleGreen;

            // Настройка таблицы
            dataGridView1 = new DataGridView();
            dataGridView1.Location = new Point(12, 12);
            dataGridView1.Size = new Size(510, 280);
            dataGridView1.BackgroundColor = Color.FromArgb(44, 44, 54);
            dataGridView1.ForeColor = Color.Black;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AllowUserToAddRows = false;

            // Добавляем колонки
            dataGridView1.Columns.Add("ParamName", "Имя переменной (результата)");
            dataGridView1.Columns.Add("Expression", "Математическая формула");

            // Кнопка Добавить строку
            btnAddRow = new Button();
            btnAddRow.Text = "Добавить";
            btnAddRow.Location = new Point(12, 305);
            btnAddRow.Size = new Size(100, 30);
            btnAddRow.BackColor = Color.DimGray;
            btnAddRow.ForeColor = Color.White;

            // Кнопка Удалить строку
            btnDeleteRow = new Button();
            btnDeleteRow.Text = "Удалить";
            btnDeleteRow.Location = new Point(120, 305);
            btnDeleteRow.Size = new Size(100, 30);
            btnDeleteRow.BackColor = Color.DimGray;
            btnDeleteRow.ForeColor = Color.White;

            // Кнопка Сохранить
            btnSave = new Button();
            btnSave.Text = "Применить";
            btnSave.Location = new Point(422, 305);
            btnSave.Size = new Size(100, 30);
            btnSave.BackColor = Color.ForestGreen;
            btnSave.ForeColor = Color.White;

            // Безопасное добавление элементов управления на форму поштучно
            this.Controls.Add(dataGridView1);
            this.Controls.Add(btnAddRow);
            this.Controls.Add(btnDeleteRow);
            this.Controls.Add(btnSave);
        }

        // Загружаем текущие формулы в таблицу при открытии окна
        private void LoadFormulas()
        {
            foreach (var kvp in _formulas)
            {
                dataGridView1.Rows.Add(kvp.Key, kvp.Value);
            }
        }

        // Сохраняем измененные формулы обратно в программу при нажатии кнопки
        private void BtnSave_Click(object sender, EventArgs e)
        {
            _formulas.Clear();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["ParamName"].Value != null && row.Cells["Expression"].Value != null)
                {
                    string key = row.Cells["ParamName"].Value.ToString().Trim();
                    string val = row.Cells["Expression"].Value.ToString().Trim();

                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
                    {
                        _formulas[key] = val;
                    }
                }
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
