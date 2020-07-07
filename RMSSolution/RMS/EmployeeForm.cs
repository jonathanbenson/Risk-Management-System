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
    public partial class EmployeeForm : Form
    {
        public EmployeeForm()
        {
            InitializeComponent();
        }

        private void EmployeeForm_Load(object sender, EventArgs e)
        {
            
        }

        public void loadEmployees()
        {
           
            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            var command = new SQLiteCommand($"SELECT * FROM Employee WHERE STATUS = 1", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                TreeNode tn = new TreeNode(reader.GetString(5) + ' ' + reader.GetString(6));
                tn.Tag = reader.GetInt32(0).ToString();
                this.treeView1.Nodes.Add(tn);
                
            }

            connection.Close();

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            var command = new SQLiteCommand($"SELECT * FROM Employee WHERE STATUS = 1 AND ID = {this.treeView1.SelectedNode.Tag.ToString()}", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                this.textBox1.Text = reader.GetString(5);
                this.textBox2.Text = reader.GetString(6);
                this.textBox3.Text = reader.GetInt32(2).ToString();
                this.textBox4.Text = reader.GetInt32(3).ToString();
                this.numericUpDown1.Value = reader.GetInt32(4);
            }

            connection.Close();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            var command = new SQLiteCommand($"SELECT * FROM Employee WHERE STATUS = 1 AND ID = {this.treeView1.SelectedNode.Tag.ToString()}", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                
            }

            connection.Close();
        }
    }
}
