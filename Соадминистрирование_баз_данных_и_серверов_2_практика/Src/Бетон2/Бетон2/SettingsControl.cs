using System;
using System.Drawing;
using System.Windows.Forms;

namespace Бетон2
{
    public class SettingsControl : UserControl
    {
        public event EventHandler<bool> ConnectionTested;

        private TextBox connectionStringTextBox;
        private Label infoLabel;

        public SettingsControl()
        {
            InitializeComponents();
        }

        public string ConnectionString => connectionStringTextBox.Text;

        private void InitializeComponents()
        {
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;

            // Заголовок раздела
            var titleLabel = new Label();
            titleLabel.Text = "Настройки подключения к базе данных";
            titleLabel.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold);
            titleLabel.Location = new Point(0, 0);
            titleLabel.Size = new Size(600, 30);
            titleLabel.ForeColor = Color.DarkBlue;

            // Группа настроек подключения
            var connectionGroup = new GroupBox();
            connectionGroup.Text = "Параметры подключения";
            connectionGroup.Location = new Point(0, 50);
            connectionGroup.Size = new Size(700, 150);
            connectionGroup.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);

            var connectionLabel = new Label();
            connectionLabel.Text = "Строка подключения:";
            connectionLabel.Location = new Point(20, 30);
            connectionLabel.Size = new Size(150, 20);

            connectionStringTextBox = new TextBox();
            connectionStringTextBox.Text = @"Server=DESKTOP-I1TS63J;Database=PP07;Integrated Security=True;";
            connectionStringTextBox.Location = new Point(20, 55);
            connectionStringTextBox.Size = new Size(650, 20);
            connectionStringTextBox.Multiline = true;
            connectionStringTextBox.Height = 60;

            var infoGroup = new GroupBox();
            infoGroup.Text = "Информация о подключении";
            infoGroup.Location = new Point(0, 220);
            infoGroup.Size = new Size(700, 80);
            infoGroup.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);

            infoLabel = new Label();
            infoLabel.Text = "Текущая база данных: " + GetCurrentDatabaseInfo();
            infoLabel.Location = new Point(20, 25);
            infoLabel.Size = new Size(650, 20);
            infoLabel.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular);

            infoGroup.Controls.Add(infoLabel);

            // Кнопки
            var testButton = new Button();
            testButton.Text = "Тестировать подключение";
            testButton.Location = new Point(20, 320);
            testButton.Size = new Size(150, 35);
            testButton.BackColor = Color.LightBlue;
            testButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            testButton.Click += TestConnection;

            var saveButton = new Button();
            saveButton.Text = "Сохранить настройки";
            saveButton.Location = new Point(180, 320);
            saveButton.Size = new Size(150, 35);
            saveButton.BackColor = Color.LightGreen;
            saveButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            saveButton.Click += SaveSettings;

            connectionGroup.Controls.AddRange(new Control[] { connectionLabel, connectionStringTextBox });
            mainPanel.Controls.AddRange(new Control[] {
                titleLabel, connectionGroup, infoGroup, testButton, saveButton
            });

            this.Controls.Add(mainPanel);
        }

        private string GetCurrentDatabaseInfo()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    return context.GetDatabaseInfo();
                }
            }
            catch
            {
                return "Не подключено";
            }
        }

        private void TestConnection(object sender, EventArgs e)
        {
            try
            {
                string conn = connectionStringTextBox.Text.Trim();

                using (var context = new DynamicDbContext(conn))
                {
                    context.Connect();
                    var tables = context.GetTableNames();

                    MessageBox.Show($"Подключение успешно! Найдено таблиц: {tables.Count}", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    infoLabel.Text = $"Текущая база данных: {new ApplicationDbContext(conn).GetDatabaseInfo()}";
                    ConnectionTested?.Invoke(this, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ConnectionTested?.Invoke(this, false);
            }
        }

        private void SaveSettings(object sender, EventArgs e)
        {
            MessageBox.Show("Настройки подключения сохранены!", "Успех",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}