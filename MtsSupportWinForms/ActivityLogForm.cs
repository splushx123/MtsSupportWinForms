using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class ActivityLogForm : Form
    {
        private readonly ListBox _list = new ListBox();

        public ActivityLogForm()
        {
            Theme.StyleForm(this);
            Text = "Журналирование";
            Width = 980;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;

            _list.Dock = DockStyle.Fill;
            _list.Font = new Font("Consolas", 10F);
            _list.HorizontalScrollbar = true;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(10), BackColor = Theme.Surface };
            var btnRefresh = Theme.CreatePrimaryButton("Обновить", 120);
            var btnClear = Theme.CreateSecondaryButton("Очистить журнал", 150);
            var btnClose = Theme.CreateSecondaryButton("Закрыть", 120);
            btnRefresh.Click += delegate { LoadLog(); };
            btnClear.Click += delegate {
                if (MessageBox.Show("Очистить журнал действий?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.IO.File.WriteAllText(System.IO.Path.Combine(Application.StartupPath, "logs", "activity.log"), string.Empty);
                    LoadLog();
                }
            };
            btnClose.Click += delegate { Close(); };
            top.Controls.AddRange(new Control[] { btnRefresh, btnClear, btnClose });

            Controls.Add(_list);
            Controls.Add(top);
            Load += delegate { LoadLog(); };
        }

        private void LoadLog()
        {
            _list.Items.Clear();
            foreach (var line in LogService.ReadAll().Reverse())
            {
                _list.Items.Add(line);
            }
        }
    }
}
