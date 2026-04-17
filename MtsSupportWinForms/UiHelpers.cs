using System.Data;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    public static class UiHelpers
    {
        public static void BindLookup(ComboBox comboBox, DataTable data, string valueMember, string displayMember, bool includeEmpty)
        {
            var bindTable = data.Copy();
            if (includeEmpty)
            {
                var row = bindTable.NewRow();
                row[valueMember] = 0;
                row[displayMember] = "Не выбрано";
                bindTable.Rows.InsertAt(row, 0);
            }
            comboBox.DataSource = bindTable;
            comboBox.ValueMember = valueMember;
            comboBox.DisplayMember = displayMember;
        }

        public static int? ComboValue(ComboBox comboBox)
        {
            if (comboBox.SelectedValue == null) return null;
            int value;
            if (int.TryParse(comboBox.SelectedValue.ToString(), out value))
            {
                return value == 0 ? (int?)null : value;
            }
            return null;
        }
    }
}
