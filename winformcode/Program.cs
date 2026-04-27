using System;
using System.Windows.Forms;
using HospitalApp.Forms;

namespace HospitalApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
    }
}