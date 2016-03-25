using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;
using MySql.Data.MySqlClient;
using Xceed.Wpf;

namespace ASR_03
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection conn = new MySqlConnection("server=192.162.89.38;port=6306;userid=admin;password=admin;database=analiticsb");

        public MainWindow()
        {
            InitializeComponent();
            UpdateASR();
        }

        private void UpdateASR()
        {
            DateTime dtFrom = (DateTime)dtPickerFrom.Value;
            DateTime dtTo = (DateTime)dtPickerTo.Value;

            string sql = "SELECT IFNULL(c.mediaGateLocation, 'Итого') AS 'Region', "
                + "COUNT(*) AS 'Total', "
                + "SUM(IF(c.result = 0, 1, 0)) AS 'Success', "
                + "SUM(IF(c.result = 0 AND c.CallCodeResult = 4, 1, 0)) AS 'Connect', "
                + "ROUND((SUM(IF(c.CallCodeResult = 4, 1, 0)) / COUNT(*) * 100), 2) AS 'ASR' "
                + "FROM cdr c INNER JOIN voip v ON c.voip_index = v.voip_index "
                + "WHERE timeStart BETWEEN STR_TO_DATE('"
                + dtFrom.ToString("yyyy-MM-dd  HH:mm:ss")
                + "', '%Y-%m-%d %H:%i:%s') AND STR_TO_DATE('"
                + dtTo.ToString("yyyy-MM-dd  HH:mm:ss")
                + "', '%Y-%m-%d %H:%i:%s') "
                + "GROUP BY c.mediaGateLocation WITH ROLLUP";

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adp.Fill(ds, "LoadDataBinding");
                dataGridASR.DataContext = ds;
            }
            catch (MySqlException ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
            finally
            {
                conn.Close();
                Mouse.OverrideCursor = null;
            }
        }

        private void dtPickerFrom_Initialized(object sender, EventArgs e)
        {
            DateTime dtFrom = DateTime.Today;
            dtPickerFrom.DefaultValue = dtFrom.AddDays(-1);
        }

        private void dtPickerTo_Initialized(object sender, EventArgs e)
        {
            DateTime dtTo = DateTime.Today;
            dtPickerTo.DefaultValue = dtTo.AddSeconds(-1);
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            UpdateASR();
        }
    }
}
