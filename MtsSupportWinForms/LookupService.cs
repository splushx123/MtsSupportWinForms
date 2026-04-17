using System.Data;

namespace MtsSupportWinForms
{
    public static class LookupService
    {
        public static DataTable Clients()
        {
            return Db.Query("SELECT client_id, fio FROM Client ORDER BY fio");
        }

        public static DataTable Employees()
        {
            return Db.Query("SELECT employee_id, fio FROM Employee ORDER BY fio");
        }

        public static DataTable Positions()
        {
            return Db.Query("SELECT position_id, title_position FROM Position ORDER BY title_position");
        }

        public static DataTable Models()
        {
            return Db.Query("SELECT model_id, title_model FROM Model ORDER BY title_model");
        }

        public static DataTable Statuses()
        {
            return Db.Query("SELECT status_id, title_status FROM Status ORDER BY status_id");
        }
    }
}
