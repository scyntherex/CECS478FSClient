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

        // Declare CspParmeters and RsaCryptoServiceProvider
        // objects with global scope of your Form class.
        CspParameters cspp = new CspParameters();
        RSACryptoServiceProvider rsa;

        // Path variables for source, encryption, and
        // decryption folders. Must end with a backslash.
        const string EncrFolder = @"c:\Users\Rone\Desktop\Encrypt";
        const string DecrFolder = @"c:\Users\Rone\Desktop\Decrypt";
        const string SrcFolder = @"c:\Users\Rone\Desktop\src";

        // Public key file
        const string PubKeyFile = @"c:\Users\Rone\Desktop\Encrypt\rsaPublicKey.txt";

        // Key container name for
        // private/public key value pair.
        const string keyName = "Key01";

        private RootObject user;
        public string ConversationID;
        
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
            createConveration(users_list.SelectedItem.ToString());
        }
        
        public string conversation_id { get; set; }
        
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
                        string responseJson = await response.Content.ReadAsStringAsync();
                        System.Data.DataSet dataSet = JsonConvert.DeserializeObject<System.Data.DataSet>(responseJson);
                        System.Data.DataTable dataTable = dataSet.Tables["messages"];

                        textBox3.Multiline = true;
                        textBox3.AcceptsReturn = true;
                        textBox3.ScrollBars = ScrollBars.Vertical;
                        foreach (System.Data.DataRow row in dataTable.Rows)
                        {
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

        private void EncryptFile(string inFile)
        {

            // Create instance of Rijndael for
            // symetric encryption of the data.
            RijndaelManaged rjndl = new RijndaelManaged();
            rjndl.KeySize = 256;
            rjndl.BlockSize = 256;
            rjndl.Mode = CipherMode.CBC;
            ICryptoTransform transform = rjndl.CreateEncryptor();

            // Use RSACryptoServiceProvider to
            // enrypt the Rijndael key.
            // rsa is previously instantiated: 
            //    rsa = new RSACryptoServiceProvider(cspp);
            byte[] keyEncrypted = rsa.Encrypt(rjndl.Key, false);

            // Create byte arrays to contain
            // the length values of the key and IV.
            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            int lKey = keyEncrypted.Length;
            LenK = BitConverter.GetBytes(lKey);
            int lIV = rjndl.IV.Length;
            LenIV = BitConverter.GetBytes(lIV);

            // Write the following to the FileStream
            // for the encrypted file (outFs):
            // - length of the key
            // - length of the IV
            // - ecrypted key
            // - the IV
            // - the encrypted cipher content

            int startFileName = inFile.LastIndexOf("\\") + 1;
            // Change the file's extension to ".enc"
            string outFile = EncrFolder + inFile.Substring(startFileName, inFile.LastIndexOf(".") - startFileName) + ".enc";

            using (FileStream outFs = new FileStream(outFile, FileMode.Create))
            {

                outFs.Write(LenK, 0, 4);
                outFs.Write(LenIV, 0, 4);
                outFs.Write(keyEncrypted, 0, lKey);
                outFs.Write(rjndl.IV, 0, lIV);

                // Now write the cipher text using
                // a CryptoStream for encrypting.
                using (CryptoStream outStreamEncrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                {

                    // By encrypting a chunk at
                    // a time, you can save memory
                    // and accommodate large files.
                    int count = 0;
                    int offset = 0;

                    // blockSizeBytes can be any arbitrary size.
                    int blockSizeBytes = rjndl.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];
                    int bytesRead = 0;

                    using (FileStream inFs = new FileStream(inFile, FileMode.Open))
                    {
                        do
                        {
                            count = inFs.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamEncrypted.Write(data, 0, count);
                            bytesRead += blockSizeBytes;
                        }
                        while (count > 0);
                       // string temp_inBase64 = Convert.ToBase64String(data);
                       // inFs.WriteAllLines(@"C:\Users\Rone\Desktop\EncryptrsaPublicKey.enc", temp_inBase64);
                        inFs.Close();
                    }
                    outStreamEncrypted.FlushFinalBlock();
                    outStreamEncrypted.Close();
                }
                outFs.Close();
            }

        }

        private void DecryptFile(string inFile)
        {

            // Create instance of Rijndael for
            // symetric decryption of the data.
            RijndaelManaged rjndl = new RijndaelManaged();
            rjndl.KeySize = 256;
            rjndl.BlockSize = 256;
            rjndl.Mode = CipherMode.CBC;

            // Create byte arrays to get the length of
            // the encrypted key and IV.
            // These values were stored as 4 bytes each
            // at the beginning of the encrypted package.
            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            // Consruct the file name for the decrypted file.
            string outFile = DecrFolder + inFile.Substring(0, inFile.LastIndexOf(".")) + ".txt";

            // Use FileStream objects to read the encrypted
            // file (inFs) and save the decrypted file (outFs).
            using (FileStream inFs = new FileStream(EncrFolder + inFile, FileMode.Open))
            {

                inFs.Seek(0, SeekOrigin.Begin);
                inFs.Seek(0, SeekOrigin.Begin);
                inFs.Read(LenK, 0, 3);
                inFs.Seek(4, SeekOrigin.Begin);
                inFs.Read(LenIV, 0, 3);

                // Convert the lengths to integer values.
                int lenK = BitConverter.ToInt32(LenK, 0);
                int lenIV = BitConverter.ToInt32(LenIV, 0);

                // Determine the start postition of
                // the ciphter text (startC)
                // and its length(lenC).
                int startC = lenK + lenIV + 8;
                int lenC = (int)inFs.Length - startC;

                // Create the byte arrays for
                // the encrypted Rijndael key,
                // the IV, and the cipher text.
                byte[] KeyEncrypted = new byte[lenK];
                byte[] IV = new byte[lenIV];

                // Extract the key and IV
                // starting from index 8
                // after the length values.
                inFs.Seek(8, SeekOrigin.Begin);
                inFs.Read(KeyEncrypted, 0, lenK);
                inFs.Seek(8 + lenK, SeekOrigin.Begin);
                inFs.Read(IV, 0, lenIV);
                Directory.CreateDirectory(DecrFolder);
                // Use RSACryptoServiceProvider
                // to decrypt the Rijndael key.
                byte[] KeyDecrypted = rsa.Decrypt(KeyEncrypted, false);

                // Decrypt the key.
                ICryptoTransform transform = rjndl.CreateDecryptor(KeyDecrypted, IV);

                // Decrypt the cipher text from
                // from the FileSteam of the encrypted
                // file (inFs) into the FileStream
                // for the decrypted file (outFs).
                using (FileStream outFs = new FileStream(outFile, FileMode.Create))
                {

                    int count = 0;
                    int offset = 0;

                    // blockSizeBytes can be any arbitrary size.
                    int blockSizeBytes = rjndl.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];


                    // By decrypting a chunk a time,
                    // you can save memory and
                    // accommodate large files.

                    // Start at the beginning
                    // of the cipher text.
                    inFs.Seek(startC, SeekOrigin.Begin);
                    using (CryptoStream outStreamDecrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                    {
                        do
                        {
                            count = inFs.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamDecrypted.Write(data, 0, count);

                        }
                        while (count > 0);

                        outStreamDecrypted.FlushFinalBlock();
                        outStreamDecrypted.Close();
                    }
                    outFs.Close();
                }
                inFs.Close();
            }

        }

        private void CreateAsmKeys_Click(object sender, EventArgs e)
        {
            // Stores a key pair in the key container.
            cspp.KeyContainerName = keyName;
            rsa = new RSACryptoServiceProvider(cspp);
            rsa.PersistKeyInCsp = true;
            if (rsa.PublicOnly == true)
                label1.Text = "Key: " + cspp.KeyContainerName + " - Public Only";
            else
                label1.Text = "Key: " + cspp.KeyContainerName + " - Full Key Pair";

        }

        private void Encrypt_Click(object sender, EventArgs e)
        {
            if (rsa == null)
                MessageBox.Show("Key not set.");
            else
            {

                // Display a dialog box to select a file to encrypt.
                openFileDialog1.InitialDirectory = SrcFolder;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string fName = openFileDialog1.FileName;
                    if (fName != null)
                    {
                        FileInfo fInfo = new FileInfo(fName);
                        // Pass the file name without the path.
                        string name = fInfo.FullName;
                        EncryptFile(name);
                    }
                }
            }
        }

        private void Decrypt_Click(object sender, EventArgs e)
        {
            if (rsa == null)
                MessageBox.Show("Key not set.");
            else
            {
                // Display a dialog box to select the encrypted file.
                openFileDialog2.InitialDirectory = EncrFolder;
                if (openFileDialog2.ShowDialog() == DialogResult.OK)
                {
                    string fName = openFileDialog2.FileName;
                    if (fName != null)
                    {
                        FileInfo fi = new FileInfo(fName);
                        string name = fi.Name;
                        DecryptFile(name);
                    }
                }
            }
        }

        private void ExportPubKey_Click(object sender, EventArgs e)
        {
            // Save the public key created by the RSA
            // to a file. Caution, persisting the
            // key to a file is a security risk.
            Directory.CreateDirectory(EncrFolder);
            StreamWriter sw = new StreamWriter(PubKeyFile, false);
            sw.Write(rsa.ToXmlString(false));
            sw.Close();
        }

        private void ImportPubKey_Click(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader(PubKeyFile);
            cspp.KeyContainerName = keyName;
            rsa = new RSACryptoServiceProvider(cspp);
            string keytxt = sr.ReadToEnd();
            rsa.FromXmlString(keytxt);
            rsa.PersistKeyInCsp = true;
            if (rsa.PublicOnly == true)
                label1.Text = "Key: " + cspp.KeyContainerName + " - Public Only";
            else
                label1.Text = "Key: " + cspp.KeyContainerName + " - Full Key Pair";
            sr.Close();
        }

        private void GetPrivKey_Click(object sender, EventArgs e)
        {
            cspp.KeyContainerName = keyName;

            rsa = new RSACryptoServiceProvider(cspp);
            rsa.PersistKeyInCsp = true;

            if (rsa.PublicOnly == true)
                label1.Text = "Key: " + cspp.KeyContainerName + " - Public Only";
            else
                label1.Text = "Key: " + cspp.KeyContainerName + " - Full Key Pair";
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