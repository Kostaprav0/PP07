using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace Бетон2
{
    public class UsersControl : UserControl
    {
        private DynamicDbContext _context;
        private TabControl usersTabControl;
        private DataGridView rolesGrid;
        private DataGridView usersGrid;
        private Button refreshButton;

        public UsersControl(DynamicDbContext context)
        {
            _context = context;
            InitializeComponents();
            LoadUsersData();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;

            refreshButton = new Button();
            refreshButton.Text = "Обновить данные";
            refreshButton.Size = new Size(120, 30);
            refreshButton.Location = new Point(650, 10);
            refreshButton.BackColor = Color.LightBlue;
            refreshButton.Click += (s, e) => LoadUsersData();

            usersTabControl = new TabControl();
            usersTabControl.Dock = DockStyle.Fill;
            usersTabControl.Location = new Point(0, 40);
            usersTabControl.Size = new Size(800, 500);

            var usersTab = new TabPage("Пользователи БД");
            usersTab.BackColor = Color.White;
            InitializeUsersTab(usersTab);

            var rolesTab = new TabPage("Роли БД");
            rolesTab.BackColor = Color.White;
            InitializeRolesTab(rolesTab);

            usersTabControl.Controls.AddRange(new TabPage[] { usersTab, rolesTab });

            mainPanel.Controls.AddRange(new Control[] { refreshButton, usersTabControl });
            this.Controls.Add(mainPanel);
        }

        private void InitializeUsersTab(TabPage tabPage)
        {
            var usersPanel = new Panel();
            usersPanel.Dock = DockStyle.Fill;
            usersPanel.Padding = new Padding(10);

            var usersLabel = new Label();
            usersLabel.Text = "Управление пользователями базы данных";
            usersLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            usersLabel.Location = new Point(10, 10);
            usersLabel.Size = new Size(400, 25);

            usersGrid = new DataGridView();
            usersGrid.Location = new Point(10, 50);
            usersGrid.Size = new Size(750, 300);
            usersGrid.BackgroundColor = Color.White;
            usersGrid.ReadOnly = true;
            usersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            usersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            var addUserButton = new Button();
            addUserButton.Text = "Создать пользователя";
            addUserButton.Location = new Point(10, 360);
            addUserButton.Size = new Size(150, 35);
            addUserButton.BackColor = Color.LightGreen;
            addUserButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            addUserButton.Click += (s, e) => CreateDatabaseUser();

            var deleteUserButton = new Button();
            deleteUserButton.Text = "Удалить пользователя";
            deleteUserButton.Location = new Point(170, 360);
            deleteUserButton.Size = new Size(150, 35);
            deleteUserButton.BackColor = Color.LightCoral;
            deleteUserButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            deleteUserButton.Click += (s, e) => DeleteDatabaseUser();

            var changePasswordButton = new Button();
            changePasswordButton.Text = "Сменить пароль";
            changePasswordButton.Location = new Point(330, 360);
            changePasswordButton.Size = new Size(150, 35);
            changePasswordButton.BackColor = Color.LightBlue;
            changePasswordButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            changePasswordButton.Click += (s, e) => ChangeUserPassword();

            usersPanel.Controls.AddRange(new Control[] {
                usersLabel, usersGrid, addUserButton, deleteUserButton, changePasswordButton
            });

            tabPage.Controls.Add(usersPanel);
        }

        private void InitializeRolesTab(TabPage tabPage)
        {
            var rolesPanel = new Panel();
            rolesPanel.Dock = DockStyle.Fill;
            rolesPanel.Padding = new Padding(10);

            var rolesLabel = new Label();
            rolesLabel.Text = "Управление ролями базы данных";
            rolesLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            rolesLabel.Location = new Point(10, 10);
            rolesLabel.Size = new Size(400, 25);

            rolesGrid = new DataGridView();
            rolesGrid.Location = new Point(10, 50);
            rolesGrid.Size = new Size(750, 300);
            rolesGrid.BackgroundColor = Color.White;
            rolesGrid.ReadOnly = true;
            rolesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            rolesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            var addRoleButton = new Button();
            addRoleButton.Text = "Создать роль";
            addRoleButton.Location = new Point(10, 360);
            addRoleButton.Size = new Size(120, 35);
            addRoleButton.BackColor = Color.LightGreen;
            addRoleButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            addRoleButton.Click += (s, e) => CreateDatabaseRole();

            var deleteRoleButton = new Button();
            deleteRoleButton.Text = "Удалить роль";
            deleteRoleButton.Location = new Point(140, 360);
            deleteRoleButton.Size = new Size(120, 35);
            deleteRoleButton.BackColor = Color.LightCoral;
            deleteRoleButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            deleteRoleButton.Click += (s, e) => DeleteDatabaseRole();

            var assignRoleButton = new Button();
            assignRoleButton.Text = "Назначить роль";
            assignRoleButton.Location = new Point(270, 360);
            assignRoleButton.Size = new Size(120, 35);
            assignRoleButton.BackColor = Color.LightBlue;
            assignRoleButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            assignRoleButton.Click += (s, e) => AssignRoleToUser();

            var revokeRoleButton = new Button();
            revokeRoleButton.Text = "Отозвать роль";
            revokeRoleButton.Location = new Point(400, 360);
            revokeRoleButton.Size = new Size(120, 35);
            revokeRoleButton.BackColor = Color.LightYellow;
            revokeRoleButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            revokeRoleButton.Click += (s, e) => RevokeRoleFromUser();

            rolesPanel.Controls.AddRange(new Control[] {
                rolesLabel, rolesGrid, addRoleButton, deleteRoleButton, assignRoleButton, revokeRoleButton
            });

            tabPage.Controls.Add(rolesPanel);
        }

        public void LoadUsersData()
        {
            try
            {
                var usersData = _context.GetDatabaseUsers();
                usersGrid.DataSource = usersData;

                var rolesData = _context.GetDatabaseRoles();
                rolesGrid.DataSource = rolesData;

                MessageBox.Show("Данные успешно обновлены!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateDatabaseUser()
        {
            try
            {
                using (var form = new CreateUserForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        _context.CreateDatabaseUser(form.UserName, form.Password);
                        MessageBox.Show($"Пользователь '{form.UserName}' успешно создан!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadUsersData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания пользователя: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteDatabaseUser()
        {
            if (usersGrid.CurrentRow == null)
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userName = usersGrid.CurrentRow.Cells["name"].Value?.ToString();
            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("Не удалось определить имя пользователя!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{userName}'?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _context.DeleteDatabaseUser(userName);
                    MessageBox.Show($"Пользователь '{userName}' успешно удален!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUsersData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ChangeUserPassword()
        {
            if (usersGrid.CurrentRow == null)
            {
                MessageBox.Show("Выберите пользователя для смены пароля!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userName = usersGrid.CurrentRow.Cells["name"].Value?.ToString();
            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("Не удалось определить имя пользователя!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var form = new ChangePasswordForm(userName))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        _context.ChangeUserPassword(userName, form.NewPassword);
                        MessageBox.Show($"Пароль пользователя '{userName}' успешно изменен!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка смены пароля: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateDatabaseRole()
        {
            try
            {
                var roleName = Interaction.InputBox(
                    "Введите название новой роли:", "Создание роли", "");

                if (!string.IsNullOrEmpty(roleName))
                {
                    _context.CreateDatabaseRole(roleName);
                    MessageBox.Show($"Роль '{roleName}' успешно создана!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUsersData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания роли: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteDatabaseRole()
        {
            if (rolesGrid.CurrentRow == null)
            {
                MessageBox.Show("Выберите роль для удаления!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var roleName = rolesGrid.CurrentRow.Cells["name"].Value?.ToString();
            if (string.IsNullOrEmpty(roleName))
            {
                MessageBox.Show("Не удалось определить название роли!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить роль '{roleName}'?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _context.DeleteDatabaseRole(roleName);
                    MessageBox.Show($"Роль '{roleName}' успешно удалена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUsersData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления роли: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AssignRoleToUser()
        {
            if (usersGrid.CurrentRow == null || rolesGrid.CurrentRow == null)
            {
                MessageBox.Show("Выберите пользователя и роль!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userName = usersGrid.CurrentRow.Cells["name"].Value?.ToString();
            var roleName = rolesGrid.CurrentRow.Cells["name"].Value?.ToString();

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(roleName))
            {
                MessageBox.Show("Не удалось определить пользователя или роль!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _context.AssignRoleToUser(userName, roleName);
                MessageBox.Show($"Роль '{roleName}' успешно назначена пользователю '{userName}'!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка назначения роли: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RevokeRoleFromUser()
        {
            if (usersGrid.CurrentRow == null || rolesGrid.CurrentRow == null)
            {
                MessageBox.Show("Выберите пользователя и роль!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userName = usersGrid.CurrentRow.Cells["name"].Value?.ToString();
            var roleName = rolesGrid.CurrentRow.Cells["name"].Value?.ToString();

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(roleName))
            {
                MessageBox.Show("Не удалось определить пользователя или роль!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _context.RevokeRoleFromUser(userName, roleName);
                MessageBox.Show($"Роль '{roleName}' успешно отозвана у пользователя '{userName}'!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отзыва роли: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}