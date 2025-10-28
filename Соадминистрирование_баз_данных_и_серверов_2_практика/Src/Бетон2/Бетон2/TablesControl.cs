using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace Бетон2
{
    public class TablesControl : UserControl
    {
        private DynamicDbContext _context;
        private ComboBox tablesComboBox;
        private DataGridView dataGridView;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private Label statusLabel;

        public TablesControl(DynamicDbContext context)
        {
            _context = context;
            InitializeComponents();
            LoadTablesFromDatabase();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;

            var titlePanel = new Panel();
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 60;
            titlePanel.BackColor = Color.LightSteelBlue;

            var titleLabel = new Label();
            titleLabel.Text = "РАБОТА С ТАБЛИЦАМИ";
            titleLabel.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold);
            titleLabel.ForeColor = Color.DarkBlue;
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;

            titlePanel.Controls.Add(titleLabel);

            var tableSelectPanel = new Panel();
            tableSelectPanel.Dock = DockStyle.Top;
            tableSelectPanel.Height = 80;
            tableSelectPanel.BackColor = SystemColors.Control;
            tableSelectPanel.Padding = new Padding(10);

            var tableLabel = new Label();
            tableLabel.Text = "Выберите таблицу:";
            tableLabel.Location = new Point(20, 15);
            tableLabel.Size = new Size(120, 20);
            tableLabel.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);

            tablesComboBox = new ComboBox();
            tablesComboBox.Location = new Point(150, 12);
            tablesComboBox.Size = new Size(250, 25);
            tablesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            tablesComboBox.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
            tablesComboBox.SelectedIndexChanged += TableSelected;

            statusLabel = new Label();
            statusLabel.Text = "Загрузка таблиц...";
            statusLabel.Location = new Point(420, 15);
            statusLabel.Size = new Size(300, 20);
            statusLabel.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Italic);
            statusLabel.ForeColor = Color.DarkGray;

            tableSelectPanel.Controls.AddRange(new Control[] { tableLabel, tablesComboBox, statusLabel });

            var buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Top;
            buttonPanel.Height = 50;
            buttonPanel.BackColor = Color.LightGray;
            buttonPanel.Padding = new Padding(10);

            addButton = new Button();
            addButton.Text = "Добавить";
            addButton.Location = new Point(20, 10);
            addButton.Size = new Size(100, 30);
            addButton.BackColor = Color.LightGreen;
            addButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            addButton.Click += AddRecord;
            addButton.Enabled = false;

            editButton = new Button();
            editButton.Text = "Изменить";
            editButton.Location = new Point(130, 10);
            editButton.Size = new Size(100, 30);
            editButton.BackColor = Color.LightBlue;
            editButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            editButton.Click += EditRecord;
            editButton.Enabled = false;

            deleteButton = new Button();
            deleteButton.Text = "Удалить";
            deleteButton.Location = new Point(240, 10);
            deleteButton.Size = new Size(100, 30);
            deleteButton.BackColor = Color.LightCoral;
            deleteButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            deleteButton.Click += DeleteRecord;
            deleteButton.Enabled = false;

            buttonPanel.Controls.AddRange(new Control[] { addButton, editButton, deleteButton });

            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.ReadOnly = true;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);

            mainPanel.Controls.AddRange(new Control[] {
                dataGridView,
                buttonPanel,
                tableSelectPanel,
                titlePanel
            });

            this.Controls.Add(mainPanel);
        }

        private void LoadTablesFromDatabase()
        {
            try
            {
                statusLabel.Text = "Загрузка списка таблиц...";

                if (_context == null)
                {
                    statusLabel.Text = "Ошибка: контекст базы данных не инициализирован";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }

                var tableNames = _context.GetTableNames();

                tablesComboBox.Items.Clear();

                if (tableNames.Count == 0)
                {
                    statusLabel.Text = "Таблицы не найдены";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }

                foreach (var table in tableNames)
                {
                    tablesComboBox.Items.Add(table);
                }

                tablesComboBox.SelectedIndex = 0;

                statusLabel.Text = $"Найдено таблиц: {tableNames.Count}";
                statusLabel.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Ошибка загрузки таблиц: {ex.Message}";
                statusLabel.ForeColor = Color.Red;

                MessageBox.Show($"Ошибка работы с базой данных:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TableSelected(object sender, EventArgs e)
        {
            if (tablesComboBox.SelectedItem == null) return;

            var tableName = tablesComboBox.SelectedItem.ToString();

            try
            {
                LoadTableData(tableName);
                addButton.Enabled = true;
                editButton.Enabled = true;
                deleteButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTableData(string tableName)
        {
            try
            {
                var dataTable = _context.GetTableData(tableName);
                dataGridView.DataSource = dataTable;

                if (dataGridView.Columns.Count > 0)
                {
                    dataGridView.AutoResizeColumns();
                    statusLabel.Text = $"Загружено {dataTable.Rows.Count} записей из таблицы '{tableName}'";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось загрузить данные таблицы {tableName}: {ex.Message}");
            }
        }

        private void AddRecord(object sender, EventArgs e)
        {
            if (tablesComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tableName = tablesComboBox.SelectedItem.ToString();
            var editForm = new EditRecordForm(tableName, null, _context);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadTableData(tableName);
            }
        }

        private void EditRecord(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tableName = tablesComboBox.SelectedItem.ToString();
            var selectedRow = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;
            var editForm = new EditRecordForm(tableName, selectedRow, _context);

            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadTableData(tableName);
            }
        }

        private void DeleteRecord(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для удаления!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var tableName = tablesComboBox.SelectedItem.ToString();
                    var selectedRow = ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row;
                    var primaryKey = _context.GetPrimaryKey(tableName);
                    var id = selectedRow[primaryKey];

                    var affected = _context.DeleteRecord(tableName, primaryKey, id);

                    if (affected > 0)
                    {
                        LoadTableData(tableName);
                        MessageBox.Show("Запись удалена успешно!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}