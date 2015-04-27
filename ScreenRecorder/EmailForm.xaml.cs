/*#################################################################################################
*Project : ScreenRecorder
*Developped by : Daniel de Carvalho Fernandes, Michael Caraccio & Khaled Chabbou
*Date : 27 April 2015
*#################################################################################################*/

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
        // Picture of the name
        private string _pictureName;

        /// <summary>
        /// Constructor that takes a picture name
        /// </summary>
        /// <param name="pictureName"></param>
        public EmailForm(string pictureName)
        {
            _pictureName = pictureName;
            InitializeComponent();

            //Error or succeed set to hidden
            LblErrorYourEmail.Visibility = Visibility.Hidden;
            LblErrorYourPassword.Visibility = Visibility.Hidden;
            LblErrorEmailDestination.Visibility = Visibility.Hidden;
            LblEmailSucceed.Visibility = Visibility.Hidden;
            LblErrorEmailBody.Visibility = Visibility.Hidden;
            LblErrorEmailSubject.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// Button to send the email, and get some validations of the textboxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            bool emailSenderValid = false;
            bool passwordValid = false;
            bool emailDestValid = false;
            bool emailSubjectValid = false;
            bool emailBodyValid = false;

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
                LblErrorEmailDestination.Content = "Destination email is not a valid email : DestinationEmail@gmail.com";
                LblErrorEmailDestination.Visibility = Visibility.Visible;
            }
            else
            {
                LblErrorEmailDestination.Content = null;
                LblErrorEmailDestination.Visibility = Visibility.Hidden;
                emailDestValid = true;
            }

            // Subject Field Validation
            if (TxtSubjectEmail.Text == "")
            {
                LblErrorEmailSubject.Content = "Your Subject is empty";
                LblErrorEmailSubject.Visibility = Visibility.Visible;
            }
            else
            {
                LblErrorEmailSubject.Content = null;
                LblErrorEmailSubject.Visibility = Visibility.Hidden;
                emailSubjectValid = true;
            }

            // Body Field Validation
            if (TxtBodyEmail.Text == "")
            {
                LblErrorEmailBody.Content = "Your Body is empty";
                LblErrorEmailBody.Visibility = Visibility.Visible;
            }
            else
            {
                LblErrorEmailBody.Content = null;
                LblErrorEmailBody.Visibility = Visibility.Hidden;
                emailBodyValid = true;
            }

 
            // Every fields are valid, let's try to send the email
            if (emailDestValid && emailSenderValid && passwordValid && emailBodyValid && emailSubjectValid)
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
                    mM.Subject = TxtSubjectEmail.Text;
                    //deciding for the attachment
                    mM.Attachments.Add(new Attachment(_pictureName));
                    //add the body of the email
                    mM.Body = TxtBodyEmail.Text;
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

                    //Succeed set to visible
                    LblEmailSucceed.Visibility = Visibility.Visible;
                    LblErrorYourEmail.Visibility = Visibility.Hidden;
                    LblErrorYourPassword.Visibility = Visibility.Hidden;
                    LblErrorEmailDestination.Visibility = Visibility.Hidden;
                    LblErrorEmailBody.Visibility = Visibility.Hidden;
                    LblErrorEmailSubject.Visibility = Visibility.Hidden;
                    LblEmailSucceed.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {

                        LblEmailSucceed.UpdateLayout();
                    }));

                    Thread.Sleep(2000);
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
                    LblErrorEmailBody.Visibility = Visibility.Visible;
                    LblErrorEmailSubject.Visibility = Visibility.Visible;
                } //end of catch
            }
        }

        /// <summary>
        /// Click the button return to quit the current window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQuitSendEmail_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
