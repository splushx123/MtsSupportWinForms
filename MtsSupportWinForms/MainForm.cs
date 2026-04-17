using System;
using System.Drawing;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class MainForm : Form
    {
        private readonly UserAccount _user;
        private readonly FlowLayoutPanel _statsPanel = new FlowLayoutPanel();
        private readonly FlowLayoutPanel _tilesPanel = new FlowLayoutPanel();

        public MainForm(UserAccount user)
        {
            _user = user;
            Theme.StyleForm(this);
            Text = "Главное окно";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;

            var sidebar = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Theme.Sidebar, Padding = new Padding(18) };
            var brand = new Label
            {
                Text = "MTS Support\nDesktop",
                Dock = DockStyle.Top,
                Height = 86,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White
            };
            var userInfo = new Label
            {
                Text = _user.FullName + "\n" + _user.Email,
                Dock = DockStyle.Top,
                Height = 60,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(223, 227, 235)
            };
            var nav = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(0, 20, 0, 0) };

            AddNavButton(nav, "Клиенты", delegate { OpenModule("Клиенты", new ClientsForm(_user)); }, true);
            AddNavButton(nav, "Обращения", delegate { OpenModule("Обращения", new RequestsForm(_user)); }, true);
            AddNavButton(nav, "Оборудование", delegate { OpenModule("Оборудование", new EquipmentForm(_user)); }, _user.Role != UserRole.OperatorLine1);
            AddNavButton(nav, "Сотрудники", delegate { OpenModule("Сотрудники", new EmployeesForm(_user)); }, _user.Role == UserRole.Administrator);
            AddNavButton(nav, "Решения", delegate { OpenModule("Решения", new SolutionsForm(_user)); }, _user.Role != UserRole.OperatorLine1);
            AddNavButton(nav, "Отчеты", delegate { OpenModule("Отчеты", new ReportsForm()); }, _user.Role == UserRole.Administrator);
            AddNavButton(nav, "Журнал", delegate { OpenModule("Журнал", new ActivityLogForm()); }, _user.Role == UserRole.Administrator);
            AddNavButton(nav, "Выход", delegate { Close(); }, true);

            sidebar.Controls.Add(nav);
            sidebar.Controls.Add(userInfo);
            sidebar.Controls.Add(brand);

            var top = new Panel { Dock = DockStyle.Top, Height = 110, Padding = new Padding(24, 16, 24, 14), BackColor = Theme.Surface };
            var title = new Label
            {
                Text = "Главный модуль администратора",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Theme.Text
            };
            var subtitle = new Label
            {
                Text = "Централизованный доступ к клиентам, обращениям, оборудованию, сотрудникам, решениям, отчетам и журналированию.",
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Theme.Muted
            };
            top.Controls.Add(subtitle);
            top.Controls.Add(title);

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22) };
            _statsPanel.Dock = DockStyle.Top;
            _statsPanel.Height = 138;
            _statsPanel.WrapContents = true;
            _tilesPanel.Dock = DockStyle.Fill;
            _tilesPanel.WrapContents = true;
            _tilesPanel.AutoScroll = true;
            _tilesPanel.Padding = new Padding(0, 14, 0, 0);

            body.Controls.Add(_tilesPanel);
            body.Controls.Add(_statsPanel);

            Controls.Add(body);
            Controls.Add(top);
            Controls.Add(sidebar);
            Load += delegate { FillStats(); FillTiles(); };
        }

        private void AddNavButton(FlowLayoutPanel panel, string text, Action action, bool visible)
        {
            if (!visible) return;
            var btn = Theme.CreateNavButton(text);
            btn.Click += delegate { action(); };
            panel.Controls.Add(btn);
        }

        private void FillStats()
        {
            _statsPanel.Controls.Clear();
            _statsPanel.Controls.Add(Theme.CreateStatCard("Клиенты", SafeCount("SELECT COUNT(*) FROM Client").ToString(), Theme.Primary));
            _statsPanel.Controls.Add(Theme.CreateStatCard("Обращения", SafeCount("SELECT COUNT(*) FROM Request").ToString(), Theme.Success));
            _statsPanel.Controls.Add(Theme.CreateStatCard("Открытые заявки", SafeCount("SELECT COUNT(*) FROM Request r INNER JOIN Status s ON s.status_id=r.status_id WHERE s.title_status <> N'Закрыто'").ToString(), Theme.Warning));
            _statsPanel.Controls.Add(Theme.CreateStatCard("Решения", SafeCount("SELECT COUNT(*) FROM Solution").ToString(), Color.FromArgb(56, 96, 178)));
        }

        private void FillTiles()
        {
            _tilesPanel.Controls.Clear();
            AddTile("Клиенты", "Поиск, карточка клиента, изменение и удаление записей.", delegate { OpenModule("Клиенты", new ClientsForm(_user)); }, true);
            AddTile("Обращения", "Регистрация заявок, назначение сотрудника, изменение статуса и контроль сроков.", delegate { OpenModule("Обращения", new RequestsForm(_user)); }, true);
            AddTile("Оборудование", "Справочник устройств клиентов и связь с моделями оборудования.", delegate { OpenModule("Оборудование", new EquipmentForm(_user)); }, _user.Role != UserRole.OperatorLine1);
            AddTile("Сотрудники", "Администрирование сотрудников и должностей.", delegate { OpenModule("Сотрудники", new EmployeesForm(_user)); }, _user.Role == UserRole.Administrator);
            AddTile("Решения", "База знаний по устранению типовых инцидентов.", delegate { OpenModule("Решения", new SolutionsForm(_user)); }, _user.Role != UserRole.OperatorLine1);
            AddTile("Отчеты", "Аналитика по статусам, нагрузке, срокам и решениям.", delegate { OpenModule("Отчеты", new ReportsForm()); }, _user.Role == UserRole.Administrator);
            AddTile("Журналирование", "Просмотр действий пользователей и операций изменения данных.", delegate { OpenModule("Журнал", new ActivityLogForm()); }, _user.Role == UserRole.Administrator);
        }

        private void AddTile(string title, string description, Action action, bool visible)
        {
            if (!visible) return;
            var card = Theme.CreateCard();
            card.Width = 310;
            card.Height = 172;
            card.Cursor = Cursors.Hand;

            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Theme.Text
            };
            var descLabel = new Label
            {
                Text = description,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Theme.Muted
            };
            var openButton = Theme.CreatePrimaryButton("Открыть раздел", 140);
            openButton.Dock = DockStyle.Bottom;
            openButton.Click += delegate { action(); };

            card.Click += delegate { action(); };
            titleLabel.Click += delegate { action(); };
            descLabel.Click += delegate { action(); };

            card.Controls.Add(openButton);
            card.Controls.Add(descLabel);
            card.Controls.Add(titleLabel);
            _tilesPanel.Controls.Add(card);
        }

        private void OpenModule(string moduleName, Form form)
        {
            LogService.Log("Открытие модуля", moduleName + " | " + _user.Email);
            using (form)
            {
                form.ShowDialog(this);
            }
            FillStats();
        }

        private int SafeCount(string sql)
        {
            try
            {
                return Db.Count(sql);
            }
            catch
            {
                return 0;
            }
        }
    }
}
