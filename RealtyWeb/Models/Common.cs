using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using RealtyWeb.Models;
using System.Reflection;

namespace RealtyWeb.Common
{
    public class Common
    {
        public static void SendClientInquiryEmail(string message, AppUser clientInfo)
        {
            MailMessage m = new MailMessage();
            SmtpClient sc = new SmtpClient();
            m.From = new MailAddress(ConfigurationManager.AppSettings["adminAccount"]);
            m.To.Add(m.From);
            m.Subject = "[Client Inquiry] - Name: " + clientInfo.FirstName;
            m.Body = string.Format("\nName: {0}\nEmail: {1}\nContact No: {2}\nMessage: {3}\n\n\nThis is an auto-generated email. Please do not reply.", clientInfo.FirstName, clientInfo.Email, clientInfo.PhoneNumber, message);
            sc.Host = ConfigurationManager.AppSettings["SMTPHost"];
            try
            {
                sc.Port = 25;
                sc.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["adminAccount"],
                                                                  ConfigurationManager.AppSettings["adminPassword"]);
                sc.EnableSsl = false;
                sc.Send(m);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SendPropertyClientInquiryEmail(string message, int webRefNo, AppUser clientInfo, AppUser agentInfo)
        {
            MailMessage m = new MailMessage();
            SmtpClient sc = new SmtpClient();
            m.From = new MailAddress(ConfigurationManager.AppSettings["adminAccount"]);
            m.To.Add(agentInfo.Email);
            m.Bcc.Add(m.From);
            m.Subject = "[Property Client Inquiry] - Web Ref: " + webRefNo.ToString();
            m.Body = string.Format("\nHi {0},\n\nPlease refer below property inquiry from client.\n\nProperty Web Ref #: {1}\n\nName: {2}\nEmail: {3}\nContact No: {4}\nMessage: {5}\n\n\nThis is an auto-generated email. Please do not reply.", agentInfo.FirstName, webRefNo, clientInfo.FirstName, clientInfo.Email, clientInfo.PhoneNumber, message);

            sc.Host = ConfigurationManager.AppSettings["SMTPHost"];
            try
            {
                sc.Port = 25;
                sc.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["adminAccount"],
                                                                  ConfigurationManager.AppSettings["adminPassword"]);
                sc.EnableSsl = false;
                sc.Send(m);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SendEmailToSupport(string body, string destination, string subject)
        {
            MailMessage m = new MailMessage();
            SmtpClient sc = new SmtpClient();
            m.From = new MailAddress(ConfigurationManager.AppSettings["supportAccount"]);
            m.To.Add(destination);
            m.Subject = subject;
            m.Body = body;
            sc.Host = ConfigurationManager.AppSettings["SMTPHost"];
            try
            {
                sc.Port = 25;
                sc.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["supportAccount"],
                                                                  ConfigurationManager.AppSettings["supportPassword"]);
                sc.EnableSsl = false;
                sc.Send(m);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public static class MyExtensions
    {
        public static string GenerateTitleForURL(this PropertyEntry entry)
        {
            string title = string.Empty;

            if ((entry.Bedrooms ?? 0) > 0)
            {
                title = title + entry.Bedrooms.ToString() + "-Bedroom-";
            }

            if (!string.IsNullOrEmpty(entry.Property.PropertyName))
            {
                title = title + entry.Property.PropertyName.Replace(' ', '-') + "-";
            }

            if (!string.IsNullOrEmpty(entry.PropertyIndicator.PropIndicatorName))
            {
                title = title + entry.PropertyIndicator.PropIndicatorName.Replace(' ', '-') + "-";
            }

            if (!string.IsNullOrEmpty(entry.Address3))
            {
                title = title + "in-" + entry.Address3.TrimEnd().Replace(' ', '-') + "-";
            }

            title = title + entry.PropertyEntryId.ToString();

            return title.ToLower();
        }

        public static string GenerateTitle(this PropertyEntry entry)
        {
            string title = string.Empty;

            if ((entry.Bedrooms ?? 0) > 0)
            {
                title = title + entry.Bedrooms.ToString() + " Bedroom";
            }

            if (!string.IsNullOrEmpty(entry.Property.PropertyName))
            {
                title = title + " " + entry.Property.PropertyName;
            }

            if (!string.IsNullOrEmpty(entry.PropertyIndicator.PropIndicatorName))
            {
                title = title + " " + entry.PropertyIndicator.PropIndicatorName;
            }

            if (!string.IsNullOrEmpty(entry.Address3))
            {
                title = title + " in " + entry.Address3.TrimEnd();
            }

            return title.ToLower();
        }
    }   

    public enum Menu
    {
        None,
        Home,
        About,
        Main,
        ForSale,
        ForRent,
        Development,
        Foreclosed,
        Realty,
        Login,
        Manage
    }

    public enum PropIndicator
    {
        Unknown = 0,
        [EnumValue("For Sale")]
        ForSale,
        [EnumValue("For Rent")]
        ForRent,
        [EnumValue("Development")]
        Development,
        [EnumValue("Foreclosed")]
        Foreclosed
    }

    public class ListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class EnumValue : System.Attribute
    {
        private string _value;
        public EnumValue(string value)
        {
            _value = value;
        }
        public string Value
        {
            get { return _value; }
        }
    }

    public static class EnumString
    {
        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            EnumValue[] attrs = fi.GetCustomAttributes(typeof(EnumValue), false) as EnumValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }
    }
}