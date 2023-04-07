using System;
using System.Windows.Forms;

namespace RV_Convertor
{
    public partial class LogForm : Form
    {
        /* Getting access to the main form */
        mainForm mainForm = new mainForm();

        /* A variable for responding to the LogIn form */
        private bool isAdmin = false;
        public bool IsAdmin { get { return isAdmin; }}

        /* Here you can change the login and password to log in */
        private string[,] adminData = { { "admin", "password" }, {"root" ,""} };

        public LogForm()
        {
            InitializeComponent();
        }

        /* The main function that handles the click */
        private void ButtonLogIn_Click(object sender, EventArgs e)
        {
            string enteredLogin = textBoxLogin.Text;
            string enterdPassword = textBoxPassword.Text;

            /* Comparing the data specified by the user */
            for (int i = 0; i < adminData.GetLength(0); i++)
                if(adminData[i,0] == enteredLogin)
                    if(adminData[i,1] == enterdPassword) isAdmin = true;

            /* We give out the result */
            if (isAdmin)
            {
                MessageBox.Show("Access is allowed!\n  Hello Admin!");
                mainForm.IsAdmin = isAdmin;
                this.Close();
            } else MessageBox.Show("Uncorrect login or password. Try again.");
        }

        /* Show or encrypt password */
        private void checkBoxShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowPass.Checked)
                textBoxPassword.PasswordChar = '\0';
            else textBoxPassword.PasswordChar = '*';
        }
    }
}
