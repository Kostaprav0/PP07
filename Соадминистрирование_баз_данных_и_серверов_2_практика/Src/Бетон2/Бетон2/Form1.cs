using System;
using System.Drawing;
using System.Windows.Forms;

namespace Бетон2
{
    public class Form1 : Form
    {
        private ApplicationDbContext _context;
        private DynamicDbContext _dynamicContext;
        private Panel contentPanel;
        private Label headerLabel;

        public Form1()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Система обнаружения трещин в бетонных конструкциях";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            var mainTableLayout = new TableLayoutPanel();
            mainTableLayout.Dock = DockStyle.Fill;
            mainTableLayout.RowCount = 2;
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 90));
            mainTableLayout.ColumnCount = 1;

            headerLabel = new Label();
            headerLabel.Text = "СИСТЕМА УПРАВЛЕНИЯ БАЗОЙ ДАННЫХ";
            headerLabel.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold);
            headerLabel.ForeColor = Color.White;
            headerLabel.BackColor = Color.SteelBlue;
            headerLabel.TextAlign = ContentAlignment.MiddleCenter;
            headerLabel.Dock = DockStyle.Fill;

            var contentTableLayout = new TableLayoutPanel();
            contentTableLayout.Dock = DockStyle.Fill;
            contentTableLayout.ColumnCount = 2;
            contentTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            contentTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));

            var menuPanel = CreateMenuPanel();

            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = SystemColors.Control;
            contentPanel.Padding = new Padding(10);

            contentTableLayout.Controls.Add(menuPanel, 0, 0);
            contentTableLayout.Controls.Add(contentPanel, 1, 0);

            mainTableLayout.Controls.Add(headerLabel, 0, 0);
            mainTableLayout.Controls.Add(contentTableLayout, 0, 1);

            this.Controls.Add(mainTableLayout);

            ShowWelcomeScreen();
        }

        private Panel CreateMenuPanel()
        {
            var menuPanel = new Panel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.BackColor = Color.LightSteelBlue;
            menuPanel.Padding = new Padding(5);

            var menuLabel = new Label();
            menuLabel.Text = "МЕНЮ";
            menuLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            menuLabel.ForeColor = Color.DarkBlue;
            menuLabel.TextAlign = ContentAlignment.MiddleCenter;
            menuLabel.Dock = DockStyle.Top;
            menuLabel.Height = 40;
            menuLabel.BackColor = Color.SteelBlue;

            var settingsButton = CreateMenuButton("Настройки", ShowSettings);
            var tablesButton = CreateMenuButton("Таблицы", ShowTables);
            var usersButton = CreateMenuButton("Пользователи", ShowUsers);

            menuPanel.Controls.Add(usersButton);
            menuPanel.Controls.Add(tablesButton);
            menuPanel.Controls.Add(settingsButton);
            menuPanel.Controls.Add(menuLabel);

            return menuPanel;
        }

        private Button CreateMenuButton(string text, EventHandler clickHandler)
        {
            var button = new Button();
            button.Text = text;
            button.Dock = DockStyle.Top;
            button.Height = 50;
            button.Margin = new Padding(0, 5, 0, 0);
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.LightBlue;
            button.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(10, 0, 0, 0);
            button.Click += clickHandler;

            button.MouseEnter += (s, e) => { button.BackColor = Color.SteelBlue; button.ForeColor = Color.White; };
            button.MouseLeave += (s, e) => { button.BackColor = Color.LightBlue; button.ForeColor = Color.Black; };

            return button;
        }

        private void ShowWelcomeScreen()
        {
            ClearContentPanel();
            headerLabel.Text = "СИСТЕМА УПРАВЛЕНИЯ БАЗОЙ ДАННЫХ";

            var welcomePanel = new Panel();
            welcomePanel.Dock = DockStyle.Fill;
            welcomePanel.BackColor = Color.White;
            welcomePanel.Padding = new Padding(20);

            var welcomeLabel = new Label();
            welcomeLabel.Text = "Добро пожаловать в систему управления базой данных!\n\n" +
                               "Для начала работы выберите раздел в меню слева.";
            welcomeLabel.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Regular);
            welcomeLabel.TextAlign = ContentAlignment.MiddleCenter;
            welcomeLabel.Dock = DockStyle.Fill;

            welcomePanel.Controls.Add(welcomeLabel);
            contentPanel.Controls.Add(welcomePanel);
        }

        private void ShowSettings(object sender, EventArgs e)
        {
            ClearContentPanel();
            headerLabel.Text = "НАСТРОЙКИ ПОДКЛЮЧЕНИЯ К БАЗЕ ДАННЫХ";

            var settingsControl = new SettingsControl();
            settingsControl.Dock = DockStyle.Fill;
            settingsControl.ConnectionTested += (s, success) =>
            {
                if (success)
                {
                    try
                    {
                        _context = new ApplicationDbContext(settingsControl.ConnectionString);
                        _dynamicContext = new DynamicDbContext(settingsControl.ConnectionString);

                        _context.Database.Connection.Open();
                        _context.Database.Connection.Close();

                        MessageBox.Show("Подключение к базе данных успешно установлено!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            contentPanel.Controls.Add(settingsControl);
        }

        private void ShowTables(object sender, EventArgs e)
        {
            ClearContentPanel();
            headerLabel.Text = "РАБОТА С ТАБЛИЦАМИ";

            if (_context == null || _dynamicContext == null)
            {
                try
                {
                    _context = new ApplicationDbContext();
                    _dynamicContext = new DynamicDbContext(_context.Database.Connection.ConnectionString);

                    _context.Database.Connection.Open();
                    _context.Database.Connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось подключиться к базе данных: {ex.Message}\n\nПроверьте настройки подключения.", "Ошибка подключения",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ShowSettings(sender, e);
                    return;
                }
            }

            var tablesControl = new TablesControl(_dynamicContext);
            tablesControl.Dock = DockStyle.Fill;

            contentPanel.Controls.Add(tablesControl);
        }

        private void ShowUsers(object sender, EventArgs e)
        {
            ClearContentPanel();
            headerLabel.Text = "УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ И РОЛЯМИ";

            if (_dynamicContext == null)
            {
                MessageBox.Show("Сначала подключитесь к базе данных через раздел 'Настройки'", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ShowSettings(sender, e);
                return;
            }

            var usersControl = new UsersControl(_dynamicContext);
            usersControl.Dock = DockStyle.Fill;

            contentPanel.Controls.Add(usersControl);
        }

        private void ClearContentPanel()
        {
            contentPanel.Controls.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
                _dynamicContext?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}