using DataStore.Irepository;
using DataStore.Models;
using DataStore.repository;
using DataStore.ViewModels;
using Word = Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NLog;
using Spire.Doc;
using CustodianEmailSMSGateway.Email;
using System.Configuration;
using Oracle.DataAccess.Client;
using System.Data;

namespace DataStore.Utilities
{
    public class Utility
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private store<PremiumCalculatorMapping> _premium_map = null;
        public Utility()
        {
            _apiconfig = new store<ApiConfiguration>();
            _premium_map = new store<PremiumCalculatorMapping>();
        }
        public async Task<bool> ValidateHash(decimal value_of_goods, string secret, string _hash)
        {
            bool res = false;
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(Convert.ToInt32(value_of_goods) + secret));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            if (Sb.ToString().ToUpper().Equals(_hash.ToUpper()))
            {
                res = true;
            }
            return res;
        }
        public async Task<bool> CheckForAssignedFunction(string methodName, string merchant_id)
        {
            var apiconfig = new store<ApiConfiguration>();
            var getconfig = await apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim() && x.is_active == true);
            if (getconfig == null || string.IsNullOrEmpty(getconfig.assigned_function))
            {
                return false;
            }
            if (!getconfig.assigned_function.Split('|').Any(x => x.Trim().ToLower().Equals(methodName.ToLower())))
            {
                return false;
            }
            else
            {
                var method_status = new store<ApiMethods>().FindOneByCriteria(x => x.method_name.Trim().ToLower().Equals(methodName.Trim().ToLower()) && x.is_active == true);
                if (method_status != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        public async Task<response> GenerateCertificate(GenerateCert cert)
        {
            response response = new response();
            try
            {

                Document doc = new Document();
                string newrtfPath = HttpContext.Current.Server.MapPath("~/Cert/template.rtf");
                doc.LoadFromFile(newrtfPath, FileFormat.Rtf);
                doc.Replace("#POLICY_NO#", cert.policy_no, false, true);
                doc.Replace("#NAME#", cert.name, false, true);
                doc.Replace("#ADDRESS#", cert.address, false, true);
                doc.Replace("#FROM_DATE#", cert.from_date, false, true);
                //doc.Replace("#TO_DATE#", cert.to_date, false, true);
                doc.Replace("#VREG_NO#", cert.vehicle_reg_no, false, true);
                doc.Replace("#INTEREST#", cert.interest.Trim(), false, true);
                doc.Replace("#VALUE_OF_GOODS#", string.Format("N{0:N}", Convert.ToDecimal(cert.value_of_goods)), false, true);
                doc.Replace("#FROM_LOC#", cert.from_location, false, true);
                doc.Replace("#PREMIUM#", string.Format("N{0:N}", Convert.ToDecimal(cert.premium)), false, true);
                doc.Replace("#TO_LOC#", cert.to_location, false, true);//#SERIAL_NUMBER#
                doc.Replace("#SERIAL_NUMBER#", cert.serial_number, false, true);
                string pdfPath = HttpContext.Current.Server.MapPath($"~/Cert/GeneratedCert/{cert.serial_number}.pdf");
                if (System.IO.File.Exists(pdfPath))
                {
                    System.IO.File.Delete(pdfPath);
                    doc.SaveToFile(pdfPath, FileFormat.PDF);
                }
                else
                {
                    doc.SaveToFile(pdfPath, FileFormat.PDF);
                }

                cert.cert_path = pdfPath;
                //dont wait for certificate to send continue process and let ths OS thread do the sending without slowing down the application
                var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Notification.html"));
                string imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                Task.Factory.StartNew(() =>
                {
                    this.SendEmail(cert, template, imagepath);
                });

                response.status = 200;
                response.message = $"Insurance purchase was successful, a copy of your insurance document has been sent to this ({cert.email_address}) email";
                response.policy_details = new policy_details
                {
                    certificate_no = cert.serial_number,
                    policy_number = cert.policy_no,
                    download_link = $"https://api.custodianplc.com.ng/CustodianApiv2.0/download/{cert.serial_number}"
                };

            }
            catch (Exception ex)
            {
                response.status = 401;
                response.message = $"System malfunction";
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }

            return response;
        }
        public async Task<response> GenerateCertificateOneOff(GenerateCert cert)
        {
            response response = new response();
            try
            {

                Document doc = new Document();
                string newrtfPath = HttpContext.Current.Server.MapPath("~/Cert/templateOneoff.rtf");
                doc.LoadFromFile(newrtfPath, FileFormat.Rtf);
                doc.Replace("#POLICY_NO#", cert.policy_no, false, true);
                doc.Replace("#NAME#", cert.name, false, true);
                doc.Replace("#ADDRESS#", cert.address, false, true);
                doc.Replace("#FROM_DATE#", cert.from_date, false, true);
                doc.Replace("#TO_DATE#", cert.to_date, false, true);
                doc.Replace("#VREG_NO#", cert.vehicle_reg_no, false, true);
                doc.Replace("#SERIAL_NUMBER#", cert.serial_number, false, true);
                string pdfPath = HttpContext.Current.Server.MapPath($"~/Cert/GeneratedCert/{cert.serial_number}_NB.pdf");
                if (System.IO.File.Exists(pdfPath))
                {
                    System.IO.File.Delete(pdfPath);
                    doc.SaveToFile(pdfPath, FileFormat.PDF);
                }
                else
                {
                    doc.SaveToFile(pdfPath, FileFormat.PDF);
                }

                cert.cert_path = pdfPath;
                //dont wait for certificate to send continue process and let ths OS thread do the sending without slowing down the application
                var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Notification.html"));
                string imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                Task.Factory.StartNew(() =>
                {
                    this.SendEmail(cert, template, imagepath);
                });

                response.status = 200;
                response.message = $"Insurance purchase was successful, a copy of your insurance document has been sent to this ({cert.email_address}) email";
                response.policy_details = new policy_details
                {
                    certificate_no = cert.serial_number,
                    policy_number = cert.policy_no,
                };

            }
            catch (Exception ex)
            {
                response.status = 401;
                response.message = $"System malfunction";
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }

            return response;
        }
        public async Task<string> GetSerialNumber()
        {
            Guid serialGuid = Guid.NewGuid();
            string uniqueSerial = serialGuid.ToString("N");
            string uniqueSerialLength = uniqueSerial.Substring(0, 28).ToUpper();
            char[] serialArray = uniqueSerialLength.ToCharArray();
            string finalSerialNumber = "";
            int j = 0;
            for (int i = 0; i < 28; i++)
            {
                for (j = i; j < 4 + i; j++)
                {
                    finalSerialNumber += serialArray[j];
                }
                if (j == 28)
                {
                    break;
                }
                else
                {
                    i = (j) - 1;
                    finalSerialNumber += "-";
                }
            }

            return finalSerialNumber;
        }
        public void SendEmail(GenerateCert sendmail, string temp, string Imagepath, string new_biz = "")
        {
            try
            {
                StringBuilder template = new StringBuilder(temp);
                string body = $"Dear {sendmail.name},<br/><br/>Please find attached your GIT Insurance documents.<br/><br/> Thank you.<br/><br/>";
                template.Replace("#CONTENT#", body);
                template.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                List<string> attach = new List<string>();
                attach.Add(sendmail.cert_path);

                var send = new SendEmail().Send_Email(sendmail.email_address, $"{new_biz} GIT Insurance Document", template.ToString(), $"{new_biz}  GIT Insurance Document", true, Imagepath, null, null, attach);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
        }
        public async Task<string> GeneratePolicyNO(int val)
        {
            string final = "";
            if (val <= 9)
            {
                final = "000000" + val;
            }
            else if (val.ToString().Length < 7)
            {
                int loop = 7 - val.ToString().Length;
                string zeros = "";
                for (int i = 0; i < loop; i++)
                {
                    zeros += "0";
                }
                final = zeros + val;
            }
            else
            {
                final = val.ToString();
            }

            return "HO/A/07/T" + final + "E";
        }
        public async Task<response> Validator(string method_name, string merchant_id, string _category, decimal value_of_goods, string hash)
        {
            try
            {
                var check_user_function = await this.CheckForAssignedFunction(method_name, merchant_id);
                if (!check_user_function)
                {
                    return new response
                    {
                        status = 406,
                        message = "Access denied. user not configured for this service "
                    };
                }
                Category category;
                if (!Enum.TryParse(_category, out category))
                {
                    return new response
                    {
                        status = 401,
                        message = "Invalid category type, kindly check the api  documentation for the category types"
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    return new response
                    {
                        status = 405,
                        message = "Invalid merchant Id"
                    };
                }
                var can_proceed = await this.ValidateHash(value_of_goods, config.secret_key, hash);
                if (!can_proceed)
                {
                    return new response
                    {
                        status = 403,
                        message = "Security violation: hash value mismatch"
                    };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
            return null;
        }
        public void SendMail(Life_Claims mail, bool IsCustodian, string template, string imagepath)
        {
            try
            {
                log.Info($"About to send email to {mail.email_address}");
                StringBuilder sb = new StringBuilder(template);
                log.Info($"About to send temp to here");
                sb.Replace("#NAME#", mail.policy_holder_name);
                sb.Replace("#POLICYNUMBER#", mail.policy_number);
                sb.Replace("#POLICYTYPE#", mail.policy_type);
                sb.Replace("#CLAIMSTYPE#", mail.claim_request_type);
                sb.Replace("#EMAILADDRESS#", mail.email_address);
                sb.Replace("#PHONENUMBER#", mail.phone_number);
                sb.Replace("#CLAIMNUMBER#", mail.claim_number);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                log.Info($"About to send param to all");
                var image_path = imagepath;
                if (IsCustodian)
                {
                    string msg_1 = @"Dear Team,<br/> A claim has been logged succesfully and require your attention for further processing";
                    sb.Replace("#CONTENT#", msg_1);
                    var email = ConfigurationManager.AppSettings["Notification"];
                    var list = email.Split('|');
                    string emailaddress = "";
                    List<string> cc = new List<string>();
                    if (list.Count() > 1)
                    {
                        int i = 0;
                        emailaddress = list[0];
                        foreach (var item in list)
                        {
                            if (i == 0)
                            {
                                ++i;
                                continue;
                            }
                            else
                            {
                                cc.Add(item);
                                ++i;
                            }


                        }
                    }
                    else
                    {
                        emailaddress = list[0];
                    }
                    var send = new SendEmail().Send_Email(emailaddress, "Claim Request", sb.ToString(), "Claims Request", true, image_path, cc, null, null);
                }
                else
                {

                    string msg_1 = $@"Your claim has been submitted successfully. Your claim number is <strong>{mail.claim_number}</strong>. 
                                You can check your claim status on our website or call(+234)12774000 - 9";
                    sb.Replace("#CONTENT#", msg_1);
                    var send = new SendEmail().Send_Email(mail.email_address, "Claim Request", sb.ToString(), "Claims Request", true, image_path, null, null, null);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
        }
        public void SendMail(ViewModelNonLife mail, bool IsCustodian, string template, string imagepath)
        {
            try
            {
                log.Info($"About to send email to {mail.email_address}");
                StringBuilder sb = new StringBuilder(template);
                log.Info($"About to send temp to here");
                sb.Replace("#NAME#", mail.policy_holder_name);
                sb.Replace("#POLICYNUMBER#", mail.policy_number);
                sb.Replace("#POLICYTYPE#", mail.policy_type);
                sb.Replace("#CLAIMSTYPE#", mail.claim_request_type);
                sb.Replace("#EMAILADDRESS#", mail.email_address);
                sb.Replace("#PHONENUMBER#", mail.phone_number);
                sb.Replace("#CLAIMNUMBER#", mail.claims_number);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                log.Info($"About to send param to all");
                var image_path = imagepath;
                if (IsCustodian)
                {
                    string msg_1 = @"Dear Team,<br/><br/> A claim has been logged succesfully and require your attention for further processing";
                    sb.Replace("#CONTENT#", msg_1);
                    var email = ConfigurationManager.AppSettings["Notification"];
                    var list = email.Split('|');
                    string emailaddress = "";
                    List<string> cc = new List<string>();
                    if (list.Count() > 1)
                    {
                        int i = 0;
                        emailaddress = list[0];
                        foreach (var item in list)
                        {
                            if (i == 0)
                            {
                                ++i;
                                continue;
                            }
                            else
                            {
                                cc.Add(item);
                                ++i;
                            }


                        }
                    }
                    else
                    {
                        emailaddress = list[0];
                    }
                    var send = new SendEmail().Send_Email(emailaddress, "Claim Request", sb.ToString(), "Claims Request", true, image_path, cc, null, null);
                }
                else
                {

                    string msg_1 = $@"Your claim has been submitted successfully. Your claim number is <strong>{mail.claims_number}</strong>. 
                                You can check your claim status on our website or call(+234)12774000 - 9";
                    sb.Replace("#CONTENT#", msg_1);
                    var send = new SendEmail().Send_Email(mail.email_address, "Claim Request", sb.ToString(), "Claims Request", true, image_path, null, null, null);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
        }
        public async Task<string> SendGITMail(GITInsurance git, string status, string merchant_id)
        {
            try
            {
                var path = HttpContext.Current.Server.MapPath("~/Cert/Gitnotification.html");
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                var template = System.IO.File.ReadAllText(path);
                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#NAME#", git.insured_name);
                sb.Replace("#POLICYNUMBER#", git.policy_no);
                sb.Replace("#VALUE_OF_GOODS#", string.Format("N {0:N}", git.value_of_goods));
                sb.Replace("#DESCRIPTION#", git.goods_description);
                sb.Replace("#EMAILADDRESS#", git.email_address);
                sb.Replace("#PHONENUMBER#", git.phone_number);
                sb.Replace("#PREMIUM#", string.Format("N {0:N}", git.premium));
                sb.Replace("#STATUS#", status);
                sb.Replace("#START#", git.from_date.ToShortDateString());

                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                var email = ConfigurationManager.AppSettings["Notification"];
                var list = email.Split('|');
                string emailaddress = "";
                List<string> cc = new List<string>();
                if (list.Count() > 1)
                {
                    int i = 0;
                    emailaddress = list[0];
                    foreach (var item in list)
                    {
                        if (i == 0)
                        {
                            ++i;
                            continue;
                        }
                        else
                        {
                            cc.Add(item);
                            ++i;
                        }

                    }
                }
                else
                {
                    emailaddress = list[0];
                }
                string title = "Custodian GIT Start Trip Notification";
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (status == "NO")
                {
                    sb.Replace("#HEADER#", "Git Start Trip Notification");
                    sb.Replace("#CONTENT#", $"Dear Team,<br/><br/>A customer has bought GIT Insurance from {config.merchant_name}");
                    title = "Custodian GIT Start Trip Notification";
                    sb.Replace("#END#", "---");
                }
                else
                {
                    title = "Custodian GIT End Trip Notification";
                    sb.Replace("#HEADER#", "Git End Trip Notification");
                    sb.Replace("#CONTENT#", $"Dear Team,<br/><br/>A customer has delievered his goods successfully from {config.merchant_name}");
                    sb.Replace("#END#", git.to_date.Value.ToShortDateString());
                }

                Task.Factory.StartNew(() =>
                {
                    var send = new SendEmail().Send_Email(emailaddress, title, sb.ToString(), title, true, imagepath, cc, null, null);
                });
                return "True";
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return null;
            }
        }
        public async Task<string> SendGITMail(GITInsurance git, string merchant_id)
        {
            try
            {
                var path = HttpContext.Current.Server.MapPath("~/Cert/Gitnotification.html");
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                var template = System.IO.File.ReadAllText(path);
                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#NAME#", git.insured_name);
                sb.Replace("#POLICYNUMBER#", git.policy_no);
                sb.Replace("#VALUE_OF_GOODS#", "---");
                sb.Replace("#DESCRIPTION#", git.category);
                sb.Replace("#EMAILADDRESS#", git.email_address);
                sb.Replace("#PHONENUMBER#", git.phone_number);
                sb.Replace("#PREMIUM#", string.Format("N {0:N}", git.premium));
                sb.Replace("#STATUS#", "---");
                sb.Replace("#START#", git.from_date.ToShortDateString());
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                sb.Replace("#END#", git.to_date.Value.ToShortDateString());
                var email = ConfigurationManager.AppSettings["Notification"];
                var list = email.Split('|');
                string emailaddress = "";
                List<string> cc = new List<string>();
                if (list.Count() > 1)
                {
                    int i = 0;
                    emailaddress = list[0];
                    foreach (var item in list)
                    {
                        if (i == 0)
                        {
                            ++i;
                            continue;
                        }
                        else
                        {
                            cc.Add(item);
                            ++i;
                        }

                    }
                }
                else
                {
                    emailaddress = list[0];
                }
                string title = "New Business Custodian GIT Insurance";
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());

                sb.Replace("#HEADER#", " ");
                sb.Replace("#CONTENT#", $"Dear Team,<br/><br/>A customer just bought GIT Insurance from {config.merchant_name}");
                Task.Factory.StartNew(() =>
                {
                    var send = new SendEmail().Send_Email(emailaddress, title, sb.ToString(), title, true, imagepath, cc, null, null);
                });
                return "True";
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return null;
            }
        }
        public async Task<LifeClaimStatus> SubmitLifeClaims(string policy_no, string claim_type)
        {
            try
            {
                string ConnectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;
                using (OracleConnection cn = new OracleConnection(ConnectionString))
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = cn;
                    cn.Open();
                    cmd.CommandText = "cust_max_mgt.create_claim";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_policy_no", OracleDbType.Varchar2).Value = policy_no;
                    cmd.Parameters.Add("p_type_code", OracleDbType.Varchar2).Value = claim_type;
                    cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    string response = cmd.Parameters["v_data"].Value.ToString();
                    log.Info($"response from turnquest {response}");
                    if (string.IsNullOrEmpty(response))
                    {
                        return null;
                    }
                    var transpose = Newtonsoft.Json.JsonConvert.DeserializeObject<LifeClaimStatus>(response);
                    return transpose;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return null;
            }
        }
        public async Task<string> ClaimCode(string claim_type)
        {
            if (claim_type.ToLower().Trim() == "surrender")
            {
                return "SUR";
            }
            else if (claim_type.ToLower().Trim() == "death")
            {
                return "DTH";
            }
            else if (claim_type.ToLower().Trim() == "termination")
            {
                return "TEM";
            }
            else if (claim_type.ToLower().Trim().StartsWith("parmanet"))
            {
                return "DIS";
            }
            else
            {
                return "PROCEED";
            }
        }
        public async Task<LifeClaimStatus> CheckClaimStatus(string claim_number)
        {
            try
            {
                string ConnectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;
                using (OracleConnection cn = new OracleConnection(ConnectionString))
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = cn;
                    cn.Open();
                    cmd.CommandText = "cust_max_mgt.check_claim_status";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_claim_no", OracleDbType.Varchar2).Value = claim_number;
                    cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    string response = cmd.Parameters["v_data"].Value.ToString();
                    log.Info($"response from turnquest claims status {response}");
                    if (string.IsNullOrEmpty(response))
                    {
                        return null;
                    }
                    var transpose = Newtonsoft.Json.JsonConvert.DeserializeObject<LifeClaimStatus>(response);
                    return transpose;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return null;
            }
        }
    }
}
