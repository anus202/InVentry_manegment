using Prectice_Interview.Data;
using Prectice_Interview.Service;
using System;
using System.Windows.Forms;

namespace Prectice_Interview.Form
{
    public partial class frmSignup : System.Windows.Forms.Form
    {
        private readonly GenaricService _service;
        public frmSignup(GenaricService service)
        {
            InitializeComponent();
            _service = service;
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {

                var getUser = await _service.GetAllAsync<Login>();
                if(!getUser.Any())
                {
                   Login payload = new Login
                   {
                       Email = txtEmail.Text,
                       Password = txtPassword.Text
                   };
                    await _service.AddAsync<Login>(payload);
                }
                if (txtEmail.Text == "" || txtPassword.Text == "")
                {
                    MessageBox.Show("Please enter email and password.");
                    return;
                }
                var user = getUser.FirstOrDefault(x => x.Email == txtEmail.Text && x.Password == txtPassword.Text);
                if (user == null)
                {
                    MessageBox.Show("Invalid email or password.");
                    return;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}