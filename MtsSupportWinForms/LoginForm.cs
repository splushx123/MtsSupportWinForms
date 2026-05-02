using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class LoginForm : Form
    {
        private readonly TextBox _txtEmail = Theme.CreateTextBox(290);
        private readonly CheckBox _chkRemember = new CheckBox();
        private readonly Label _lblError = new Label();
        private readonly string _rememberFile = Path.Combine(Application.StartupPath, "last_email.txt");

        public UserAccount CurrentUser { get; private set; }

        public LoginForm()
        {
            Theme.StyleForm(this);
            Text = "Авторизация по e-mail";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(920, 520);
            _txtEmail.MaxLength = 120;

            var left = new Panel { Dock = DockStyle.Left, Width = 360, BackColor = Theme.Sidebar, Padding = new Padding(34) };
            var appName = new Label
            {
                Text = "MTS Support\nWindows Forms",
                Dock = DockStyle.Top,
                Height = 110,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White
            };
            // На левой панели формы авторизации оставляем только название приложения.
            left.Controls.Clear();
            left.Controls.Add(appName);

            var rightWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(42) };
            var card = Theme.CreateCard(28);
            card.Dock = DockStyle.Fill;

            var title = new Label
            {
                Text = "Вход в систему",
                Dock = DockStyle.Top,
                Height = 42,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Theme.Text
            };
            var subtitle = new Label
            {
                Text = "Для входа укажите e-mail пользователя. Роль определяется автоматически.",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Theme.Muted
            };

            var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, Height = 210, Padding = new Padding(0, 12, 0, 0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "E-mail", AutoSize = true, Padding = new Padding(0, 10, 0, 0) }, 0, 0);
            layout.Controls.Add(_txtEmail, 1, 0);

            _chkRemember.Text = "Запомнить e-mail на этом компьютере";
            _chkRemember.AutoSize = true;
            _chkRemember.ForeColor = Theme.Text;
            layout.Controls.Add(_chkRemember, 1, 1);

            var linkCheck = new LinkLabel { Text = "Проверить подключение к SQL Server", AutoSize = true, LinkColor = Theme.Primary, ActiveLinkColor = Theme.PrimaryDark };
            linkCheck.Click += delegate { TestConnection(); };
            layout.Controls.Add(linkCheck, 1, 2);

            var hint = new Label
            {
                Text = "Пример e-mail адресов для входа задается в App.config: admin, operator (оператор 1 линии) и engineer (оператор 2 линии).",
                AutoSize = true,
                ForeColor = Theme.Muted,
                MaximumSize = new Size(360, 0)
            };
            layout.Controls.Add(hint, 1, 3);

            _lblError.AutoSize = true;
            _lblError.ForeColor = Color.Firebrick;
            layout.Controls.Add(_lblError, 1, 4);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 58, FlowDirection = FlowDirection.RightToLeft };
            var btnLogin = Theme.CreatePrimaryButton("Войти", 130);
            var btnClose = Theme.CreateSecondaryButton("Закрыть", 130);
            btnLogin.Click += delegate { DoLogin(); };
            btnClose.Click += delegate { Close(); };
            buttons.Controls.Add(btnLogin);
            buttons.Controls.Add(btnClose);

            card.Controls.Add(buttons);
            card.Controls.Add(layout);
            card.Controls.Add(subtitle);
            card.Controls.Add(title);
            rightWrap.Controls.Add(card);

            Controls.Add(rightWrap);
            Controls.Add(left);

            if (File.Exists(_rememberFile))
            {
                _txtEmail.Text = File.ReadAllText(_rememberFile).Trim();
                _chkRemember.Checked = _txtEmail.Text.Length > 0;
            }
            else
            {
                _txtEmail.Text = "admin@mts-support.local";
            }

            AcceptButton = btnLogin;
        }

        private void TestConnection()
        {
            string error;
            if (Db.CanConnect(out error))
            {
                MessageBox.Show("Подключение к SQL Server выполнено успешно.", "Проверка подключения", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Не удалось подключиться к базе данных.\n\n" + error, "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DoLogin()
        {
            _lblError.Text = string.Empty;
            var email = (_txtEmail.Text ?? string.Empty).Trim();
            if (email.Length == 0)
            {
                _lblError.Text = "Введите e-mail пользователя.";
                return;
            }
            if (!ValidationUtils.IsValidEmail(email))
            {
                _lblError.Text = "Введите корректный e-mail адрес.";
                return;
            }

            CurrentUser = AuthService.Authenticate(email);
            if (CurrentUser == null)
            {
                _lblError.Text = "Указанный e-mail не найден в списке учетных записей приложения.";
                return;
            }

            string error;
            if (!Db.CanConnect(out error))
            {
                MessageBox.Show("Не удалось подключиться к SQL Server. Проверь строку подключения в App.config.\n\n" + error,
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_chkRemember.Checked)
            {
                File.WriteAllText(_rememberFile, _txtEmail.Text.Trim());
            }
            else if (File.Exists(_rememberFile))
            {
                File.Delete(_rememberFile);
            }

            LogService.Log("Авторизация", CurrentUser.Email + " | " + CurrentUser.FullName);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
