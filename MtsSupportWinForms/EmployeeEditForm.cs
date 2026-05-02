using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class EmployeeEditForm : Form
    {
        private readonly int? _employeeId;
        private readonly UserRole _role;
        private readonly bool _readOnlyView;
        private readonly TextBox _txtFio = Theme.CreateTextBox(320);
        private readonly TextBox _txtPhone = Theme.CreateTextBox(320);
        private readonly ComboBox _cbPosition = Theme.CreateComboBox(320);

        public EmployeeEditForm(int? employeeId, UserRole role, bool readOnlyView = false)
        {
            _employeeId = employeeId;
            _role = role;
            _readOnlyView = readOnlyView;
            Theme.StyleForm(this);
            Text = employeeId.HasValue ? "Карточка сотрудника" : "Новый сотрудник";
            Width = 620;
            Height = 320;
            StartPosition = FormStartPosition.CenterParent;
            _txtFio.MaxLength = 120;
            _txtPhone.MaxLength = 20;

            var card = Theme.CreateCard();
            card.Dock = DockStyle.Fill;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "ФИО", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 0);
            layout.Controls.Add(_txtFio, 1, 0);
            layout.Controls.Add(new Label { Text = "Телефон", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 1);
            layout.Controls.Add(_txtPhone, 1, 1);
            layout.Controls.Add(new Label { Text = "Должность", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 2);
            layout.Controls.Add(_cbPosition, 1, 2);
            card.Controls.Add(layout);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft };
            var btnSave = Theme.CreatePrimaryButton("Сохранить", 120);
            var btnClose = Theme.CreateSecondaryButton("Закрыть", 120);
            btnSave.Click += delegate { Save(); };
            btnClose.Click += delegate { Close(); };
            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnClose);

            Controls.Add(card);
            Controls.Add(buttons);
            Load += delegate { LoadLookups(); LoadData(); ApplyRole(btnSave); ApplyReadOnly(btnSave); };
        }

        private void LoadLookups()
        {
            UiHelpers.BindLookup(_cbPosition, LookupService.Positions(), "position_id", "title_position", false);
        }

        private void LoadData()
        {
            if (_employeeId.HasValue)
            {
                var table = Db.Query("SELECT employee_id, fio, phone, position_id FROM Employee WHERE employee_id = @id", new SqlParameter("@id", _employeeId.Value));
                if (table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    _txtFio.Text = row["fio"].ToString();
                    _txtPhone.Text = row["phone"].ToString();
                    _cbPosition.SelectedValue = Convert.ToInt32(row["position_id"]);
                }
            }
        }

        private void ApplyRole(Button btnSave)
        {
            if (_role != UserRole.Administrator)
            {
                btnSave.Enabled = false;
                _txtFio.ReadOnly = true;
                _txtPhone.ReadOnly = true;
                _cbPosition.Enabled = false;
            }
        }

        private void ApplyReadOnly(Button btnSave)
        {
            if (!_readOnlyView) return;
            btnSave.Visible = false;
            _txtFio.ReadOnly = true;
            _txtPhone.ReadOnly = true;
            _cbPosition.Enabled = false;
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_txtFio.Text) || string.IsNullOrWhiteSpace(_txtPhone.Text) || _cbPosition.SelectedValue == null)
            {
                MessageBox.Show("Заполните все обязательные поля.");
                return;
            }
            if (!ValidationUtils.IsValidPersonName(_txtFio.Text))
            {
                MessageBox.Show("ФИО сотрудника должно содержать только буквы, пробелы или дефис.");
                return;
            }
            if (!ValidationUtils.IsValidPhone(_txtPhone.Text))
            {
                MessageBox.Show("Телефон сотрудника введен некорректно.");
                return;
            }
            if (!ValidationUtils.IsValidPhonePlus7(_txtPhone.Text))
            {
                MessageBox.Show("Телефон сотрудника должен быть в формате +7XXXXXXXXXX.");
                return;
            }

            try
            {
                if (_employeeId.HasValue)
                {
                    Db.Execute(@"UPDATE Employee SET fio=@fio, phone=@phone, position_id=@position_id WHERE employee_id=@id",
                        new SqlParameter("@fio", _txtFio.Text.Trim()),
                        new SqlParameter("@phone", _txtPhone.Text.Trim()),
                        new SqlParameter("@position_id", Convert.ToInt32(_cbPosition.SelectedValue)),
                        new SqlParameter("@id", _employeeId.Value));
                }
                else
                {
                    var nextId = Db.NextId("Employee", "employee_id");
                    Db.Execute(@"INSERT INTO Employee (employee_id, fio, phone, position_id) VALUES (@id, @fio, @phone, @position_id)",
                        new SqlParameter("@id", nextId),
                        new SqlParameter("@fio", _txtFio.Text.Trim()),
                        new SqlParameter("@phone", _txtPhone.Text.Trim()),
                        new SqlParameter("@position_id", Convert.ToInt32(_cbPosition.SelectedValue)));
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить сотрудника.\n" + ex.Message);
            }
        }
    }
}
