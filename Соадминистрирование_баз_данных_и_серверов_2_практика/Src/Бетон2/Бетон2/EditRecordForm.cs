using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace Бетон2
{
    public class EditRecordForm : Form
    {
        private string _tableName;
        private DataRow _existingRow;
        private DynamicDbContext _context;
        private Panel fieldsPanel;
        private Dictionary<string, Control> _inputControls;
        private string _primaryKey;

        public EditRecordForm(string tableName, DataRow existingRow, DynamicDbContext context)
        {
            _tableName = tableName;
            _existingRow = existingRow;
            _context = context;
            _inputControls = new Dictionary<string, Control>();
            InitializeComponents();
            LoadTableSchema();
        }

        private void InitializeComponents()
        {
            this.Text = _existingRow == null ? $"Добавление записи в {_tableName}" : $"Изменение записи в {_tableName}";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);

            var titleLabel = new Label();
            titleLabel.Text = _existingRow == null ? "Добавление новой записи" : "Изменение записи";
            titleLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 40;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.BackColor = Color.SteelBlue;
            titleLabel.ForeColor = Color.White;

            fieldsPanel = new Panel();
            fieldsPanel.Dock = DockStyle.Fill;
            fieldsPanel.AutoScroll = true;
            fieldsPanel.Padding = new Padding(10);

            var buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 60;
            buttonPanel.BackColor = SystemColors.Control;

            var saveButton = new Button();
            saveButton.Text = _existingRow == null ? "Добавить" : "Изменить";
            saveButton.Size = new Size(100, 30);
            saveButton.Location = new Point(200, 15);
            saveButton.BackColor = Color.LightGreen;
            saveButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            saveButton.Click += SaveRecord;

            var cancelButton = new Button();
            cancelButton.Text = "Отмена";
            cancelButton.Size = new Size(100, 30);
            cancelButton.Location = new Point(310, 15);
            cancelButton.BackColor = Color.LightCoral;
            cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.AddRange(new Control[] { saveButton, cancelButton });
            mainPanel.Controls.AddRange(new Control[] { fieldsPanel, buttonPanel, titleLabel });
            this.Controls.Add(mainPanel);
        }

        private void LoadTableSchema()
        {
            try
            {
                var columns = _context.GetColumnsInfo(_tableName);
                _primaryKey = _context.GetPrimaryKey(_tableName);

                int yPos = 10;

                foreach (var column in columns)
                {
                    if (column.IsIdentity && _existingRow == null)
                        continue;

                    var label = new Label();
                    label.Text = GetDisplayName(column.Name);
                    label.Location = new Point(10, yPos);
                    label.Size = new Size(200, 20);
                    label.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);

                    Control inputControl = CreateInputControl(column, yPos + 25);
                    _inputControls[column.Name] = inputControl;

                    fieldsPanel.Controls.Add(label);
                    fieldsPanel.Controls.Add(inputControl);

                    yPos += 60;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки схемы таблицы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Control CreateInputControl(ColumnInfo column, int yPos)
        {
            object currentValue = null;
            if (_existingRow != null && _existingRow.Table.Columns.Contains(column.Name))
            {
                currentValue = _existingRow[column.Name];
                if (currentValue == DBNull.Value)
                    currentValue = null;
            }

            var netType = column.GetNetType();

            if (netType == typeof(bool))
            {
                var checkBox = new CheckBox();
                checkBox.Location = new Point(10, yPos);
                checkBox.Size = new Size(400, 25);
                checkBox.Checked = currentValue != null && Convert.ToBoolean(currentValue);
                checkBox.Tag = column;
                return checkBox;
            }
            else if (netType == typeof(DateTime))
            {
                var datePicker = new DateTimePicker();
                datePicker.Location = new Point(10, yPos);
                datePicker.Size = new Size(400, 25);
                datePicker.Value = currentValue != null ? Convert.ToDateTime(currentValue) : DateTime.Now;
                datePicker.Tag = column;
                return datePicker;
            }
            else if (netType == typeof(int) || netType == typeof(decimal) || netType == typeof(double))
            {
                var numericBox = new NumericUpDown();
                numericBox.Location = new Point(10, yPos);
                numericBox.Size = new Size(400, 25);

                if (currentValue != null)
                    numericBox.Value = Convert.ToDecimal(currentValue);

                numericBox.Tag = column;
                return numericBox;
            }
            else
            {
                var textBox = new TextBox();
                textBox.Location = new Point(10, yPos);
                textBox.Size = new Size(400, 25);
                textBox.Text = currentValue?.ToString() ?? "";
                textBox.Tag = column;

                if (column.MaxLength.HasValue && column.MaxLength > 0)
                    textBox.MaxLength = column.MaxLength.Value;

                return textBox;
            }
        }

        private string GetDisplayName(string columnName)
        {
            return columnName.Replace("_", " ");
        }

        private void SaveRecord(object sender, EventArgs e)
        {
            try
            {
                var values = new Dictionary<string, object>();

                foreach (var kvp in _inputControls)
                {
                    var columnName = kvp.Key;
                    var control = kvp.Value;
                    var columnInfo = (ColumnInfo)control.Tag;

                    if (columnInfo.IsIdentity && _existingRow == null)
                        continue;

                    object value = GetValueFromControl(control, columnInfo.GetNetType());

                    if (value == null && !columnInfo.IsNullable)
                    {
                        value = GetDefaultValue(columnInfo.GetNetType());
                    }

                    values[columnName] = value;
                }

                if (_existingRow == null)
                {
                    var newId = _context.InsertRecord(_tableName, values);
                    MessageBox.Show($"Запись добавлена успешно! ID: {newId}", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var id = _existingRow[_primaryKey];
                    var affected = _context.UpdateRecord(_tableName, values, _primaryKey, id);
                    MessageBox.Show($"Запись обновлена успешно! Затронуто записей: {affected}", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private object GetValueFromControl(Control control, Type targetType)
        {
            try
            {
                return control switch
                {
                    TextBox textBox => string.IsNullOrEmpty(textBox.Text) ? null : Convert.ChangeType(textBox.Text, targetType),
                    DateTimePicker datePicker => datePicker.Value,
                    NumericUpDown numeric => Convert.ChangeType(numeric.Value, targetType),
                    CheckBox checkBox => checkBox.Checked,
                    _ => null
                };
            }
            catch
            {
                return GetDefaultValue(targetType);
            }
        }

        private object GetDefaultValue(Type type)
        {
            if (type == typeof(string))
                return string.Empty;
            if (type == typeof(int) || type == typeof(decimal) || type == typeof(double))
                return 0;
            if (type == typeof(bool))
                return false;
            if (type == typeof(DateTime))
                return DateTime.Now;

            return null;
        }
    }
}