using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class RequestsForm : Form
    {
        private readonly UserAccount _user;
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly TextBox _txtSearch = Theme.CreateTextBox(180);
        private readonly ComboBox _cbStatus = Theme.CreateComboBox(180);

        public RequestsForm(UserAccount user)
        {
            _user = user;
            Theme.StyleForm(this);
            Text = "Обращения";
            Width = 1200;
            Height = 680;
            StartPosition = FormStartPosition.CenterParent;
            Theme.StyleGrid(_grid);
            _txtSearch.MaxLength = 100;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10), BackColor = Color.White };
            top.Controls.Add(new Label { Text = "Поиск:", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
            top.Controls.Add(_txtSearch);
            top.Controls.Add(new Label { Text = "Статус:", AutoSize = true, Padding = new Padding(8, 9, 0, 0) });
            top.Controls.Add(_cbStatus);

            var btnAdd = Theme.CreatePrimaryButton("Создать", 100);
            var btnEdit = Theme.CreateSecondaryButton("Изменить", 100);
            var btnDelete = Theme.CreateSecondaryButton("Удалить", 100);
            var btnCard = Theme.CreateSecondaryButton("Карточка", 100);

            _txtSearch.TextChanged += delegate { LoadData(); };
            _cbStatus.SelectedIndexChanged += delegate { LoadData(); };
            btnAdd.Click += delegate { OpenEditor(null); };
            btnEdit.Click += delegate { EditSelected(); };
            btnCard.Click += delegate { OpenCardSelected(); };
            btnDelete.Click += delegate { DeleteSelected(); };

            top.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnCard });
            Controls.Add(_grid);
            Controls.Add(top);
            Load += delegate { LoadStatuses(); LoadData(); };
        }

        private void LoadStatuses()
        {
            UiHelpers.BindLookup(_cbStatus, LookupService.Statuses(), "status_id", "title_status", true);
        }

        private void LoadData()
        {
            var value = _txtSearch.Text.Trim();
            if (!ValidationUtils.IsValidSearchText(value))
            {
                MessageBox.Show("Поисковая строка содержит недопустимые символы.", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var search = value.Length == 0 ? "%" : value + "%";
            var statusId = UiHelpers.ComboValue(_cbStatus);
            _grid.DataSource = Db.Query(@"
SELECT r.request_id AS [Код], c.fio AS [Клиент], e.fio AS [Сотрудник], s.title_status AS [Статус],
       r.description AS [Описание], r.date_request AS [Дата обращения]
FROM Request r
INNER JOIN Client c ON c.client_id = r.client_id
LEFT JOIN Employee e ON e.employee_id = r.employee_id
INNER JOIN Status s ON s.status_id = r.status_id
WHERE (c.fio LIKE @search OR ISNULL(r.description,'') LIKE @search)
  AND (@statusId IS NULL OR r.status_id = @statusId)
ORDER BY r.date_request DESC, r.request_id DESC",
                new SqlParameter("@search", search),
                new SqlParameter("@statusId", (object)statusId ?? DBNull.Value));
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
            using (var form = new RequestEditForm(id, _user.Role, readOnlyView))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void DeleteSelected()
        {
            if (_user.Role == UserRole.SpecialistLine2)
            {
                MessageBox.Show("У этой роли нет прав на удаление обращений.");
                return;
            }

            var id = SelectedId();
            if (!id.HasValue) return;
            if (MessageBox.Show("Удалить обращение?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                Db.Execute("DELETE FROM Request WHERE request_id = @id", new SqlParameter("@id", id.Value));
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить обращение.\n" + ex.Message);
            }
        }
    }
}
