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
using Flurl;
using System.Security.Cryptography;
using System.IO;

namespace FSchatFront
{
    //TODO
    //exchange keys 
    //encrypt/decrypt

    //user checks if they have a pair key
    // if user wants to send message, check for priv and pub keys from other users.
    // if no publc for recipient, import, cause cant encrypt w/o public key.

    // to decrypt, use the priv key

    public partial class Form1 : Form
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";

        }
        // Declare CspParmeters and RsaCryptoServiceProvider
        // objects with global scope of your Form class.
        System.Security.Cryptography.CspParameters cspp = new System.Security.Cryptography.CspParameters();
        System.Security.Cryptography.RSACryptoServiceProvider rsa;

        // Path variables for source, encryption, and
        // decryption folders. Must end with a backslash.
        const string EncrFolder = @"c:\Encrypt\";
        const string DecrFolder = @"c:\Decrypt\";
        const string SrcFolder = @"c:\docs\";

        // Public key file


        // Key container name for
        // private/public key value pair.
        public string pubKey;
        public string privKey;

        public enum KeySizes
        {
            SIZE_512 = 512,
            SIZE_1024 = 1024,
            SIZE_2048 = 2048,
            SIZE_952 = 952,
            SIZE_1369 = 1369
        };

        //  const string keyName = "Key01";
        //string keyName = "Key01";
        string keyName = DataContainer.User;

        // Public key file
        string PubKeyFile = @"c:\Encrypt\rsaPublicKey_";

        string message;

        private RootObject user;
        public string ConversationID;

        string sending2;

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
        string em;
        private void button2_Click(object sender, EventArgs e)
        {
            em = email.Text;
            DataContainer.User = email.Text;
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
                //textBox1.Text = user.auth_token;

                if (responseMessage.IsSuccessStatusCode)
                {
                    linkLabel1.Visible = true;
                    getUsers();
                    users_list.Enabled = true;
                    textBox2.Enabled = true;
                    textBox3.Enabled = true;
                    button3.Enabled = true;
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
                    }
                }
            }
        }

        private void users_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //EncrytorM();
                string userToChat2 = users_list.SelectedItem.ToString();
                DataContainer.messageToUser = userToChat2.ToString();
                //encdeckeyx form1 = new encdeckeyx();
                //form1.Show();
                createConveration(DataContainer.messageToUser.ToString());
            }
            catch (System.NullReferenceException)
            {
                MessageBox.Show("Select a Recipient First");
            }
        }

        public string conversation_id { get; set; }

       /* async void conversationsCreate2(string email)
        {
            string sending = sending2;
            string myToken = DataContainer.ValueToShare.ToString();
            string messageTo = DataContainer.messageToUser.ToString();
            var builtUrl = new Url("https://thefsocietychat.herokuapp.com/conversations/create");
            var client2 = builtUrl
                .WithOAuthBearerToken(myToken);
            var resp = await client2
                .WithHeader("Accept", "application/json")
                .PostUrlEncodedAsync(new
                {
                    recipient_email = email
                })
                .ReceiveString()
                ;

            string output = resp.ToString();
            listAllUsers messageID = JsonConvert.DeserializeObject<listAllUsers>(output);
            string output2 = messageID.ToString();
            conversationID = messageID.id.ToString();

            var builtUrl2 = new Url("https://chronoschat.co/messages/create");

            var client3 = builtUrl2
                .WithOAuthBearerToken(myToken);

            try
            {
                var resp2 = await client3
                  .WithHeader("Accept", "application/json")
                  .PostUrlEncodedAsync(new
                  {
                      body = sending,
                      conversation_id = conversationID
                  })
                  .ReceiveString()
                  ;



                string resp2Output = resp2.ToString();
                listAllUsers sendOutput = JsonConvert.DeserializeObject<listAllUsers>(resp2Output);
                string output3 = sendOutput.ToString();
                string sendStatus = sendOutput.status.ToString();
                MessageBox.Show(sendStatus);
                if (sendStatus == "Message Sent")
                {
                    // textBox1.Clear();
                }
                else
                {
                    MessageBox.Show("An unknown error occured. Please try again later");
                }
            }
            catch
            {
                MessageBox.Show("An Error Occured. Message Not Sent.");
            }
        }
        */
        
        
        async void createConveration(string email)
        {
            
            var url = new Url("https://thefsocietychat.herokuapp.com/conversations/create");
            var theclient = url.WithOAuthBearerToken(user.auth_token);
            var response = await theclient
                .WithHeader("Accept", "application/json")
                .PostUrlEncodedAsync(new
                {
                    recipient_email = email
                })
                .ReceiveString()
            ;
            string createConversatioResponse = response.ToString();
            Form1 convoID = JsonConvert.DeserializeObject<Form1>(createConversatioResponse);
            string otherOutput = convoID.ToString();
            ConversationID = convoID.conversation_id.ToString();
            get_mess();
           
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
                        string emailtoremove = email.ToString();
                        users_list.Items.Remove(em);
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

        
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            message = textBox2.Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            mess_send();
        }

        async void mess_send()
        {
            var url = new Url("https://thefsocietychat.herokuapp.com/messages/create");
            var theclient = url.WithOAuthBearerToken(user.auth_token);
            var response = await theclient
                .WithHeader("Accept", "application/json")
                .PostUrlEncodedAsync(new
                {
                    conversation_id = ConversationID,
                    body = textBox2.Text
                })
                .ReceiveString()
            ;

            string result = response.ToString();
            //MessageBox.Show(result);
            get_mess();
        }


        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }

        async void get_mess()
        {
            textBox3.Text = String.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string accesstoken = user.auth_token;
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accesstoken);

                    using (HttpResponseMessage response = await client.GetAsync("https://thefsocietychat.herokuapp.com/messages/index?conversation_id=" 
                        + ConversationID))
                    {
                        byte[] text;
                        
                        string responseJson = await response.Content.ReadAsStringAsync();
                        System.Data.DataSet dataSet = JsonConvert.DeserializeObject<System.Data.DataSet>(responseJson);
                        System.Data.DataTable dataTable = dataSet.Tables["messages"];

                        textBox3.Multiline = true;
                        textBox3.AcceptsReturn = true;
                        textBox3.ScrollBars = ScrollBars.Vertical;
                        foreach (System.Data.DataRow row in dataTable.Rows)
                        {
                            //string text2 = textBox3.Text;

                            //text = Dencrypt9(Convert.FromBase64String(row["body"].ToString()));
                            //textBox3.Text = Encoding.UTF8.GetString(text);

                            textBox3.AppendText((string)row["body"]);
                            textBox3.AppendText(Environment.NewLine);
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

        private void CreateAsmKeys_Click(object sender, EventArgs e)
        {
            DataContainer.User = username.Text;
            DialogResult dialogResult = MessageBox.Show("This will overwrite any existing keys for " + DataContainer.User.ToString() + ". Do you want to continue?", "WARNING", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                MessageBox.Show("New KeySet Created");
                cspp.KeyContainerName = DataContainer.User;

                rsa = new RSACryptoServiceProvider(2048, cspp);
                //store a key pair in the key container.

                rsa.PersistKeyInCsp = true;
                if (rsa.PublicOnly == true)
                    label1.Text = "Key: " + cspp.KeyContainerName + " - Public Only";
                else
                    label1.Text = "Key: " + cspp.KeyContainerName + " - Full Key Pair";


                string promptValue = encdeckeyx.ShowDialog("Enter a file name", "New File");
                // MessageBox.Show(promptValue);

                string keyFileName = PubKeyFile + promptValue + ".txt";

                Directory.CreateDirectory(EncrFolder);
                StreamWriter sw = new StreamWriter(keyFileName, false);
                sw.Write(rsa.ToXmlString(false));
                sw.Close();
                MessageBox.Show("Public Key Exported to:" + keyFileName);
                DataContainer.privateKey = rsa.ExportParameters(true);

            }
            else if (dialogResult == DialogResult.No)
            {
                //do nothing
            }

        }

        private void Encrypt_Click(object sender, EventArgs e)
        {
            EncrytorM();
        }

        void EncrytorM()
        {
            try
            {
                byte[] data = Encrypt9(Encoding.UTF8.GetBytes(message), ref pubKey);
                sending2 = Convert.ToBase64String(data);
                textBox2.Text = sending2;

            }
            catch
            {
                MessageBox.Show("Set a Public Key First");
            }

        }
        static byte[] Encrypt9(byte[] input, ref string key2)
        {
            byte[] encrypted;
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                try
                {
                    rsa.ImportParameters(DataContainer.publicKey);
                    encrypted = rsa.Encrypt(input, true);
                    key2 = rsa.ToXmlString(false);

                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    encrypted = null;
                    MessageBox.Show("No Public Key Set");
                }

            }

            return encrypted;
        }

        private void Decrypt_Click(object sender, EventArgs e)
        {
            byte[] text;
            string text2 = textBox3.Text;
            text = Dencrypt9(Convert.FromBase64String(text2));
            textBox3.Text = Encoding.UTF8.GetString(text);
        }

        static byte[] Dencrypt9(byte[] input)
        {
            byte[] dencrypted;
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportParameters(DataContainer.privateKey);
                try
                {
                    dencrypted = rsa.Decrypt(input, true);
                }
                catch
                {
                    MessageBox.Show("Incorrect Private Key");
                    dencrypted = null;
                }


            }
            return dencrypted;
        }

        private void ExportPubKey_Click(object sender, EventArgs e)
        {
            
        }

        private void ImportPubKey_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = SrcFolder;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fName = openFileDialog1.FileName;
                if (fName != null)
                {
                    FileInfo fInfo = new FileInfo(fName);
                    // Pass the file name without the path.
                    string name = fInfo.FullName;
                    StreamReader sr = new StreamReader(name);
                    cspp.KeyContainerName = keyName;
                    rsa = new RSACryptoServiceProvider(2048, cspp);

                    string keytxt = sr.ReadToEnd();
                    rsa.FromXmlString(keytxt);
                    rsa.PersistKeyInCsp = true;
                    DataContainer.publicKey = rsa.ExportParameters(false);
                    if (rsa.PublicOnly == true)
                        label1.Text = "Key: Public Only";
                    else
                        label1.Text = "Key: Full Key Pair for: " + cspp.KeyContainerName;
                    sr.Close();

                }
            }
        }

        private void GetPrivKey_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            users_list.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            button3.Enabled = false;
        }

        private void email_TextChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            encdeckeyx otherform = new encdeckeyx();
            otherform.Show();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
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