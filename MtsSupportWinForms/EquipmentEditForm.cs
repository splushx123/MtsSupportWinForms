using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class EquipmentEditForm : Form
    {
        private readonly int? _equipmentId;
        private readonly UserRole _role;
        private readonly Label _lblId = new Label { AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
        private readonly TextBox _txtSerial = Theme.CreateTextBox(320);
        private readonly ComboBox _cbModel = Theme.CreateComboBox(320);
        private readonly ComboBox _cbClient = Theme.CreateComboBox(320);

        public EquipmentEditForm(int? equipmentId, UserRole role)
        {
            _equipmentId = equipmentId;
            _role = role;
            Theme.StyleForm(this);
            Text = equipmentId.HasValue ? "Карточка оборудования" : "Новое оборудование";
            Width = 620;
            Height = 320;
            StartPosition = FormStartPosition.CenterParent;

            var card = Theme.CreateCard();
            card.Dock = DockStyle.Fill;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "Код оборудования", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 0);
            layout.Controls.Add(_lblId, 1, 0);
            layout.Controls.Add(new Label { Text = "Серийный номер", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 1);
            layout.Controls.Add(_txtSerial, 1, 1);
            layout.Controls.Add(new Label { Text = "Модель", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 2);
            layout.Controls.Add(_cbModel, 1, 2);
            layout.Controls.Add(new Label { Text = "Клиент", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 3);
            layout.Controls.Add(_cbClient, 1, 3);
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
            Load += delegate { LoadLookups(); LoadData(); ApplyRole(btnSave); };
        }

        private void LoadLookups()
        {
            UiHelpers.BindLookup(_cbModel, LookupService.Models(), "model_id", "title_model", false);
            UiHelpers.BindLookup(_cbClient, LookupService.Clients(), "client_id", "fio", true);
        }

        private void LoadData()
        {
            if (_equipmentId.HasValue)
            {
                var table = Db.Query("SELECT equipment_id, serial_number, model_id, client_id FROM Equipment WHERE equipment_id = @id", new SqlParameter("@id", _equipmentId.Value));
                if (table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    _lblId.Text = row["equipment_id"].ToString();
                    _txtSerial.Text = row["serial_number"].ToString();
                    _cbModel.SelectedValue = Convert.ToInt32(row["model_id"]);
                    if (row["client_id"] != DBNull.Value) _cbClient.SelectedValue = Convert.ToInt32(row["client_id"]);
                }
            }
            else
            {
                _lblId.Text = Db.NextId("Equipment", "equipment_id").ToString();
            }
        }

        private void ApplyRole(Button btnSave)
        {
            if (_role == UserRole.OperatorLine1)
            {
                btnSave.Enabled = false;
                _txtSerial.ReadOnly = true;
                _cbModel.Enabled = false;
                _cbClient.Enabled = false;
            }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_txtSerial.Text) || _cbModel.SelectedValue == null)
            {
                MessageBox.Show("Заполните серийный номер и модель.");
                return;
            }

            try
            {
                if (_equipmentId.HasValue)
                {
                    Db.Execute(@"UPDATE Equipment SET serial_number=@serial, model_id=@model_id, client_id=@client_id WHERE equipment_id=@id",
                        new SqlParameter("@serial", _txtSerial.Text.Trim()),
                        new SqlParameter("@model_id", Convert.ToInt32(_cbModel.SelectedValue)),
                        new SqlParameter("@client_id", (object)UiHelpers.ComboValue(_cbClient) ?? DBNull.Value),
                        new SqlParameter("@id", _equipmentId.Value));
                }
                else
                {
                    Db.Execute(@"INSERT INTO Equipment (equipment_id, serial_number, model_id, client_id) VALUES (@id, @serial, @model_id, @client_id)",
                        new SqlParameter("@id", Convert.ToInt32(_lblId.Text)),
                        new SqlParameter("@serial", _txtSerial.Text.Trim()),
                        new SqlParameter("@model_id", Convert.ToInt32(_cbModel.SelectedValue)),
                        new SqlParameter("@client_id", (object)UiHelpers.ComboValue(_cbClient) ?? DBNull.Value));
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить оборудование.\n" + ex.Message);
            }
        }
    }
}
