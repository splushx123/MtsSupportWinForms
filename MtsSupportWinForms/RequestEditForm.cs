using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class RequestEditForm : Form
    {
        private readonly int? _requestId;
        private readonly UserRole _role;
        private readonly bool _readOnlyView;

        private readonly ComboBox _cbClient = Theme.CreateComboBox(340);
        private readonly ComboBox _cbEmployee = Theme.CreateComboBox(340);
        private readonly ComboBox _cbStatus = Theme.CreateComboBox(340);
        private readonly TextBox _txtDescription = Theme.CreateTextBox(340);
        private readonly DateTimePicker _dtRequest = new DateTimePicker { Width = 340, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm" };

        public RequestEditForm(int? requestId, UserRole role, bool readOnlyView = false)
        {
            _requestId = requestId;
            _role = role;
            _readOnlyView = readOnlyView;
            Theme.StyleForm(this);
            Text = requestId.HasValue ? "Карточка обращения" : "Новое обращение";
            Width = 700;
            Height = 430;
            StartPosition = FormStartPosition.CenterParent;
            _txtDescription.Multiline = true;
            _txtDescription.MaxLength = 2000;
            _txtDescription.Height = 110;

            var card = Theme.CreateCard();
            card.Dock = DockStyle.Fill;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var row = 0;
            layout.Controls.Add(new Label { Text = "Клиент", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, row);
            layout.Controls.Add(_cbClient, 1, row++);
            layout.Controls.Add(new Label { Text = "Сотрудник", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, row);
            layout.Controls.Add(_cbEmployee, 1, row++);
            if (_requestId.HasValue)
            {
                layout.Controls.Add(new Label { Text = "Статус", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, row);
                layout.Controls.Add(_cbStatus, 1, row++);
            }
            layout.Controls.Add(new Label { Text = "Описание", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, row);
            layout.Controls.Add(_txtDescription, 1, row++);
            layout.Controls.Add(new Label { Text = "Дата обращения", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, row);
            layout.Controls.Add(_dtRequest, 1, row);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft };
            var btnSave = Theme.CreatePrimaryButton("Сохранить", 120);
            var btnClose = Theme.CreateSecondaryButton("Закрыть", 120);
            btnSave.Click += delegate { Save(); };
            btnClose.Click += delegate { Close(); };
            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnClose);

            card.Controls.Add(layout);
            Controls.Add(card);
            Controls.Add(buttons);
            Load += delegate { LoadLookups(); LoadData(); ApplyRole(btnSave); ApplyReadOnly(btnSave); };
        }

        private void LoadLookups()
        {
            UiHelpers.BindLookup(_cbClient, LookupService.Clients(), "client_id", "fio", false);
            UiHelpers.BindLookup(_cbEmployee, LookupService.Employees(), "employee_id", "fio", true);
            UiHelpers.BindLookup(_cbStatus, LookupService.Statuses(), "status_id", "title_status", false);
        }

        private void LoadData()
        {
            if (_requestId.HasValue)
            {
                var table = Db.Query("SELECT request_id, client_id, employee_id, status_id, description, date_request FROM Request WHERE request_id = @id", new SqlParameter("@id", _requestId.Value));
                if (table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    _cbClient.SelectedValue = Convert.ToInt32(row["client_id"]);
                    if (row["employee_id"] != DBNull.Value) _cbEmployee.SelectedValue = Convert.ToInt32(row["employee_id"]);
                    _cbStatus.SelectedValue = Convert.ToInt32(row["status_id"]);
                    _txtDescription.Text = row["description"].ToString();
                    if (row["date_request"] != DBNull.Value) _dtRequest.Value = Convert.ToDateTime(row["date_request"]);
                }
            }
            else
            {
                _dtRequest.Value = DateTime.Now;
                _cbStatus.SelectedValue = GetInWorkStatusId();
            }
        }

        private void ApplyRole(Button btnSave)
        {
            if (_role == UserRole.OperatorLine1)
            {
                _cbEmployee.Enabled = false;
            }
            if (_role == UserRole.SpecialistLine2)
            {
                _cbClient.Enabled = false;
            }
        }

        private void ApplyReadOnly(Button btnSave)
        {
            if (!_readOnlyView) return;
            btnSave.Visible = false;
            _cbClient.Enabled = false;
            _cbEmployee.Enabled = false;
            _cbStatus.Enabled = false;
            _txtDescription.ReadOnly = true;
            _dtRequest.Enabled = false;
        }

        private void Save()
        {
            var statusId = UiHelpers.ComboValue(_cbStatus);
            if (_cbClient.SelectedValue == null || string.IsNullOrWhiteSpace(_txtDescription.Text))
            {
                MessageBox.Show("Заполните обязательные поля обращения.");
                return;
            }
            if (_requestId.HasValue && !statusId.HasValue)
            {
                MessageBox.Show("Для существующего обращения должен быть указан статус.");
                return;
            }
            if (_txtDescription.Text.Trim().Length < 10)
            {
                MessageBox.Show("Описание обращения должно содержать не менее 10 символов.");
                return;
            }
            if (ValidationUtils.IsFutureDate(_dtRequest.Value))
            {
                MessageBox.Show("Дата обращения не может быть в будущем.");
                return;
            }

            try
            {
                if (_requestId.HasValue)
                {
                    Db.Execute(@"UPDATE Request SET client_id=@client_id, employee_id=@employee_id, status_id=@status_id, description=@description, date_request=@date_request WHERE request_id=@id",
                        new SqlParameter("@client_id", Convert.ToInt32(_cbClient.SelectedValue)),
                        new SqlParameter("@employee_id", (object)UiHelpers.ComboValue(_cbEmployee) ?? DBNull.Value),
                        new SqlParameter("@status_id", statusId.Value),
                        new SqlParameter("@description", _txtDescription.Text.Trim()),
                        new SqlParameter("@date_request", _dtRequest.Value),
                        new SqlParameter("@id", _requestId.Value));
                }
                else
                {
                    var nextId = Db.NextId("Request", "request_id");
                    var defaultStatusId = statusId;
                    if (!defaultStatusId.HasValue)
                    {
                        defaultStatusId = GetInWorkStatusId();
                    }
                    Db.Execute(@"INSERT INTO Request (request_id, client_id, employee_id, status_id, description, date_request) VALUES (@id, @client_id, @employee_id, @status_id, @description, @date_request)",
                        new SqlParameter("@id", nextId),
                        new SqlParameter("@client_id", Convert.ToInt32(_cbClient.SelectedValue)),
                        new SqlParameter("@employee_id", (object)UiHelpers.ComboValue(_cbEmployee) ?? DBNull.Value),
                        new SqlParameter("@status_id", defaultStatusId.Value),
                        new SqlParameter("@description", _txtDescription.Text.Trim()),
                        new SqlParameter("@date_request", _dtRequest.Value));
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить обращение.\n" + ex.Message);
            }
        }

        private int GetInWorkStatusId()
        {
            var value = Db.Scalar("SELECT TOP 1 status_id FROM Status WHERE title_status = N'В работе'");
            if (value == null || value == DBNull.Value)
            {
                value = Db.Scalar("SELECT TOP 1 status_id FROM Status ORDER BY status_id");
            }
            return Convert.ToInt32(value);
        }
    }
}
