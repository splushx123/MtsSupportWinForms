using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class ClientEditForm : Form
    {
        private readonly int? _clientId;
        private readonly UserRole _role;
        private readonly bool _readOnlyView;

        private readonly TextBox _txtFio = Theme.CreateTextBox(360);
        private readonly TextBox _txtPhone = Theme.CreateTextBox(360);
        private readonly TextBox _txtAddress = Theme.CreateTextBox(360);
        private readonly TextBox _txtEmail = Theme.CreateTextBox(360);

        public ClientEditForm(int? clientId, UserRole role, bool readOnlyView = false)
        {
            _clientId = clientId;
            _role = role;
            _readOnlyView = readOnlyView;
            Theme.StyleForm(this);
            Text = clientId.HasValue ? "Карточка клиента" : "Новый клиент";
            Width = 660;
            Height = 360;
            StartPosition = FormStartPosition.CenterParent;
            _txtFio.MaxLength = 120;
            _txtPhone.MaxLength = 20;
            _txtAddress.MaxLength = 250;
            _txtEmail.MaxLength = 120;

            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var card = Theme.CreateCard();
            card.Dock = DockStyle.Top;
            card.Height = 210;

            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            form.Controls.Add(new Label { Text = "ФИО", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 0);
            form.Controls.Add(_txtFio, 1, 0);
            form.Controls.Add(new Label { Text = "Телефон", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 1);
            form.Controls.Add(_txtPhone, 1, 1);
            form.Controls.Add(new Label { Text = "Адрес", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 2);
            form.Controls.Add(_txtAddress, 1, 2);
            form.Controls.Add(new Label { Text = "Почта", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 3);
            form.Controls.Add(_txtEmail, 1, 3);
            card.Controls.Add(form);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, FlowDirection = FlowDirection.RightToLeft };
            var btnSave = Theme.CreatePrimaryButton("Сохранить", 130);
            var btnClose = Theme.CreateSecondaryButton("Закрыть", 130);
            btnSave.Click += delegate { Save(); };
            btnClose.Click += delegate { Close(); };
            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnClose);

            root.Controls.Add(card);
            root.Controls.Add(buttons);
            Controls.Add(root);
            Load += delegate { LoadData(); };

            if (_readOnlyView)
            {
                _txtFio.ReadOnly = true;
                _txtPhone.ReadOnly = true;
                _txtAddress.ReadOnly = true;
                _txtEmail.ReadOnly = true;
                btnSave.Visible = false;
            }
            else if (_role == UserRole.SpecialistLine2)
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
                    _txtFio.Text = row["fio"].ToString();
                    _txtPhone.Text = row["phone"].ToString();
                    _txtAddress.Text = row["address"].ToString();
                    _txtEmail.Text = row["email"].ToString();
                }
            }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_txtFio.Text) || string.IsNullOrWhiteSpace(_txtPhone.Text))
            {
                MessageBox.Show("Заполните обязательные поля: ФИО и телефон.");
                return;
            }
            if (!ValidationUtils.IsValidPersonName(_txtFio.Text))
            {
                MessageBox.Show("ФИО должно содержать только буквы, пробелы или дефис.");
                return;
            }
            if (!ValidationUtils.IsValidPhone(_txtPhone.Text))
            {
                MessageBox.Show("Телефон должен содержать только цифры и символы + ( ) -.");
                return;
            }
            if (!ValidationUtils.IsValidPhonePlus7(_txtPhone.Text))
            {
                MessageBox.Show("Телефон клиента должен быть в формате +7XXXXXXXXXX.");
                return;
            }
            if (!string.IsNullOrWhiteSpace(_txtEmail.Text) && !ValidationUtils.IsValidEmail(_txtEmail.Text))
            {
                MessageBox.Show("Укажите корректный e-mail клиента.");
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
                    var nextId = Db.NextId("Client", "client_id");
                    Db.Execute(@"INSERT INTO Client (client_id, fio, phone, address, email) VALUES (@id, @fio, @phone, @address, @email)",
                        new SqlParameter("@id", nextId),
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
