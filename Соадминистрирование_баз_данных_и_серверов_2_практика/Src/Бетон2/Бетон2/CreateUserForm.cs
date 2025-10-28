using System;
using System.Drawing;
using System.Windows.Forms;

namespace Бетон2
{
    public class CreateUserForm : Form
    {
        private TextBox userNameTextBox;
        private TextBox passwordTextBox;
        private TextBox confirmPasswordTextBox;
        private Button createButton;
        private Button cancelButton;
        private Label passwordMatchLabel;

        public string UserName => userNameTextBox.Text.Trim();
        public string Password => passwordTextBox.Text;

        public CreateUserForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Создание нового пользователя БД";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);

            var titleLabel = new Label();
            titleLabel.Text = "Создание пользователя базы данных";
            titleLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 40;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.BackColor = Color.SteelBlue;
            titleLabel.ForeColor = Color.White;

            var fieldsPanel = new Panel();
            fieldsPanel.Dock = DockStyle.Fill;
            fieldsPanel.Padding = new Padding(10);

            var userNameLabel = new Label();
            userNameLabel.Text = "Имя пользователя:";
            userNameLabel.Location = new Point(10, 20);
            userNameLabel.Size = new Size(150, 20);

            userNameTextBox = new TextBox();
            userNameTextBox.Location = new Point(160, 17);
            userNameTextBox.Size = new Size(200, 25);
            userNameTextBox.TextChanged += ValidateForm;

            var passwordLabel = new Label();
            passwordLabel.Text = "Пароль:";
            passwordLabel.Location = new Point(10, 60);
            passwordLabel.Size = new Size(150, 20);

            passwordTextBox = new TextBox();
            passwordTextBox.Location = new Point(160, 57);
            passwordTextBox.Size = new Size(200, 25);
            passwordTextBox.UseSystemPasswordChar = true;
            passwordTextBox.TextChanged += ValidateForm;

            var confirmPasswordLabel = new Label();
            confirmPasswordLabel.Text = "Подтверждение пароля:";
            confirmPasswordLabel.Location = new Point(10, 100);
            confirmPasswordLabel.Size = new Size(150, 20);

            confirmPasswordTextBox = new TextBox();
            confirmPasswordTextBox.Location = new Point(160, 97);
            confirmPasswordTextBox.Size = new Size(200, 25);
            confirmPasswordTextBox.UseSystemPasswordChar = true;
            confirmPasswordTextBox.TextChanged += ValidateForm;

            passwordMatchLabel = new Label();
            passwordMatchLabel.Location = new Point(10, 130);
            passwordMatchLabel.Size = new Size(350, 30);
            passwordMatchLabel.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular);
            passwordMatchLabel.Text = "Введите пароль и подтверждение";

            fieldsPanel.Controls.AddRange(new Control[] {
                userNameLabel, userNameTextBox,
                passwordLabel, passwordTextBox,
                confirmPasswordLabel, confirmPasswordTextBox,
                passwordMatchLabel
            });

            var buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 50;
            buttonPanel.BackColor = SystemColors.Control;

            createButton = new Button();
            createButton.Text = "Создать";
            createButton.Size = new Size(100, 30);
            createButton.Location = new Point(150, 10);
            createButton.BackColor = Color.LightGreen;
            createButton.Click += CreateButton_Click;
            createButton.Enabled = false;

            cancelButton = new Button();
            cancelButton.Text = "Отмена";
            cancelButton.Size = new Size(100, 30);
            cancelButton.Location = new Point(260, 10);
            cancelButton.BackColor = Color.LightCoral;
            cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.AddRange(new Control[] { createButton, cancelButton });

            mainPanel.Controls.AddRange(new Control[] { fieldsPanel, buttonPanel, titleLabel });
            this.Controls.Add(mainPanel);
        }

        private void ValidateForm(object sender, EventArgs e)
        {
            bool isUserNameValid = !string.IsNullOrWhiteSpace(UserName);
            bool isPasswordValid = !string.IsNullOrEmpty(Password);
            bool isConfirmPasswordValid = !string.IsNullOrEmpty(confirmPasswordTextBox.Text);
            bool passwordsMatch = Password == confirmPasswordTextBox.Text;
            bool passwordLengthValid = Password.Length >= 6;

            if (isPasswordValid && isConfirmPasswordValid)
            {
                if (passwordsMatch)
                {
                    passwordMatchLabel.Text = "✓ Пароли совпадают";
                    passwordMatchLabel.ForeColor = Color.Green;
                    confirmPasswordTextBox.BackColor = Color.LightGreen;
                }
                else
                {
                    passwordMatchLabel.Text = "✗ Пароли не совпадают";
                    passwordMatchLabel.ForeColor = Color.Red;
                    confirmPasswordTextBox.BackColor = Color.LightPink;
                }
            }
            else
            {
                passwordMatchLabel.Text = "Введите пароль и подтверждение";
                passwordMatchLabel.ForeColor = Color.Gray;
                confirmPasswordTextBox.BackColor = Color.White;
            }

            if (isPasswordValid)
            {
                passwordTextBox.BackColor = passwordLengthValid ? Color.LightGreen : Color.LightPink;
            }
            else
            {
                passwordTextBox.BackColor = Color.White;
            }

            createButton.Enabled = isUserNameValid &&
                                 isPasswordValid &&
                                 isConfirmPasswordValid &&
                                 passwordsMatch &&
                                 passwordLengthValid;
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            if (Password != confirmPasswordTextBox.Text)
            {
                MessageBox.Show("Пароли не совпадают! Пожалуйста, проверьте введенные пароли.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                confirmPasswordTextBox.Focus();
                confirmPasswordTextBox.SelectAll();
                return;
            }

            if (Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                passwordTextBox.Focus();
                passwordTextBox.SelectAll();
                return;
            }

            if (string.IsNullOrWhiteSpace(UserName))
            {
                MessageBox.Show("Введите имя пользователя!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                userNameTextBox.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}