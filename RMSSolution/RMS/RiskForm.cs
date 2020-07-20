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
    public partial class RiskForm : Form
    {

        private long parentId;
        private long id;
        private string action;

        public RiskForm(string action)
        {
            InitializeComponent();

            this.action = action;
        }

        public long ParentId { get => parentId; set => parentId = value; }
        public long Id { get => id; set => id = value; }

        private void RiskForm_Load(object sender, EventArgs e)
        {
            this.comboBox1.Items.Add("DAYS");
            this.comboBox1.Items.Add("WEEKS");
            this.comboBox1.Items.Add("MONTHS");
            this.comboBox1.Items.Add("YEARS");


            // establist connection with the database
            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();


            


            if (this.action == "CREATE RISK")
            {
                this.Text = "ACTION: CREATE RISK";
            }
            else if (this.action == "MODIFY RISK")
            {
                this.Text = "ACTION: MODIFY RISK";

                // nothing is readonly but fills in the name and description fields with their previous values
                var command = new SQLiteCommand($"SELECT * FROM Risk WHERE STATUS = 1 AND ID = {this.id}", connection);

                MessageBox.Show(this.id.ToString());
                SQLiteDataReader reader = command.ExecuteReader();

                reader.Read();

                this.textBox1.Text = reader.GetString(3);
                this.textBox2.Text = reader.GetString(4);

                this.numericUpDown1.Value = reader.GetInt32(5);
                this.numericUpDown2.Value = reader.GetInt32(6);


                string periodUnit = reader.GetString(7);

                if (periodUnit == "ONE-TIME")
                {
                    this.comboBox1.SelectedIndex = 0;

                    this.numericUpDown2.Value = 0;
                    this.numericUpDown2.ReadOnly = true;
                }
                else if (periodUnit == "DAYS")
                {
                    this.comboBox1.SelectedIndex = 1;
                }
                else if (periodUnit == "WEEKS")
                {
                    this.comboBox1.SelectedIndex = 2;
                }
                else if (periodUnit == "MONTHS")
                {
                    this.comboBox1.SelectedIndex = 3;
                }
                else if (periodUnit == "YEARS")
                {
                    this.comboBox1.SelectedIndex = 4;
                }


                reader.Close();


            }
            else if (this.action == "DELETE RISK")
            {
                this.Text = "DELETE: CREATE RISK";

                // nothing is readonly but fills in the name and description fields with their previous values
                var command = new SQLiteCommand($"SELECT * FROM Risk WHERE STATUS = 1 AND ID = {this.id}", connection);

                SQLiteDataReader reader = command.ExecuteReader();

                reader.Read();

                this.textBox1.Text = reader.GetString(3);
                this.textBox2.Text = reader.GetString(4);

                this.numericUpDown1.Value = reader.GetInt32(5);
                this.numericUpDown2.Value = reader.GetInt32(6);



                string periodUnit = reader.GetString(7);

                if (periodUnit == "ONE-TIME")
                {
                    this.comboBox1.SelectedIndex = 0;
                }
                else if (periodUnit == "DAYS")
                {
                    this.comboBox1.SelectedIndex = 1;
                }
                else if (periodUnit == "WEEKS")
                {
                    this.comboBox1.SelectedIndex = 2;
                }
                else if (periodUnit == "MONTHS")
                {
                    this.comboBox1.SelectedIndex = 3;
                }
                else if (periodUnit == "YEARS")
                {
                    this.comboBox1.SelectedIndex = 4;
                }


                this.textBox1.ReadOnly = true;
                this.textBox2.ReadOnly = true;
                this.numericUpDown1.ReadOnly = true;
                this.numericUpDown2.ReadOnly = true;
                this.comboBox1.Enabled = false;


                reader.Close();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            if (this.action == "CREATE RISK")
            {
                if (this.textBox1.Text.Length == 0 || this.textBox2.Text.Length == 0 || this.comboBox1.SelectedIndex == -1)
                {
                    MessageBox.Show("Error. Any field with (*) cannot be left blank.");
                    return;
                }

                
                long environmentId = this.parentId;
                string hazard = this.textBox1.Text.ToString();
                string evaluation = this.textBox2.Text.ToString();
                long cost = long.Parse(this.numericUpDown1.Value.ToString());
                long period = long.Parse(this.numericUpDown2.Value.ToString());
                string periodUnit = this.comboBox1.SelectedItem.ToString();

                MessageBox.Show(cost.ToString());
                MessageBox.Show(period.ToString());

                string cmd = $"INSERT INTO Risk (STATUS, ENVIRONMENTID, HAZARD, EVALUATION, COST, PERIOD, PERIODUNIT) VALUES (1, {environmentId}, '{hazard}', '{evaluation}', {cost}, {period}, '{periodUnit}')";

                MessageBox.Show(cmd);
                    
                var command = new SQLiteCommand(cmd, connection);

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
            else if (this.action == "MODIFY RISK")
            {
                // updating of name and description only...CANNOT UPDATE THE PARENTID OF THE ENVIRONMENT

                string hazard = this.textBox1.Text;
                string evaluation = this.textBox2.Text;
                long cost = long.Parse(this.numericUpDown1.Value.ToString());
                long period = long.Parse(this.numericUpDown2.Value.ToString());
                string periodUnit = this.comboBox1.SelectedItem.ToString();
                
                var command = new SQLiteCommand($"" +
                    $"UPDATE Risk SET " +
                    $"HAZARD = '{hazard}', " +
                    $"EVALUATION = '{evaluation}', " +
                    $"COST = {cost}, " +
                    $"PERIOD = {period}, " +
                    $"PERIODUNIT = '{periodUnit}'" +
                    $"WHERE ID = {this.Id}", connection);

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
            else if (this.action == "DELETE RISK")
            {

                var command = new SQLiteCommand($"UPDATE Risk SET STATUS = 0 WHERE ID = {this.Id}", connection);

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
