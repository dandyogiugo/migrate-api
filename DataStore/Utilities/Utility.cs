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
using System.Web.ModelBinding;

namespace DataStore.Utilities
{
    public class Utility
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private store<PremiumCalculatorMapping> _premium_map = null;
        private store<Token> _otp = null;
        public Utility()
        {
            _apiconfig = new store<ApiConfiguration>();
            _premium_map = new store<PremiumCalculatorMapping>();
            _otp = new store<Token>();
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
        public async Task<string> Sha256(string pattern)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(pattern));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        public async Task<string> Sha512(string pattern)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA512 hash = SHA512.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(pattern));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
        public async Task<bool> ValidateGTBankUsers(string userhash, string computedhash)
        {
            if (userhash.ToUpper().Equals(computedhash.ToUpper()))
            {
                log.Info("valid hash for GTB users");
                return true;
            }
            else
            {
                log.Info("Invalid hash for GTB users");
                return false;
            }
        }
        public async Task<bool> ValidateHash2(string pattern, string secret, string _hash)
        {
            log.Info($"Passed hash {_hash.ToUpper()}");
            log.Info($"my hash partten {pattern + secret}");
            bool res = false;
            StringBuilder Sb = new StringBuilder();
            using (MD5 hash = MD5.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(pattern + secret));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            log.Info($"Computed hash {Sb.ToString().ToUpper()}");
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
        public string base64Decode(string data)
        {
            try
            {
                UTF8Encoding encoder = new UTF8Encoding();
                Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            catch (Exception e)
            {
                return null;
                //throw new Exception("Error in base64Decode" + e.Message);
            }
        }
        public async Task<bool> ValidateHeaders(string sentHeader, string merchant_id)
        {
            var apiconfig = new store<ApiConfiguration>();
            var getconfig = await apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim() && x.is_active == true);
            if (getconfig == null || string.IsNullOrEmpty(getconfig.assigned_function))
            {
                return false;
            }
            var formhash = await Sha256(getconfig.secret_key + getconfig.merchant_id);
            var formbase64headers = base64Decode(sentHeader);
            if (formhash.ToUpper().Equals(formbase64headers.ToUpper()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<req_response> GenerateCertificate(GenerateCert cert)
        {
            req_response response = new req_response();
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
        public async Task<req_response> GenerateCertificateOneOff(GenerateCert cert)
        {
            req_response response = new req_response();
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
        public async Task<req_response> Validator(string method_name, string merchant_id, string _category, decimal value_of_goods, string hash)
        {
            try
            {
                var check_user_function = await CheckForAssignedFunction(method_name, merchant_id);
                if (!check_user_function)
                {
                    return new req_response
                    {
                        status = 406,
                        message = "Access denied. user not configured for this service "
                    };
                }
                Category category;
                if (!Enum.TryParse(_category, out category))
                {
                    return new req_response
                    {
                        status = 401,
                        message = "Invalid category type, kindly check the api  documentation for the category types"
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    return new req_response
                    {
                        status = 405,
                        message = "Invalid merchant Id"
                    };
                }
                var can_proceed = await ValidateHash(value_of_goods, config.secret_key, hash);
                if (!can_proceed)
                {
                    return new req_response
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
                    sb.Replace("#FOOTER#", "");
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

                    var divisionn_obj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DivisionEmail>>(System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/json.json")));
                    var div_email = divisionn_obj.FirstOrDefault(x => x.Code.ToUpper() == "Life").Email;
                    if (!string.IsNullOrEmpty(div_email))
                    {
                        emailaddress = div_email;
                        cc.Add(list[0]);
                    }
                    var send = new SendEmail().Send_Email(emailaddress, "Claim Request", sb.ToString(), "Claims Request", true, image_path, cc, null, null);
                }
                else
                {

                    string msg_1 = $@"Your claim has been submitted successfully. Your claim number is <strong>{mail.claim_number}</strong>. 
                                You can check your claim status on our website or call(+234)12774000 - 9";
                    sb.Replace("#CONTENT#", msg_1);
                    sb.Replace("#FOOTER#", "");
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
        public void SendMail(ViewModelNonLife mail, bool IsCustodian, string template, string imagepath, string divisionemail = "")
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
                    sb.Replace("#FOOTER#", "");
                    string msg_1 = @"Dear Team,<br/><br/>A claim has been logged successfully and require your attention for further processing";
                    sb.Replace("#CONTENT#", msg_1);
                    var email = ConfigurationManager.AppSettings["Notification"];
                    var list = email.Split('|');
                    string emailaddress = "";
                    List<string> cc = new List<string>();
                    if (list.Count() > 1)
                    {
                        int i = 0;
                        if (!string.IsNullOrEmpty(divisionemail))
                        {
                            emailaddress = divisionemail;
                        }
                        else
                        {
                            emailaddress = list[0];
                        }

                        foreach (var item in list)
                        {
                            if (!string.IsNullOrEmpty(divisionemail))
                            {
                                cc.Add(item);
                                ++i;
                            }
                            else
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
                    }
                    else
                    {
                        //emailaddress = list[0];
                        if (!string.IsNullOrEmpty(divisionemail))
                        {
                            emailaddress = divisionemail;
                            cc.Add(list[0]);
                        }
                        else
                        {
                            emailaddress = list[0];
                        }
                    }
                    var send = new SendEmail().Send_Email(emailaddress, "Claim Request", sb.ToString(), "Claims Request", true, image_path, cc, null, null);
                }
                else
                {
                    sb.Replace("#FOOTER#", @"Please visit our website to confirm the status of your claim.<br /><br />
                                    If you did not initiate this process, please contact us on (+234)12774008-9 or carecentre@custodianinsurance.com");
                    string msg_1 = @"Dear Valued Customer,<br/><br/>Your claim with the below details has been submitted successfully.";
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
        public async Task<claims_details> GetLifeClaimsDetails(ClaimsDetails claim_detail)
        {
            try
            {
                log.Info($"raw response from portal {Newtonsoft.Json.JsonConvert.SerializeObject(claim_detail)}");
                string ConnectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;
                using (OracleConnection cn = new OracleConnection(ConnectionString))
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = cn;
                    cn.Open();
                    cmd.CommandText = "cust_max_mgt.get_claim_policy_info";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_policy_no", OracleDbType.Varchar2).Value = claim_detail.p_policy_no;
                    cmd.Parameters.Add("p_type", OracleDbType.Varchar2).Value = claim_detail.p_type;
                    cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_claim_type", OracleDbType.Varchar2).Value = claim_detail.p_claim_type;
                    cmd.ExecuteNonQuery();
                    string response = cmd.Parameters["v_data"].Value.ToString();
                    log.Info($"response from turnquest claims status {response}");
                    if (string.IsNullOrEmpty(response))
                    {
                        return null;
                    }
                    var transpose = Newtonsoft.Json.JsonConvert.DeserializeObject<claims_details>(response);
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
        public async Task<string> Transposer(string frequency)
        {
            string frq = "";
            if (frequency.ToLower() == "annually")
            {
                frq = "A";
            }
            else if (frequency.ToLower() == "quarterly")
            {
                frq = "Q";
            }
            else if (frequency.ToLower() == "bi-annually")
            {
                frq = "S";
            }
            else if (frequency.ToLower() == "monthly")
            {
                frq = "M";
            }
            else
            {
                frq = "F";
            }

            return frq;
        }
        public async Task<string> GenerateOTP(bool isSms, string toaddress, string fullname, Platforms source)
        {
            try
            {
                var old_otp = await _otp.FindOneByCriteria(x => x.is_used == false && x.is_valid == true && (x.mobile_number == toaddress || x.email == toaddress));
                if (old_otp != null)
                {
                    log.Info($"deactivating previous un-used otp for mobile: {toaddress}");
                    old_otp.is_used = true;
                    old_otp.is_valid = false;
                    await _otp.Update(old_otp);
                }
                log.Info($"creating new opt for user: {toaddress}");
                var new_otp = new Token
                {
                    datecreated = DateTime.Now,
                    fullname = fullname,
                    is_used = false,
                    is_valid = true,
                    mobile_number = (isSms) ? toaddress : null,
                    platform = source,
                    email = (!isSms) ? toaddress : null,
                    otp = Security.Transactions.UID.Codes.TransactionCodes.GenTransactionCodes(6)
                };
                await _otp.Save(new_otp);

                return new_otp.otp;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return null;
            }
        }
        public async Task<bool> ValidateOTP(string token, string emailorphone)
        {
            try
            {
                var validate = await _otp.FindOneByCriteria(x => x.is_used == false && x.is_valid == true && (x.mobile_number == emailorphone || x.email == emailorphone) && x.otp == token);
                if (validate != null)
                {
                    log.Info($"you have provided an valid otp {emailorphone}");
                    validate.is_used = true;
                    validate.is_valid = false;
                    await _otp.Update(validate);
                    return true;
                }
                else
                {
                    log.Info($"you have provided an invalid otp {emailorphone}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return false;
            }
        }
 
    }


    public static class Config
    {
        public const string DEFAULT_BASE_URL = "https://api-football-v1.p.rapidapi.com/v2";
        public static string BASE_URL
        {
            get
            {
                string base_url = ConfigurationManager.AppSettings["BASE_URL"];
                if (!string.IsNullOrEmpty(base_url?.Trim()))
                {
                    if (!base_url.Trim().EndsWith("/"))
                    {
                        return base_url;
                    }
                    else
                    {
                        return base_url.Remove(base_url.Length - 1, 1);
                    }
                }
                else
                {
                    return DEFAULT_BASE_URL;
                }
            }
        }

        public static string Authorization_Header
        {
            get
            {
                string header = ConfigurationManager.AppSettings["AUTH_HEADER"];
                if (!string.IsNullOrEmpty(header))
                {
                    return header;
                }
                else
                {
                    return null;
                }

            }
        }


        public static int GetID
        {
            get
            {
                string Id = ConfigurationManager.AppSettings["LeagueID"];
                if (!string.IsNullOrEmpty(Id))
                {
                    return Convert.ToInt32(Id);
                }
                else
                {
                    return 3;
                }
            }
        }
    }
}
