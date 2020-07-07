using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace RMS
{
    public partial class EnvironmentForm : Form
    {

        private long parentId;

        public EnvironmentForm()
        {
            InitializeComponent();
        }

        public long ParentId { get => parentId; set => parentId = value; }

        private void EnvironmentForm_Load(object sender, EventArgs e)
        {


            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            this.label4.Text = this.parentId.ToString();
            this.textBox3.Text = this.environmentPath(this.parentId, ref connection);

            connection.Close();
            

        }

        private string environmentPath(long rootId, ref SQLiteConnection connection)
        {

            if (rootId == -1)
            {
                return "";
            }
            else
            {
                var command = new SQLiteCommand($"SELECT * FROM Environment WHERE STATUS = 1 AND ID = {rootId}", connection);

                SQLiteDataReader reader = command.ExecuteReader();

                reader.Read();

                System.Int32 environmentId = -1;

                MessageBox.Show(reader[2].ToString());

                var data = reader[2];

                if (!reader.IsDBNull(2))
                
                {
                    environmentId = reader.GetInt32(2);
                }

                string name = reader.GetString(3);

                reader.Close();
                

                return this.environmentPath(environmentId, ref connection) + @" \ " + name;


            }

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }
    }
}
