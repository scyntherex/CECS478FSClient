using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSchatFront
{
    public static class DataContainer
    {
        public static RSAParameters publicKey;
        public static RSAParameters privateKey;

        public static string ValueToShare;
        public static string User;
        public static string messageToUser;
    }
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

     
    }
}
