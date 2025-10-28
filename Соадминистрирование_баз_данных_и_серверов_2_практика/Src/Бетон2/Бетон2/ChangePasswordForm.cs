using System;
using System.Drawing;
using System.Windows.Forms;

namespace Бетон2
{
    public class ChangePasswordForm : Form
    {
        private TextBox newPasswordTextBox;
        private TextBox confirmPasswordTextBox;
        private Button changeButton;
        private Button cancelButton;

        public string NewPassword => newPasswordTextBox.Text;

        public ChangePasswordForm(string userName)
        {
            InitializeComponents(userName);
        }

        private void InitializeComponents(string userName)
        {
            this.Text = $"Смена пароля пользователя {userName}";
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);

            var titleLabel = new Label();
            titleLabel.Text = $"Смена пароля для пользователя: {userName}";
            titleLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 40;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.BackColor = Color.SteelBlue;
            titleLabel.ForeColor = Color.White;

            var fieldsPanel = new Panel();
            fieldsPanel.Dock = DockStyle.Fill;
            fieldsPanel.Padding = new Padding(10);

            var newPasswordLabel = new Label();
            newPasswordLabel.Text = "Новый пароль:";
            newPasswordLabel.Location = new Point(10, 20);
            newPasswordLabel.Size = new Size(150, 20);

            newPasswordTextBox = new TextBox();
            newPasswordTextBox.Location = new Point(160, 17);
            newPasswordTextBox.Size = new Size(200, 25);
            newPasswordTextBox.UseSystemPasswordChar = true;

            var confirmPasswordLabel = new Label();
            confirmPasswordLabel.Text = "Подтверждение:";
            confirmPasswordLabel.Location = new Point(10, 60);
            confirmPasswordLabel.Size = new Size(150, 20);

            confirmPasswordTextBox = new TextBox();
            confirmPasswordTextBox.Location = new Point(160, 57);
            confirmPasswordTextBox.Size = new Size(200, 25);
            confirmPasswordTextBox.UseSystemPasswordChar = true;

            fieldsPanel.Controls.AddRange(new Control[] {
                newPasswordLabel, newPasswordTextBox,
                confirmPasswordLabel, confirmPasswordTextBox
            });

            var buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 50;
            buttonPanel.BackColor = SystemColors.Control;

            changeButton = new Button();
            changeButton.Text = "Изменить";
            changeButton.Size = new Size(100, 30);
            changeButton.Location = new Point(150, 10);
            changeButton.BackColor = Color.LightGreen;
            changeButton.Click += ChangeButton_Click;

            cancelButton = new Button();
            cancelButton.Text = "Отмена";
            cancelButton.Size = new Size(100, 30);
            cancelButton.Location = new Point(260, 10);
            cancelButton.BackColor = Color.LightCoral;
            cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.AddRange(new Control[] { changeButton, cancelButton });

            mainPanel.Controls.AddRange(new Control[] { fieldsPanel, buttonPanel, titleLabel });
            this.Controls.Add(mainPanel);
        }

        private void ChangeButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                MessageBox.Show("Введите новый пароль!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (NewPassword != confirmPasswordTextBox.Text)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (NewPassword.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}