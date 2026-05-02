using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class SolutionEditForm : Form
    {
        private readonly int? _solutionId;
        private readonly UserRole _role;
        private readonly bool _readOnlyView;
        private readonly ComboBox _cbEmployee = Theme.CreateComboBox(320);
        private readonly TextBox _txtTitle = Theme.CreateTextBox(320);
        private readonly TextBox _txtSteps = Theme.CreateTextBox(320);

        public SolutionEditForm(int? solutionId, UserRole role, bool readOnlyView = false)
        {
            _solutionId = solutionId;
            _role = role;
            _readOnlyView = readOnlyView;
            Theme.StyleForm(this);
            Text = solutionId.HasValue ? "Карточка решения" : "Новое решение";
            Width = 650;
            Height = 400;
            StartPosition = FormStartPosition.CenterParent;
            _txtSteps.Multiline = true;
            _txtTitle.MaxLength = 200;
            _txtSteps.MaxLength = 4000;
            _txtSteps.Height = 150;

            var card = Theme.CreateCard();
            card.Dock = DockStyle.Fill;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "Заголовок", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 0);
            layout.Controls.Add(_txtTitle, 1, 0);
            layout.Controls.Add(new Label { Text = "Сотрудник", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 1);
            layout.Controls.Add(_cbEmployee, 1, 1);
            layout.Controls.Add(new Label { Text = "Шаги решения", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 2);
            layout.Controls.Add(_txtSteps, 1, 2);
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
            UiHelpers.BindLookup(_cbEmployee, LookupService.Employees(), "employee_id", "fio", false);
        }

        private void LoadData()
        {
            if (_solutionId.HasValue)
            {
                var table = Db.Query("SELECT solution_id, employee_id, title, steps FROM Solution WHERE solution_id = @id", new SqlParameter("@id", _solutionId.Value));
                if (table.Rows.Count == 1)
                {
                    var row = table.Rows[0];
                    _txtTitle.Text = row["title"].ToString();
                    _txtSteps.Text = row["steps"].ToString();
                    _cbEmployee.SelectedValue = Convert.ToInt32(row["employee_id"]);
                }
            }
        }

        private void ApplyRole(Button btnSave)
        {
            if (_role == UserRole.OperatorLine1)
            {
                btnSave.Enabled = false;
                _txtTitle.ReadOnly = true;
                _txtSteps.ReadOnly = true;
                _cbEmployee.Enabled = false;
            }
        }

        private void ApplyReadOnly(Button btnSave)
        {
            if (!_readOnlyView) return;
            btnSave.Visible = false;
            _txtTitle.ReadOnly = true;
            _txtSteps.ReadOnly = true;
            _cbEmployee.Enabled = false;
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_txtTitle.Text) || string.IsNullOrWhiteSpace(_txtSteps.Text) || _cbEmployee.SelectedValue == null)
            {
                MessageBox.Show("Заполните все поля решения.");
                return;
            }
            if (_txtTitle.Text.Trim().Length < 5)
            {
                MessageBox.Show("Заголовок решения должен содержать не менее 5 символов.");
                return;
            }
            if (_txtSteps.Text.Trim().Length < 15)
            {
                MessageBox.Show("Шаги решения должны содержать не менее 15 символов.");
                return;
            }

            try
            {
                if (_solutionId.HasValue)
                {
                    Db.Execute(@"UPDATE Solution SET employee_id=@employee_id, title=@title, steps=@steps WHERE solution_id=@id",
                        new SqlParameter("@employee_id", Convert.ToInt32(_cbEmployee.SelectedValue)),
                        new SqlParameter("@title", _txtTitle.Text.Trim()),
                        new SqlParameter("@steps", _txtSteps.Text.Trim()),
                        new SqlParameter("@id", _solutionId.Value));
                }
                else
                {
                    var nextId = Db.NextId("Solution", "solution_id");
                    Db.Execute(@"INSERT INTO Solution (solution_id, employee_id, title, steps) VALUES (@id, @employee_id, @title, @steps)",
                        new SqlParameter("@id", nextId),
                        new SqlParameter("@employee_id", Convert.ToInt32(_cbEmployee.SelectedValue)),
                        new SqlParameter("@title", _txtTitle.Text.Trim()),
                        new SqlParameter("@steps", _txtSteps.Text.Trim()));
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить решение.\n" + ex.Message);
            }
        }
    }
}
