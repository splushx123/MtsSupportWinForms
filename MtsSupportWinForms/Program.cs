using System;
using System.Windows.Forms;

namespace MtsSupportWinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
            {
                LogService.Log("Ошибка интерфейса", e.Exception.Message);
                MessageBox.Show("Во время работы приложения возникла ошибка. Подробности записаны в журнал.\n\n" + e.Exception.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            while (true)
            {
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK || login.CurrentUser == null)
                    {
                        break;
                    }

                    var mainForm = new MainForm(login.CurrentUser);
                    Application.Run(mainForm);
                    if (!mainForm.ReturnToLogin)
                    {
                        break;
                    }
                }
            }
        }
    }
}
