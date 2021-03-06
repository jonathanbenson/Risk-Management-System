﻿using System;
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
         * 
         * QUERY ACTION -> OPEN CORROSPONDING FORM (NON-MODAL) -> VALIDATION FORM (MODAL)
         * ACTIONS
         * CREATE ENVIRONMENT
         * CREATE RISK
         * CREATE PROPOSAL
         * CREATE TRIAL
         * 
         * 
         * ***CORROSPONDING PATHS CANNOT BE EDITED***
         * MODIFY ENVIRONMENT
         * MODIFY RISK
         * MODIFY PROPOSAL
         * MODIFY TRIAL
         * 
         * ***CANNOT DELETE IF NODE CONTAINS SUBNODES***
         * DELETE ENVIRONMENT
         * DELETE RISK
         * DELETE PROPOSAL
         * DELETE TRIAL
         * 
         * 
         */
        

        

        TreeNode selectedItem;

        public Main()
        {
            InitializeComponent();


            this.selectedItem = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            // administrator authentication
            if (this.textBox1.Text != "admin" && this.textBox2.Text != "Administrator")
            {
                MessageBox.Show("Invalid Username or Password.");
            }
            else
            {
                this.groupBox2.Visible = true;
            }

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
            rootNode.Tag = reader.GetInt32(0);
            treeView1.Nodes.Add(rootNode);

            this.populateBrowseEnvironmentTreeHelper(ref rootNode, rootId, connection);

            reader.Close();
            connection.Close();

            this.treeView1.SelectedNode = this.treeView1.Nodes[0];
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

            reader.Close();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            


            this.comboBox1.Items.Clear();

            this.selectedItem = this.treeView1.SelectedNode;

            // populating of risks and proposals in the second tree view on the main screen

            // updating of right text box label
            this.label4.Text = $"Environment #{this.selectedItem.Tag}";
            this.textBox4.Text = this.selectedItem.Text;


            SQLiteHandler handl2 = new SQLiteHandler();

            var connection2 = handl2.getConnection();

            connection2.Open();

            

            var command2 = new SQLiteCommand($"SELECT * FROM Environment WHERE STATUS = 1 AND ID = {this.treeView1.SelectedNode.Tag}", connection2);

            SQLiteDataReader environmentReader = command2.ExecuteReader();

            this.textBox4.Text = $"{this.selectedItem.Text}\r\n\r\n";
            if (environmentReader.Read())
            {
                this.textBox4.Text += environmentReader.GetString(4);
            }

            environmentReader.Close();
            
            connection2.Close();

            if (treeView1.SelectedNode.Tag == null)
            {
                return;
            }
            


            //

            this.treeView2.Nodes.Clear();

            SQLiteHandler handl = new SQLiteHandler();

            var connection = handl.getConnection();

            connection.Open();

            var command = new SQLiteCommand($"SELECT * FROM Risk WHERE STATUS = 1 AND ENVIRONMENTID = {this.treeView1.SelectedNode.Tag}", connection);

            SQLiteDataReader riskReader = command.ExecuteReader();

            while (riskReader.Read())
            {


                TreeNode riskNode = new TreeNode(riskReader.GetString(3));
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

                    trialReader.Close();

                    // end of adding trials to proposals


                }

                proposalReader.Close();
                // done with adding proposal underneath the risks

                
                this.treeView2.Nodes.Add(riskNode);


            }
            riskReader.Close();
            connection.Close();

            // end of populating of risks and proposals in the second tree view on the main screen




            // updating selected item type and selected item description text boxes, as well as combo box





            // updating combo box items
            this.comboBox1.Items.Add("CREATE RISK");
            this.comboBox1.Items.Add("CREATE ENVIRONMENT");
            this.comboBox1.Items.Add("MODIFY ENVIRONMENT");
            this.comboBox1.Items.Add("DELETE ENVIRONMENT");

           


            // updating selected item type and selected item description text boxes

            


        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("You must select an action!");
            }
            else if (this.comboBox1.SelectedItem.ToString() == "CREATE ENVIRONMENT")
            {
                // Assumed to have selected an environment at this point

                EnvironmentForm ef = new EnvironmentForm("CREATE ENVIRONMENT");

                ef.ParentId = long.Parse(this.selectedItem.Tag.ToString());

                ef.ShowDialog();

                this.populateBrowseEnvironmentTree();

            }
            else if (this.comboBox1.SelectedItem.ToString() == "CREATE RISK")
            {
                RiskForm rf = new RiskForm("CREATE RISK");

                rf.ParentId = long.Parse(this.selectedItem.Tag.ToString());

                rf.ShowDialog();

                this.populateBrowseEnvironmentTree();

            }
            else if (this.comboBox1.SelectedItem.ToString() == "CREATE PROPOSAL")
            {

            }
            else if (this.comboBox1.SelectedItem.ToString() == "CREATE TRIAL")
            {

            }
            else if (this.comboBox1.SelectedItem.ToString() == "MODIFY ENVIRONMENT")
            {
                // Assumed to have selected an environment at this point

                EnvironmentForm ef = new EnvironmentForm("MODIFY ENVIRONMENT");

                ef.ParentId = long.Parse(this.selectedItem.Tag.ToString());

                ef.ShowDialog();

                this.populateBrowseEnvironmentTree();
            }
            else if (this.comboBox1.SelectedItem.ToString() == "MODIFY RISK")
            {
                // Assumed to have selected an environment at this point

                RiskForm rf = new RiskForm("MODIFY RISK");

                string tag = this.selectedItem.Tag.ToString();

                rf.Id = long.Parse(tag.Substring(2, tag.Length - 2));

                MessageBox.Show(rf.Id.ToString());

                rf.ShowDialog();

                this.populateBrowseEnvironmentTree();

            }
            else if (this.comboBox1.SelectedItem.ToString() == "MODIFY PROPOSAL")
            {

            }
            else if (this.comboBox1.SelectedItem.ToString() == "MODIFY TRIAL")
            {

            }
            else if (this.comboBox1.SelectedItem.ToString() == "DELETE ENVIRONMENT")
            {

                // check to see if environment has any child nodes
                // user cannot delete any environments that have sub environments or risks

                if (this.selectedItem.Nodes.Count != 0 || this.treeView2.Nodes.Count != 0)
                {
                    MessageBox.Show("Error. Cannot delete an environment with sub environments or risks.");
                }
                else
                {
                    EnvironmentForm ef = new EnvironmentForm("DELETE ENVIRONMENT");

                    ef.ParentId = long.Parse(this.selectedItem.Tag.ToString());


                    ef.ShowDialog();

                    this.populateBrowseEnvironmentTree();

                }

            }
            else if (this.comboBox1.SelectedItem.ToString() == "DELETE RISK")
            {
                if (this.selectedItem.Nodes.Count != 0)
                {
                    MessageBox.Show("Error. Cannot delete a risk with proposals.");
                }
                else
                {
                    RiskForm rf = new RiskForm("DELETE RISK");

                    string tag = this.selectedItem.Tag.ToString();

                    rf.Id = long.Parse(tag.Substring(2, tag.Length - 2));


                    rf.ShowDialog();

                    this.populateBrowseEnvironmentTree();

                }

            }
            else if (this.comboBox1.SelectedItem.ToString() == "DELETE PROPOSAL")
            {

            }
            else if (this.comboBox1.SelectedItem.ToString() == "DELETE TRIAL")
            {

            }

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
                // updating of right text box label
                this.label4.Text = $"Risk #{tag.Substring(2, tag.Length - 2)}";

                // displaying item information in text box to the right
                this.displayRisk(tag.Substring(2, tag.Length - 2), ref connection);

                // populating combo box of actions
                this.comboBox1.Items.Add("CREATE PROPOSAL");
                this.comboBox1.Items.Add("MODIFY RISK");
                this.comboBox1.Items.Add("DELETE RISK");
               

            }
            else if (tag[0] == '2')
                // selected node is a proposal
            {

                // updating of right text box label
                this.label4.Text = $"Proposal #{tag.Substring(2, tag.Length - 2)}";

                // displaying item information in text box to the right
                this.displayProposal(tag.Substring(2, tag.Length - 2), ref connection);

                // populating combo box of items
                this.comboBox1.Items.Add("CREATE TRIAL");
                this.comboBox1.Items.Add("MODIFY PROPOSAL");
                this.comboBox1.Items.Add("DELETE PROPOSAL");
               


            }
            else if (tag[0] == '3')
                // selected node is a trial
            {
                // updating of right text box label
                this.label4.Text = $"Trial #{tag.Substring(2, tag.Length - 2)}";

                // displaying item information in text box to the right
                this.displayTrial(tag.Substring(2, tag.Length - 2), ref connection);


                // populating combo box of items
                this.comboBox1.Items.Add("MODIFY TRIAL");
                this.comboBox1.Items.Add("DELETE TRIAL");
                

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
                string text = "";


                text += $"HAZARD\r\n";
                text += $"--------------------------------\r\n";
                text += $"{reader.GetString(3)}\r\n";
                text += $"--------------------------------\r\n\r\n";

                text += $"EVALUATION\r\n";
                text += $"--------------------------------\r\n";
                text += $"{reader.GetString(4)}\r\n";
                text += $"--------------------------------\r\n\r\n";

                text += $"EST. COST\r\n";
                text += $"--------------------------------\r\n";
                text += $"${reader.GetInt32(5)} every {reader.GetInt32(6)} {reader.GetString(7)}\r\n";
                text += $"--------------------------------";


                this.textBox4.Text = text;
            }

            reader.Close();
        }

        private void displayProposal(string id, ref SQLiteConnection connection)
        {
            var command = new SQLiteCommand($"SELECT * FROM Proposal WHERE STATUS = 1 AND ID = {id}", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            
            if (reader.Read())
            {
                string text = "";

                
                text += $"ANALYSIS\r\n";
                text += $"--------------------------------\r\n";
                text += $"{reader.GetString(3)}\r\n";
                text += $"--------------------------------\r\n\r\n";

                text += $"TREATMENT\r\n";
                text += $"--------------------------------\r\n";
                text += $"{reader.GetString(4)}\r\n";
                text += $"--------------------------------\r\n\r\n";

                text += $"EST. NET COST\r\n";
                text += $"--------------------------------\r\n";
                text += $"${reader.GetInt32(5)} every {reader.GetInt32(6)} day(s)\r\n";
                text += $"--------------------------------";
                

                this.textBox4.Text = text;
            }

            reader.Close();
        }

        private void displayTrial(string id, ref SQLiteConnection connection)
        {
            var command = new SQLiteCommand($"SELECT * FROM Trial WHERE STATUS = 1 AND ID = {id}", connection);

            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                string text = "";


                text += $"ASSESSMENT\r\n";
                text += $"--------------------------------\r\n";
                text += $"{reader.GetString(3)}\r\n";
                text += $"--------------------------------\r\n\r\n";

                text += $"VERDICT\r\n";
                text += $"--------------------------------\r\n";
                text += $"{reader.GetString(4)}\r\n";
                text += $"--------------------------------\r\n\r\n";


                text += $"NET COST\r\n";
                text += $"--------------------------------\r\n";
                text += $"${reader.GetInt32(5)} every {reader.GetInt32(6)} day(s)\r\n";
                text += $"--------------------------------";

                this.textBox4.Text = text;
            }

            reader.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.populateBrowseEnvironmentTree();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
        // end of display functions for information box

    }
}
