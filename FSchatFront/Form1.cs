using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Flurl.Http;
using Newtonsoft.Json;
using System.Net.Http;

namespace FSchatFront
{


    public partial class Form1 : Form
    {

        private RootObject user;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Register newUser = new Register();
            newUser.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String em = email.Text;
            String pw = password.Text;
            loginUser();
            //autheticateUser("https://thefsocietychat.herokuapp.com/home");
        }

        async void loginUser()
        {
            try
            {
                HttpResponseMessage responseMessage = await "https://thefsocietychat.herokuapp.com/auth_user".PostUrlEncodedAsync(new
                {
                    email = email.Text.ToString(),
                    password = password.Text.ToString(),
                });
                string responseJson = await responseMessage.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<RootObject>(responseJson);
                textBox1.Text = user.auth_token;

                if (responseMessage.IsSuccessStatusCode)
                {
                    linkLabel1.Visible = true;
                    getUsers();
                }




            }
            catch (FlurlHttpTimeoutException)
            {
                MessageBox.Show("Timed out!");
            }
            catch (FlurlHttpException ex)
            {
                if (ex.Call.Response != null)
                    MessageBox.Show("Failed with response code " + ex.Call.Response.StatusCode);
                else
                    MessageBox.Show("Totally failed before getting a response! " + ex.Message);
            }
        }

        async void autheticateUser(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    using (HttpContent content = response.Content)
                    {

                        string response2 = await content.ReadAsStringAsync();
                        string output = response2.ToString();
                        //textBox1.Text = output;

                    }
                }
            }
        }

        private void users_list_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        async void getUsers()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string accesstoken = user.auth_token;
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accesstoken);

                    using (HttpResponseMessage response = await client.GetAsync("https://thefsocietychat.herokuapp.com/conversations/index"))
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        System.Data.DataSet dataSet = JsonConvert.DeserializeObject<System.Data.DataSet>(responseJson);
                        System.Data.DataTable dataTable = dataSet.Tables["users"];

                        foreach(System.Data.DataRow row in dataTable.Rows)
                        {
                            users_list.Items.Add(row["email"]);
                        }
                    }
                }
              
            }
            catch (FlurlHttpTimeoutException)
            {
                MessageBox.Show("Timed out!");
            }
            catch (FlurlHttpException ex)
            {
                if (ex.Call.Response != null)
                    MessageBox.Show("Failed with response code " + ex.Call.Response.StatusCode);
                else
                    MessageBox.Show("Totally failed before getting a response! " + ex.Message);
            }
            
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }
    }

    public class User
    {
        public int id { get; set; }
        public string email { get; set; }
    }

    public class RootObject
    {
        public string auth_token { get; set; }
        public User user { get; set; }
    }

    public class UserList
    {
        public int id { get; set; }
        public string email { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}