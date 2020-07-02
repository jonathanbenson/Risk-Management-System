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
using System.Windows.Forms.VisualStyles;
using System.Runtime.CompilerServices;

namespace RMS
{
    public partial class Main : Form
    {
        /*
         * Permissions: 1-3
         * 
         * Cost Period: DAILY, WEEKLY, MONTHLY, QUARTERLY, ANNUALLY
         * 
         * 
         * 
         * 
         * 
         * 
         */
        

        private long dbemployeeid; // keeps track of the employee id number
        private long employeePermission; // keeps track of the permission of the employee logged in

        TreeNode selectedItem;

        public Main()
        {
            InitializeComponent();


            this.selectedItem = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // employee authentication
            if (!Utility.isIntLength4(this.textBox1.Text.ToString()) || !Utility.isIntLength4(this.textBox2.Text.ToString()))
            {
                MessageBox.Show("Error. Employee ID and Password must be a 4-digit number.");
                return;
            }
            


            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            var command = new SQLiteCommand($"SELECT * FROM Employee WHERE STATUS = 1 AND EMPLOYEEID = {this.textBox1.Text} AND PASSWORD = {this.textBox2.Text}", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                this.groupBox2.Visible = true;

                this.dbemployeeid = reader.GetInt32(0);
                this.employeePermission = reader.GetInt32(4);


                
            }
            else
            {
                MessageBox.Show("Invalid Employee ID or Password");
            }

            // end of employee authentication

            this.queryInitialVisibility(this.employeePermission);

            connection.Close();
        }


        private void queryInitialVisibility(long permission)
        {
            
            

            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        

        private void Main_Load(object sender, EventArgs e)
        {
            this.populateBrowseEnvironmentTree();
        }

        private void populateBrowseEnvironmentTree()
        {
            // populates the environment Tree with all of the environments

            treeView1.Nodes.Clear();

            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            var command = new SQLiteCommand($"SELECT * FROM Environment WHERE STATUS = 1 AND PARENTID IS NULL", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            reader.Read();
            int rootId = reader.GetInt32(0);

            TreeNode rootNode = new TreeNode(reader.GetString(3));
            rootNode.Tag = 0;
            treeView1.Nodes.Add(rootNode);

            this.populateBrowseEnvironmentTreeHelper(ref rootNode, rootId, connection);

            connection.Close();
        }

        private void populateBrowseEnvironmentTreeHelper(ref TreeNode tn, long rootId, System.Data.SQLite.SQLiteConnection connection)
        {
            // recursively populating the Environment tree
            var command = new SQLiteCommand($"SELECT * FROM Environment WHERE STATUS = 1 AND PARENTID = {rootId.ToString()}", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (reader.GetInt32(2) == rootId)
                {
                    TreeNode newTreeNode = new TreeNode(reader.GetString(3));
                    newTreeNode.Tag = reader.GetInt32(0);
                    this.populateBrowseEnvironmentTreeHelper(ref newTreeNode, reader.GetInt32(0), connection);
                    tn.Nodes.Add(newTreeNode);
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

            this.comboBox1.Items.Clear();

            this.selectedItem = this.treeView1.SelectedNode;

            // populating of risks and proposals in the second tree view on the main screen
            if (treeView1.SelectedNode.Tag == null)
                return;

            this.treeView2.Nodes.Clear();

            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            var command = new SQLiteCommand($"SELECT * FROM Risk WHERE STATUS = 1 AND ENVIRONMENTID = {this.treeView1.SelectedNode.Tag}", connection);

            SQLiteDataReader riskReader = command.ExecuteReader();

            while (riskReader.Read())
            {
                TreeNode riskNode = new TreeNode(riskReader.GetString(6));
                riskNode.Tag = "1-" + riskReader.GetInt32(0).ToString();

                // adding proposals underneath the risks
                var newCommand = new SQLiteCommand($"SELECT * FROM Proposal WHERE STATUS = 1 AND RISKID = {riskReader.GetInt32(0)}", connection);

                SQLiteDataReader proposalReader = newCommand.ExecuteReader();

                while (proposalReader.Read())
                {
                    TreeNode proposalNode = new TreeNode(proposalReader.GetString(4));
                    proposalNode.Tag = "2-" + proposalReader.GetInt32(0).ToString();
                    riskNode.Nodes.Add(proposalNode);


                    // adding trials to proposals...

                    var newestCommand = new SQLiteCommand($"SELECT * FROM Trial WHERE STATUS = 1 AND PROPOSALID = {riskReader.GetInt32(0)}", connection);

                    SQLiteDataReader trialReader = newestCommand.ExecuteReader();

                    while (trialReader.Read())
                    {
                        TreeNode trialNode = new TreeNode(trialReader.GetString(3));
                        trialNode.Tag = "3-" + trialReader.GetInt32(0).ToString();
                        proposalNode.Nodes.Add(trialNode);

                    }



                    // end of adding trials to proposals


                }


                // done with adding proposal underneath the risks

                
                this.treeView2.Nodes.Add(riskNode);


            }
            connection.Close();

            // end of populating of risks and proposals in the second tree view on the main screen




            // updating selected item type and selected item description text boxes, as well as combo box

            
            this.textBox4.Text = this.selectedItem.Text;

            this.comboBox1.Items.Add("CREATE RISK");
            this.comboBox1.Items.Add("CREATE ENVIRONMENT");


            // updating selected item type and selected item description text boxes

            


        }

        private void button4_Click(object sender, EventArgs e)
        {
            return;
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {

            this.comboBox1.Items.Clear();

            this.selectedItem = this.treeView2.SelectedNode;


            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();


            string tag = this.selectedItem.Tag.ToString();

            if (tag[0] == '1')
                // selected node is a risk
            {
                this.displayRisk(tag.Substring(2, tag.Length - 2), ref connection);
            }
            else if (tag[0] == '2')
                // selected node is a proposal
            {
                this.displayProposal(tag.Substring(2, tag.Length - 2), ref connection);
            }
            else if (tag[0] == '3')
                // selected node is a trial
            {
                this.displayTrial(tag.Substring(2, tag.Length - 2), ref connection);
            }

            connection.Close();

            
        }


        // display functions for the information box
        private void displayRisk(string id, ref SQLiteConnection connection)
        {
            var command = new SQLiteCommand($"SELECT * FROM Risk WHERE STATUS = 1 AND ID = {id}", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                this.textBox4.Text = reader.GetInt32(0).ToString();
            }
        }

        private void displayProposal(string id, ref SQLiteConnection connection)
        {
            var command = new SQLiteCommand($"SELECT * FROM Proposal WHERE STATUS = 1 AND ID = {id}", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                this.textBox4.Text = reader.GetInt32(0).ToString();
            }
        }

        private void displayTrial(string id, ref SQLiteConnection connection)
        {
            var command = new SQLiteCommand($"SELECT * FROM Trial WHERE STATUS = 1 AND ID = {id}", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                this.textBox4.Text = reader.GetInt32(0).ToString();
            }
        }
        // end of display functions for information box

        

    }
}
