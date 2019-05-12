using System;
using BotFramework;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System.IO;

namespace UI
{
    public partial class Login : MetroForm
    {
        public static bool LoadCompleted = false;
        public static bool Verified = false;
        private static string server = "localhost";
        
        public Login()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (LoadCompleted && Verified)
            {
                Close();
            }
        }

        private void Button1_MouseEnter(object sender, EventArgs e)
        {
            (sender as Button).BackColor = Color.DarkGray;
        }

        private void Button1_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).BackColor = Color.DimGray;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Username is empty");
                textBox1.Focus();
                return;
            }
            if (textBox2.Text == "")
            {
                MessageBox.Show("Password is empty");
                textBox2.Focus();
                return;
            }
            string connectionString = $"datasource={server};port=3306;username=root;password=;database=test;";
            MySqlConnection databaseConnection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand($"SELECT password FROM users WHERE email=\"" + textBox1.Text + "\"", databaseConnection);
            commandDatabase.CommandTimeout = 60;
            MySqlDataReader reader;
            try
            {
                databaseConnection.Open();
                reader = commandDatabase.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string salt = reader.GetString(0).Substring(0, reader.GetString(0).LastIndexOf("."));
                        string userinput = BCrypt.Net.BCrypt.HashPassword(textBox2.Text, salt);
                        if (userinput != reader.GetString(0))
                        {
                            MessageBox.Show("Wrong Password!");
                            textBox2.Invoke((MethodInvoker)delegate { textBox2.Text = ""; textBox2.Focus(); });
                        }
                        else
                        {
                            File.WriteAllText("account.auth",textBox1.Text + "=" + BotCore.SHA256(userinput));
                            Verified = true;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("User not found! Please register in our forum before login!");
                    textBox1.Invoke((MethodInvoker)delegate { textBox1.Text = ""; textBox1.Focus(); });
                }
                databaseConnection.Close();
            }
            catch (Exception ex)
            {
                // Show any error message.
                MessageBox.Show(ex.Message);
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if(File.Exists("account.auth"))
            {
                string value = File.ReadAllText("account.auth");
                string connectionString = $"datasource={server};port=3306;username=root;password=;database=test;";
                MySqlConnection databaseConnection = new MySqlConnection(connectionString);
                MySqlCommand commandDatabase = new MySqlCommand($"SELECT password FROM users WHERE email=\"" + value.Split('=')[0] + "\"", databaseConnection);
                commandDatabase.CommandTimeout = 60;
                MySqlDataReader reader;
                try
                {
                    databaseConnection.Open();
                    reader = commandDatabase.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (BotCore.SHA256(reader.GetString(0)) == value.Split('=')[1])
                            {
                                Verified = true;
                            }
                            else
                            {
                                pictureBox1.Visible = false;
                            }
                        }
                    }
                    else
                    {
                        pictureBox1.Visible = false;
                    }
                    databaseConnection.Close();
                }
                catch (Exception ex)
                {
                    if(ex.Message.Contains("Unable to connect to any of the specified MySQL hosts"))
                    {
                        MessageBox.Show("This bot is free to use and open source, PLEASE DO NOT PAY FOR SOMETHING FREE!");
                        Verified = true;
                    }
                    else
                    {
                        MessageBox.Show(ex.Message);
                        Environment.Exit(0);
                    }
                    // Show any error message.

                }
            }
            else
            {
                pictureBox1.Visible = false;
            }
            this.TopMost = true;
            this.Focus();
            this.BringToFront();
            this.TopMost = false;
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Verified)
            {
                DialogResult = DialogResult.Abort;
            }
        }
    }
}
