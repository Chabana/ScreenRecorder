using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Mail;
using System.Net.Mime;
using System.Security;
using System.Threading;

namespace ScreenRecorder
{
    /// <summary>
    /// Logique d'interaction pour EmailForm.xaml
    /// </summary>
    public partial class EmailForm : Window
    {
        private string pictureName;
        public EmailForm(string pictureName)
        {
            this.pictureName = pictureName;
            InitializeComponent();


            

            this.lblErrorYourEmail.Visibility = Visibility.Hidden;
            this.lblErrorYourPassword.Visibility = Visibility.Hidden;
            this.lblErrorEmailDestination.Visibility = Visibility.Hidden;
            this.lblEmailSucceed.Visibility = Visibility.Hidden;
        }



        private void btnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            if (!txtYourEmail.Text.Contains("@gmail.com"))
            {
                
            }

            try
            {
                //Mail Message
                MailMessage mM = new MailMessage();
                //Mail Address
                mM.From = new MailAddress(txtYourEmail.Text);
                //receiver email id
                mM.To.Add(txtEmailDestination.Text);
                //subject of the email
                mM.Subject = "your subject line will go here";
                //deciding for the attachment
                mM.Attachments.Add(new Attachment(pictureName));
                //add the body of the email
                mM.Body = "Body of the email";
                mM.IsBodyHtml = true;
                //SMTP client
                SmtpClient sC = new SmtpClient("smtp.gmail.com");
                //port number for Gmail mail
                sC.Port = 587;
                //credentials to login in to Gmail account
                sC.Credentials = new NetworkCredential(txtYourEmail.Text, passwordBoxYourPassword.Password);
                //enabled SSL
                sC.EnableSsl = true;
                //Send an email
                sC.Send(mM);
                this.lblEmailSucceed.Visibility = Visibility.Visible;
                this.lblErrorYourEmail.Visibility = Visibility.Hidden;
                this.lblErrorYourPassword.Visibility = Visibility.Hidden;
                this.lblErrorEmailDestination.Visibility = Visibility.Hidden;
                this.Close();

            }//end of try block
            catch (Exception ex)
            {
                this.lblErrorYourEmail.Visibility = Visibility.Visible;
                this.lblErrorYourPassword.Visibility = Visibility.Visible;
                this.lblErrorEmailDestination.Visibility = Visibility.Visible;
                this.lblEmailSucceed.Visibility = Visibility.Hidden;

            }//end of catch

            
        }




    }
}
