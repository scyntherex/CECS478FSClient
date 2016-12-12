using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Flurl.Http;
using Newtonsoft.Json;
using System.Net.Http;

namespace FSchatFront
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String em = email.Text;
            String pw = password.Text;
            String pw_confirm = password_confirmation.Text;

            //textBox4.Text = "email: " + email + "@pw: " + pw + "@pwconf" + pw_confirm;
            registerUser();

        }

        async void registerUser()
        {
           try
           {
               HttpResponseMessage responseMessage = await "https://thefsocietychat.herokuapp.com/reg_user"
                    .PostUrlEncodedAsync(new
               {
                   email = email.Text.ToString(),
                   password = password.Text.ToString(),
                   password_confirmation = password_confirmation.Text.ToString()
               });
                string responseJson = await responseMessage.Content.ReadAsStringAsync();
                //textBox4.Text = responseJson;   
 //               MessageBox.Show(responseJson);
                if (responseJson == "{user created}")
                {
                    MessageBox.Show("User Created");
                    this.Hide();
                    Form1 form1 = new Form1();
                    form1.Show();
                }
                else
                {
                    MessageBox.Show("An error has occured. User not created.");
                    email.Clear();
                    password.Clear();
                    password_confirmation.Clear();
                    
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
    }
}
