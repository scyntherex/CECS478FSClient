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
            autheticateUser("https://thefsocietychat.herokuapp.com/home");
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
                //textBox1.Text = responseJson;
              

                if (responseMessage.IsSuccessStatusCode)
                {
                    linkLabel1.Visible = true;
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
                        textBox1.Text = output;
                    }
                }
            }
        }
    }
}
