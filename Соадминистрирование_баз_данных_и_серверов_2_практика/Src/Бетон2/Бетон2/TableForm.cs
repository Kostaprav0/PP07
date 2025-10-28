using System;
using System.Drawing;
using System.Windows.Forms;

namespace Бетон2
{
    public class TableForm : Form
    {
        private ApplicationDbContext _context;
        private string _tableName;

        public TableForm(string tableName, ApplicationDbContext context)
        {
            _tableName = tableName;
            _context = context;
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = $"Таблица: {_tableName}";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var label = new Label();
            label.Text = $"Работа с таблицей: {_tableName}\n\nЗдесь будет DataGridView с данными";
            label.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Regular);
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Dock = DockStyle.Fill;

            var closeButton = new Button();
            closeButton.Text = "Закрыть";
            closeButton.Size = new Size(100, 30);
            closeButton.Location = new Point(350, 500);
            closeButton.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { label, closeButton });
        }
    }
}