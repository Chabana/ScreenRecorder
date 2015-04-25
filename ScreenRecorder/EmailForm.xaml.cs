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
using System.Windows.Threading;

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
                mM.Subject = "New Image for you !";
                //deciding for the attachment
                mM.Attachments.Add(new Attachment(pictureName));
                //add the body of the email
                mM.Body = "Here is a new image for you my friend !!!!!";
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
                lblEmailSucceed.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate()
                {

                    lblEmailSucceed.UpdateLayout();
                }));

                System.Threading.Thread.Sleep(2000);
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

        private void btnQuitSendEmail_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
