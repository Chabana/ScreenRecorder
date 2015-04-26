using System;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ScreenRecorder
{
    /// <summary>
    /// Logique d'interaction pour EmailForm.xaml
    /// </summary>
    public partial class EmailForm
    {
        private string _pictureName;
        public EmailForm(string pictureName)
        {
            _pictureName = pictureName;
            InitializeComponent();

            LblErrorYourEmail.Visibility = Visibility.Hidden;
            LblErrorYourPassword.Visibility = Visibility.Hidden;
            LblErrorEmailDestination.Visibility = Visibility.Hidden;
            LblEmailSucceed.Visibility = Visibility.Hidden;
        }



        private void btnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            bool emailSenderValid = false;
            bool passwordValid = false;
            bool emailDestValid = false;

            // Your Email Field Validation
            bool emailSender = Regex.IsMatch(TxtYourEmail.Text.Trim(), @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

            if (TxtYourEmail.Text == "")
            {
                LblErrorYourEmail.Content = "Your email is empty";
                LblErrorYourEmail.Visibility = Visibility.Visible;

            }
            else if (!emailSender)
            {
                LblErrorYourEmail.Content = "Your email is not a valid email : YourEmail@gmail.com";
                LblErrorYourEmail.Visibility = Visibility.Visible;
            }
            else if (!TxtYourEmail.Text.Contains("@gmail.com"))
            {
                LblErrorYourEmail.Content = "Only Gmail account are accepted";
                LblErrorYourEmail.Visibility = Visibility.Visible;
            }
            else
            {
                LblErrorYourEmail.Content = null;
                LblErrorYourEmail.Visibility = Visibility.Hidden;
                emailSenderValid = true;
            }

            // Password Field Validation
            if (PasswordBoxYourPassword.Password == "")
            {
                LblErrorYourPassword.Content = "Your password is empty";
                LblErrorYourPassword.Visibility = Visibility.Visible;
            }
            else
            {
                LblErrorYourPassword.Content = null;
                LblErrorYourPassword.Visibility = Visibility.Hidden;
                passwordValid = true;
            }

            // Destination Email Field Validation
            bool emailDestination = Regex.IsMatch(TxtEmailDestination.Text.Trim(), @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

            if (TxtEmailDestination.Text == "")
            {
                LblErrorEmailDestination.Content = "Destination email is empty";
                LblErrorEmailDestination.Visibility = Visibility.Visible;
            }
            else if (!emailDestination)
            {
                LblErrorEmailDestination.Content = "Destination email is not a valid email : YourEmail@gmail.com";
                LblErrorEmailDestination.Visibility = Visibility.Visible;
            }
            else
            {
                LblErrorEmailDestination.Content = null;
                LblErrorEmailDestination.Visibility = Visibility.Hidden;
                emailDestValid = true;
            }

 
            // Every fields are valid, let's try to send the email
            if (emailDestValid && emailSenderValid && passwordValid)
            {
                try
                {
                    //Mail Message
                    MailMessage mM = new MailMessage();
                    //Mail Address
                    mM.From = new MailAddress(TxtYourEmail.Text);
                    //receiver email id
                    mM.To.Add(TxtEmailDestination.Text);
                    //subject of the email
                    mM.Subject = "New Image for you !";
                    //deciding for the attachment
                    mM.Attachments.Add(new Attachment(_pictureName));
                    //add the body of the email
                    mM.Body = "Here is a new image for you my friend !!!!!";
                    mM.IsBodyHtml = true;
                    //SMTP client
                    SmtpClient sC = new SmtpClient("smtp.gmail.com");
                    //port number for Gmail mail
                    sC.Port = 587;
                    //credentials to login in to Gmail account
                    sC.Credentials = new NetworkCredential(TxtYourEmail.Text, PasswordBoxYourPassword.Password);
                    //enabled SSL
                    sC.EnableSsl = true;
                    //Send an email
                    sC.Send(mM);

                    LblEmailSucceed.Visibility = Visibility.Visible;
                    LblErrorYourEmail.Visibility = Visibility.Hidden;
                    LblErrorYourPassword.Visibility = Visibility.Hidden;
                    LblErrorEmailDestination.Visibility = Visibility.Hidden;
                    LblEmailSucceed.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {

                        LblEmailSucceed.UpdateLayout();
                    }));

                    Thread.Sleep(1500);
                    Close();


                } //end of try block
                catch (Exception)
                {
                    LblErrorEmailDestination.Content = "Email not sent. Maybe a password problem?";
                    LblErrorEmailDestination.Visibility = Visibility.Visible;

                    LblErrorYourEmail.Visibility = Visibility.Visible;
                    LblErrorYourPassword.Visibility = Visibility.Visible;
                    LblErrorEmailDestination.Visibility = Visibility.Visible;
                    LblEmailSucceed.Visibility = Visibility.Hidden;
                } //end of catch
            }
        }

        private void btnQuitSendEmail_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
