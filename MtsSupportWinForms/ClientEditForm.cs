using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class ClientEditForm : Form
    {
        private readonly int? _clientId;
        private readonly UserRole _role;

        private readonly Label _lblIdValue = new Label { AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
        private readonly TextBox _txtFio = Theme.CreateTextBox(360);
        private readonly TextBox _txtPhone = Theme.CreateTextBox(360);
        private readonly TextBox _txtAddress = Theme.CreateTextBox(360);
        private readonly TextBox _txtEmail = Theme.CreateTextBox(360);
        private readonly DataGridView _gridRequests = new DataGridView { Dock = DockStyle.Fill, Height = 160 };
        private readonly DataGridView _gridEquipment = new DataGridView { Dock = DockStyle.Fill, Height = 160 };

        public ClientEditForm(int? clientId, UserRole role)
        {
            _clientId = clientId;
            _role = role;
            Theme.StyleForm(this);
            Text = clientId.HasValue ? "Карточка клиента" : "Новый клиент";
            Width = 950;
            Height = 720;
            StartPosition = FormStartPosition.CenterParent;
            Theme.StyleGrid(_gridRequests);
            Theme.StyleGrid(_gridEquipment);

            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var card = Theme.CreateCard();
            card.Dock = DockStyle.Top;
            card.Height = 250;

            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            form.Controls.Add(new Label { Text = "Код клиента", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 0);
            form.Controls.Add(_lblIdValue, 1, 0);
            form.Controls.Add(new Label { Text = "ФИО", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 1);
            form.Controls.Add(_txtFio, 1, 1);
            form.Controls.Add(new Label { Text = "Телефон", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 2);
            form.Controls.Add(_txtPhone, 1, 2);
            form.Controls.Add(new Label { Text = "Адрес", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 3);
            form.Controls.Add(_txtAddress, 1, 3);
            form.Controls.Add(new Label { Text = "Почта", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 4);
            form.Controls.Add(_txtEmail, 1, 4);
            card.Controls.Add(form);

            var requestsCard = Theme.CreateCard();
            requestsCard.Dock = DockStyle.Top;
            requestsCard.Height = 200;
            requestsCard.Controls.Add(_gridRequests);
            requestsCard.Controls.Add(new Label { Text = "История обращений клиента", Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10F, FontStyle.Bold) });

            var equipmentCard = Theme.CreateCard();
            equipmentCard.Dock = DockStyle.Fill;
            equipmentCard.Controls.Add(_gridEquipment);
            equipmentCard.Controls.Add(new Label { Text = "Оборудование клиента", Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10F, FontStyle.Bold) });

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, FlowDirection = FlowDirection.RightToLeft };
            var btnSave = Theme.CreatePrimaryButton("Сохранить", 130);
            var btnClose = Theme.CreateSecondaryButton("Закрыть", 130);
            btnSave.Click += delegate { Save(); };
            btnClose.Click += delegate { Close(); };
            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnClose);

            root.Controls.Add(equipmentCard);
            root.Controls.Add(requestsCard);
            root.Controls.Add(card);
            root.Controls.Add(buttons);
            Controls.Add(root);
            Load += delegate { LoadData(); };

            if (_role == UserRole.SpecialistLine2)
            {
                _txtFio.ReadOnly = true;
                _txtPhone.ReadOnly = true;
                _txtAddress.ReadOnly = true;
                _txtEmail.ReadOnly = true;
                btnSave.Enabled = false;
            }
        }

        private void LoadData()
        {
            if (_clientId.HasValue)
            {
                var table = Db.Query("SELECT client_id, fio, phone, address, email FROM Client WHERE client_id = @id", new SqlParameter("@id", _clientId.Value));
                if (table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    _lblIdValue.Text = row["client_id"].ToString();
                    _txtFio.Text = row["fio"].ToString();
                    _txtPhone.Text = row["phone"].ToString();
                    _txtAddress.Text = row["address"].ToString();
                    _txtEmail.Text = row["email"].ToString();
                }

                _gridRequests.DataSource = Db.Query(@"
SELECT r.request_id AS [Код], s.title_status AS [Статус], r.description AS [Описание], r.date_request AS [Дата]
FROM Request r
INNER JOIN Status s ON s.status_id = r.status_id
WHERE r.client_id = @id
ORDER BY r.date_request DESC", new SqlParameter("@id", _clientId.Value));

                _gridEquipment.DataSource = Db.Query(@"
SELECT e.equipment_id AS [Код], e.serial_number AS [Серийный номер], m.title_model AS [Модель]
FROM Equipment e
LEFT JOIN Model m ON m.model_id = e.model_id
WHERE e.client_id = @id
ORDER BY e.equipment_id", new SqlParameter("@id", _clientId.Value));
            }
            else
            {
                _lblIdValue.Text = Db.NextId("Client", "client_id").ToString();
            }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_txtFio.Text) || string.IsNullOrWhiteSpace(_txtPhone.Text))
            {
                MessageBox.Show("Заполните обязательные поля: ФИО и телефон.");
                return;
            }

            try
            {
                if (_clientId.HasValue)
                {
                    Db.Execute(@"UPDATE Client SET fio=@fio, phone=@phone, address=@address, email=@email WHERE client_id=@id",
                        new SqlParameter("@fio", _txtFio.Text.Trim()),
                        new SqlParameter("@phone", _txtPhone.Text.Trim()),
                        new SqlParameter("@address", (object)_txtAddress.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@email", (object)_txtEmail.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@id", _clientId.Value));
                }
                else
                {
                    Db.Execute(@"INSERT INTO Client (client_id, fio, phone, address, email) VALUES (@id, @fio, @phone, @address, @email)",
                        new SqlParameter("@id", Convert.ToInt32(_lblIdValue.Text)),
                        new SqlParameter("@fio", _txtFio.Text.Trim()),
                        new SqlParameter("@phone", _txtPhone.Text.Trim()),
                        new SqlParameter("@address", (object)_txtAddress.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@email", (object)_txtEmail.Text.Trim() ?? DBNull.Value));
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить клиента.\n" + ex.Message);
            }
        }
    }
}
