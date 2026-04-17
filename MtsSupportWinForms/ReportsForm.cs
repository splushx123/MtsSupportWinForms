using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public class ReportsForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly ComboBox _cbReport = Theme.CreateComboBox(260);
        private readonly CheckBox _chkOnlyActive = new CheckBox();
        private readonly Label _lblSummary = new Label();

        public ReportsForm()
        {
            Theme.StyleForm(this);
            Text = "Отчеты и результаты автоматизации";
            Width = 1220;
            Height = 720;
            StartPosition = FormStartPosition.CenterParent;
            Theme.StyleGrid(_grid);

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 96, Padding = new Padding(12), BackColor = Theme.Surface, WrapContents = true };
            _cbReport.Items.AddRange(new object[]
            {
                "Отчет по статусу обращений",
                "Отчет по нагрузке на специалистов",
                "Отчет по времени обработки обращений",
                "Отчет по используемым решениям"
            });
            _cbReport.SelectedIndex = 0;

            _chkOnlyActive.Text = "Только активные обращения";
            _chkOnlyActive.AutoSize = true;
            _chkOnlyActive.Padding = new Padding(0, 10, 0, 0);

            var btnExport = Theme.CreateSecondaryButton("Экспорт", 130);
            var btnClose = Theme.CreateSecondaryButton("Закрыть", 120);
            btnExport.Click += delegate { ExportReport(); };
            btnClose.Click += delegate { Close(); };
            _cbReport.SelectedIndexChanged += delegate { BuildReport(); };
            _chkOnlyActive.CheckedChanged += delegate { BuildReport(); };

            _lblSummary.AutoSize = true;
            _lblSummary.Padding = new Padding(0, 10, 0, 0);
            _lblSummary.ForeColor = Theme.Muted;

            top.Controls.Add(new Label { Text = "Тип отчета:", AutoSize = true, Padding = new Padding(0, 10, 0, 0) });
            top.Controls.Add(_cbReport);
            top.Controls.Add(_chkOnlyActive);
            top.Controls.Add(btnExport);
            top.Controls.Add(btnClose);
            top.Controls.Add(_lblSummary);

            Controls.Add(_grid);
            Controls.Add(top);
            Load += delegate { BuildReport(); };
        }

        private void BuildReport()
        {
            if (_cbReport.SelectedIndex == 0) ShowStatusReport();
            else if (_cbReport.SelectedIndex == 1) ShowEmployeeLoad();
            else if (_cbReport.SelectedIndex == 2) ShowTimeReport();
            else ShowSolutionReport();
        }

        private void ShowStatusReport()
        {
            var whereActive = _chkOnlyActive.Checked ? "WHERE s.title_status <> N'Закрыто'" : string.Empty;
            _grid.DataSource = Db.Query(@"
SELECT s.title_status AS [Статус], COUNT(r.request_id) AS [Количество обращений]
FROM Status s
LEFT JOIN Request r ON r.status_id = s.status_id
" + whereActive + @"
GROUP BY s.title_status
ORDER BY [Количество обращений] DESC");
            UpdateSummary("Показано распределение обращений по статусам.");
        }

        private void ShowEmployeeLoad()
        {
            var whereActive = _chkOnlyActive.Checked ? "WHERE s.title_status <> N'Закрыто'" : string.Empty;
            _grid.DataSource = Db.Query(@"
SELECT e.fio AS [Сотрудник], p.title_position AS [Должность], COUNT(r.request_id) AS [Количество обращений]
FROM Employee e
LEFT JOIN Position p ON p.position_id = e.position_id
LEFT JOIN Request r ON r.employee_id = e.employee_id
LEFT JOIN Status s ON s.status_id = r.status_id
" + whereActive + @"
GROUP BY e.fio, p.title_position
ORDER BY [Количество обращений] DESC, e.fio");
            UpdateSummary("Показана нагрузка по сотрудникам. Отчет включает расчет количества заявок.");
        }

        private void ShowTimeReport()
        {
            var onlyActive = _chkOnlyActive.Checked ? "WHERE s.title_status <> N'Закрыто'" : string.Empty;
            _grid.DataSource = Db.Query(@"
SELECT c.fio AS [Клиент], s.title_status AS [Статус], r.date_request AS [Дата обращения],
       DATEDIFF(DAY, r.date_request, GETDATE()) AS [Дней с момента создания]
FROM Request r
INNER JOIN Client c ON c.client_id = r.client_id
INNER JOIN Status s ON s.status_id = r.status_id
" + onlyActive + @"
ORDER BY r.date_request DESC");
            UpdateSummary("Показан срок существования каждой заявки в днях с момента регистрации.");
        }

        private void ShowSolutionReport()
        {
            _grid.DataSource = Db.Query(@"
SELECT s.title AS [Заголовок], e.fio AS [Сотрудник]
FROM Solution s
LEFT JOIN Employee e ON e.employee_id = s.employee_id
ORDER BY s.solution_id DESC");
            UpdateSummary("Показан перечень решений и сотрудников, которые их оформили.");
        }

        private void UpdateSummary(string text)
        {
            _lblSummary.Text = text + " Строк в отчете: " + _grid.Rows.Count;
        }

        private void ExportReport()
        {
            if (_grid.Columns.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.", "Отчет", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files|*.csv|Text files|*.txt";
                dialog.FileName = "report.csv";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                var lines = new List<string>();
                var headers = _grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText);
                var separator = Path.GetExtension(dialog.FileName).Equals(".txt", StringComparison.OrdinalIgnoreCase) ? "\t" : ";";
                lines.Add(string.Join(separator, headers));
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (row.IsNewRow) continue;
                    var cells = row.Cells.Cast<DataGridViewCell>().Select(c => ((c.Value ?? string.Empty).ToString() ?? string.Empty).Replace(";", ","));
                    lines.Add(string.Join(separator, cells));
                }
                File.WriteAllLines(dialog.FileName, lines);
                LogService.Log("Экспорт отчета", dialog.FileName);
                MessageBox.Show("Экспорт завершен.", "Отчет", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
