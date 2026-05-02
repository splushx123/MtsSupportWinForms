using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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

            try
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "CSV files|*.csv|Excel files|*.xls|Text files|*.txt|JSON files|*.json";
                    dialog.FileName = "report.csv";
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    if (string.IsNullOrWhiteSpace(dialog.FileName))
                    {
                        MessageBox.Show("Укажите имя файла для экспорта.", "Отчет", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();
                    if (extension == ".csv" || extension == ".txt")
                    {
                        var separator = extension == ".txt" ? "\t" : ";";
                        var lines = BuildDelimitedLines(separator);
                        File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(true));
                    }
                    else if (extension == ".xls")
                    {
                        File.WriteAllText(dialog.FileName, BuildHtmlTable(), new UTF8Encoding(true));
                    }
                    else if (extension == ".json")
                    {
                        File.WriteAllText(dialog.FileName, BuildJson(), new UTF8Encoding(true));
                    }
                    else
                    {
                        MessageBox.Show("Неподдерживаемый формат экспорта.", "Отчет", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    LogService.Log("Экспорт отчета", dialog.FileName);
                    MessageBox.Show("Экспорт завершен.", "Отчет", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выполнить экспорт.\n" + ex.Message, "Отчет", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> BuildDelimitedLines(string separator)
        {
            var lines = new List<string>();
            var headers = _grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText);
            lines.Add(string.Join(separator, headers));
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                var cells = row.Cells.Cast<DataGridViewCell>().Select(c =>
                {
                    var text = (c.Value ?? string.Empty).ToString() ?? string.Empty;
                    text = text.Replace("\r", " ").Replace("\n", " ");
                    if (separator == ";") text = text.Replace(";", ",");
                    return text;
                });
                lines.Add(string.Join(separator, cells));
            }
            return lines;
        }

        private string BuildHtmlTable()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset=\"utf-8\"></head><body><table border=\"1\">");
            sb.AppendLine("<tr>");
            foreach (DataGridViewColumn column in _grid.Columns)
            {
                sb.Append("<th>").Append(System.Security.SecurityElement.Escape(column.HeaderText)).AppendLine("</th>");
            }
            sb.AppendLine("</tr>");
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                sb.AppendLine("<tr>");
                foreach (DataGridViewCell cell in row.Cells)
                {
                    var text = (cell.Value ?? string.Empty).ToString() ?? string.Empty;
                    sb.Append("<td>").Append(System.Security.SecurityElement.Escape(text)).AppendLine("</td>");
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table></body></html>");
            return sb.ToString();
        }

        private string BuildJson()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            var rows = _grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToList();
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                sb.Append("  {");
                for (int j = 0; j < _grid.Columns.Count; j++)
                {
                    var column = _grid.Columns[j];
                    var cell = row.Cells[j];
                    var value = (cell.Value ?? string.Empty).ToString() ?? string.Empty;
                    value = value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ");
                    sb.Append("\"").Append(column.HeaderText.Replace("\"", "\\\"")).Append("\":\"").Append(value).Append("\"");
                    if (j < _grid.Columns.Count - 1) sb.Append(",");
                }
                sb.Append("}");
                if (i < rows.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
