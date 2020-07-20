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
        private string action;

        public EnvironmentForm(string action)
        {
            InitializeComponent();

            this.action = action;
        }

        public long ParentId { get => parentId; set => parentId = value; }

        private void EnvironmentForm_Load(object sender, EventArgs e)
        {
            
            // establist connection with the database
            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            
            this.label4.Text = this.parentId.ToString();
            this.textBox3.Text = environmentPath(this.parentId, ref connection);

            

            if (this.action == "CREATE ENVIRONMENT")
            {
                // nothing is read only
                this.Text = "ACTION: CREATE ENVIRONMENT";
            }
            else if (this.action == "MODIFY ENVIRONMENT")
            {
                // nothing is readonly but fills in the name and description fields with their previous values
                var command = new SQLiteCommand($"SELECT * FROM Environment WHERE STATUS = 1 AND ID = {this.parentId}", connection);

                SQLiteDataReader reader = command.ExecuteReader();

                reader.Read();

                this.textBox1.Text = reader.GetString(3);
                this.textBox2.Text = reader.GetString(4);




                reader.Close();
            }
            else if (this.action == "DELETE ENVIRONMENT")
            {
                // if the user is deleting an environment, the name and description fields will be read only
                // ... but they still will have to enter the contextual information
                this.Text = "ACTION: DELETE ENVIRONMENT";
                this.textBox1.ReadOnly = true;
                this.textBox2.ReadOnly = true;

                var command = new SQLiteCommand($"SELECT * FROM Environment WHERE STATUS = 1 AND ID = {this.parentId}", connection);

                SQLiteDataReader reader = command.ExecuteReader();

                reader.Read();

                this.textBox1.Text = reader.GetString(3);
                this.textBox2.Text = reader.GetString(4);

                reader.Close();

            }

            connection.Close();
            

        }

        public static string environmentPath(long rootId, ref SQLiteConnection connection)
        {
            // recursively generates an environment path

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

                

                var data = reader[2];

                if (!reader.IsDBNull(2))
                
                {
                    environmentId = reader.GetInt32(2);
                }

                string name = reader.GetString(3);

                reader.Close();
                

                return environmentPath(environmentId, ref connection) + @" \ " + name;


            }

            
        }

        private void button2_Click(object sender, EventArgs e)
        {

            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            if (this.action == "CREATE ENVIRONMENT")
            {
                if (this.textBox1.Text.Length == 0 || this.textBox2.Text.Length == 0)
                {
                    MessageBox.Show("Error. All fields must be completed.");
                    return;
                }

                var command = new SQLiteCommand($"INSERT INTO Environment (STATUS, PARENTID, NAME, DESCRIPTION) VALUES (1, {this.label4.Text.ToString()}, '{this.textBox1.Text.ToString()}', '{this.textBox2.Text.ToString()}')", connection);

                if (!Convert.ToBoolean(command.ExecuteNonQuery()))
                {
                    MessageBox.Show("Error in querying database");
                }
                else
                {
                    MessageBox.Show("Successfully queried database");
                    this.Dispose();
                }

            }
            else if (this.action == "MODIFY ENVIRONMENT")
            {
                // updating of name and description only...CANNOT UPDATE THE PARENTID OF THE ENVIRONMENT
                var command = new SQLiteCommand($"UPDATE Environment SET NAME = '{this.textBox1.Text}', DESCRIPTION = '{this.textBox2.Text}' WHERE ID = {this.parentId}", connection);

                if (!Convert.ToBoolean(command.ExecuteNonQuery()))
                {
                    MessageBox.Show("Error in querying database");
                }
                else
                {
                    MessageBox.Show("Successfully queried database");
                    this.Dispose();
                }

            }
            else if (this.action == "DELETE ENVIRONMENT")
            {

                var command = new SQLiteCommand($"UPDATE Environment SET STATUS = 0 WHERE ID = {this.parentId}", connection);

                if (!Convert.ToBoolean(command.ExecuteNonQuery()))
                {
                    MessageBox.Show("Error in querying database");
                }
                else
                {
                    MessageBox.Show("Successfully queried database");
                    this.Dispose();
                }


            }

            connection.Close();
            

            
        }
    }
}
