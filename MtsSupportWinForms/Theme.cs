using System.Drawing;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public static class Theme
    {
        public static readonly Color AppBack = Color.FromArgb(243, 246, 251);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceAlt = Color.FromArgb(248, 250, 253);
        public static readonly Color Primary = Color.FromArgb(226, 0, 70);
        public static readonly Color PrimaryDark = Color.FromArgb(168, 0, 52);
        public static readonly Color Sidebar = Color.FromArgb(25, 33, 52);
        public static readonly Color Success = Color.FromArgb(0, 153, 102);
        public static readonly Color Warning = Color.FromArgb(242, 153, 74);
        public static readonly Color Text = Color.FromArgb(31, 41, 55);
        public static readonly Color Muted = Color.FromArgb(107, 114, 128);
        public static readonly Color Border = Color.FromArgb(226, 232, 240);

        public static void StyleForm(Form form)
        {
            form.BackColor = AppBack;
            form.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            form.ForeColor = Text;
        }

        public static Panel CreateCard(int padding = 18)
        {
            var panel = new Panel();
            panel.BackColor = Surface;
            panel.Padding = new Padding(padding);
            panel.Margin = new Padding(12);
            panel.BorderStyle = BorderStyle.FixedSingle;
            return panel;
        }

        public static Label CreateSectionTitle(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Text
            };
        }

        public static Label CreateMutedLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9F)
            };
        }

        public static Button CreatePrimaryButton(string text, int width)
        {
            var button = new Button();
            button.Text = text;
            button.Width = width;
            button.Height = 40;
            button.BackColor = Primary;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;

            if (string.Equals(text, "Добавить", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "Создать", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "Обновить", System.StringComparison.OrdinalIgnoreCase))
            {
                button.BackColor = Surface;
                button.ForeColor = Text;
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Border;
                button.MouseEnter += delegate
                {
                    button.BackColor = Primary;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderSize = 0;
                };
                button.MouseLeave += delegate
                {
                    button.BackColor = Surface;
                    button.ForeColor = Text;
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.BorderColor = Border;
                };
            }

            return button;
        }

        public static Button CreateSecondaryButton(string text, int width)
        {
            var button = new Button();
            button.Text = text;
            button.Width = width;
            button.Height = 40;
            button.BackColor = Surface;
            button.ForeColor = Text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Border;
            button.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.MouseEnter += delegate
            {
                button.BackColor = Primary;
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderSize = 0;
            };
            button.MouseLeave += delegate
            {
                button.BackColor = Surface;
                button.ForeColor = Text;
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Border;
            };
            return button;
        }

        public static Button CreateNavButton(string text, int width = 210)
        {
            var button = new Button();
            button.Text = text;
            button.Width = width;
            button.Height = 46;
            button.BackColor = Sidebar;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(14, 0, 0, 0);
            button.Cursor = Cursors.Hand;
            return button;
        }

        public static TextBox CreateTextBox(int width)
        {
            var textBox = new TextBox();
            textBox.Width = width;
            textBox.Font = new Font("Segoe UI", 10.5F);
            return textBox;
        }

        public static ComboBox CreateComboBox(int width)
        {
            var comboBox = new ComboBox();
            comboBox.Width = width;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Font = new Font("Segoe UI", 10F);
            return comboBox;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Border;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Sidebar;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 42;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 236, 242);
            grid.DefaultCellStyle.SelectionForeColor = Text;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceAlt;
        }

        public static Panel CreateStatCard(string title, string value, Color accent)
        {
            var panel = CreateCard(16);
            panel.Width = 230;
            panel.Height = 110;

            var topBar = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = accent };
            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Muted,
                Padding = new Padding(0, 8, 0, 0)
            };
            var valueLabel = new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Text
            };

            panel.Controls.Add(valueLabel);
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(topBar);
            return panel;
        }
    }
}
