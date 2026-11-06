namespace Veml
{
    partial class SessionForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox textBoxDevice;
        private System.Windows.Forms.TextBox textBoxCity;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonStart;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.textBoxDevice = new System.Windows.Forms.TextBox();
            this.textBoxCity = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // textBoxDevice (Поле ввода прибора)
            this.textBoxDevice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(64)))));
            this.textBoxDevice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxDevice.Font = new System.Drawing.Font("Courier New", 10F);
            this.textBoxDevice.ForeColor = System.Drawing.Color.LightGreen;
            this.textBoxDevice.Location = new System.Drawing.Point(30, 120); // <-- Изменено (было 50)
            this.textBoxDevice.Name = "textBoxDevice";
            this.textBoxDevice.Size = new System.Drawing.Size(320, 26);
            this.textBoxDevice.TabIndex = 1; // <-- Сюда курсор перейдет при нажатии Tab
            // textBoxCity (Поле ввода города)
            this.textBoxCity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(64)))));
            this.textBoxCity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxCity.Font = new System.Drawing.Font("Courier New", 10F);
            this.textBoxCity.ForeColor = System.Drawing.Color.LightGreen;
            this.textBoxCity.Location = new System.Drawing.Point(30, 50); // <-- Изменено (было 120)
            this.textBoxCity.Name = "textBoxCity";
            this.textBoxCity.Size = new System.Drawing.Size(320, 26);
            this.textBoxCity.TabIndex = 0; // <-- Ставим фокус ввода первым
            // label1 (Имя / Номер прибора)
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Courier New", 10F);
            this.label1.ForeColor = System.Drawing.Color.LightGreen;
            this.label1.Location = new System.Drawing.Point(26, 95); // <-- Изменено (было 25)
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(199, 20);
            this.label1.Text = "Имя / Номер прибора:";
            // label2 (Место измерения)
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Courier New", 10F);
            this.label2.ForeColor = System.Drawing.Color.LightGreen;
            this.label2.Location = new System.Drawing.Point(26, 25); // <-- Изменено (было 95)
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 20);
            this.label2.Text = "Место измерения:";
            // buttonStart
            this.buttonStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.buttonStart.Location = new System.Drawing.Point(110, 180);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(160, 40);
            this.buttonStart.TabIndex = 2;
            this.buttonStart.Text = "Запустить сессию";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // SessionForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(44)))));
            this.ClientSize = new System.Drawing.Size(382, 253);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxCity);
            this.Controls.Add(this.textBoxDevice);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SessionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Параметры сессии измерений";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
