using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class EmployeesForm : Form
    {
        private readonly UserAccount _user;
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly TextBox _txtSearch = Theme.CreateTextBox(250);

        public EmployeesForm(UserAccount user)
        {
            _user = user;
            Theme.StyleForm(this);
            Text = "Сотрудники";
            Width = 1100;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            Theme.StyleGrid(_grid);
            _txtSearch.MaxLength = 100;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(10), BackColor = Color.White };
            top.Controls.Add(new Label { Text = "Поиск:", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
            top.Controls.Add(_txtSearch);

            var btnAdd = Theme.CreatePrimaryButton("Добавить", 110);
            var btnEdit = Theme.CreateSecondaryButton("Изменить", 110);
            var btnDelete = Theme.CreateSecondaryButton("Удалить", 110);
            var btnCard = Theme.CreateSecondaryButton("Карточка", 110);

            _txtSearch.TextChanged += delegate { LoadData(); };
            btnAdd.Click += delegate { OpenEditor(null); };
            btnEdit.Click += delegate { EditSelected(); };
            btnCard.Click += delegate { OpenCardSelected(); };
            btnDelete.Click += delegate { DeleteSelected(); };

            top.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnCard });
            Controls.Add(_grid);
            Controls.Add(top);
            Load += delegate { LoadData(); };
        }

        private void LoadData()
        {
            var value = _txtSearch.Text.Trim();
            if (!ValidationUtils.IsValidSearchText(value))
            {
                MessageBox.Show("Поисковая строка содержит недопустимые символы.", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var text = value.Length == 0 ? "%" : value + "%";
            _grid.DataSource = Db.Query(@"
SELECT e.employee_id AS [Код], e.fio AS [ФИО], e.phone AS [Телефон], p.title_position AS [Должность]
FROM Employee e
LEFT JOIN Position p ON p.position_id = e.position_id
WHERE e.fio LIKE @search OR e.phone LIKE @search OR ISNULL(p.title_position,'') LIKE @search
ORDER BY e.fio", new SqlParameter("@search", text));
            if (_grid.Columns.Count > 0) _grid.Columns[0].Visible = false;
        }

        private int? SelectedId()
        {
            if (_grid.CurrentRow == null) return null;
            return Convert.ToInt32(_grid.CurrentRow.Cells[0].Value);
        }

        private void EditSelected()
        {
            var id = SelectedId();
            if (!id.HasValue) return;
            OpenEditor(id.Value);
        }

        private void OpenCardSelected()
        {
            var id = SelectedId();
            if (!id.HasValue) return;
            OpenEditor(id.Value, true);
        }

        private void OpenEditor(int? id, bool readOnlyView = false)
        {
            using (var form = new EmployeeEditForm(id, _user.Role, readOnlyView))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void DeleteSelected()
        {
            if (_user.Role != UserRole.Administrator)
            {
                MessageBox.Show("Только администратор может удалять сотрудников.");
                return;
            }

            var id = SelectedId();
            if (!id.HasValue) return;
            if (MessageBox.Show("Удалить сотрудника?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                Db.Execute("DELETE FROM Employee WHERE employee_id = @id", new SqlParameter("@id", id.Value));
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить сотрудника.\n" + ex.Message);
            }
        }
    }
}
