
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Myshop.Models;
using System.Transactions;
using System.IO;

namespace Myshop.Controllers
{
    public class AccountController : Controller
    {
        public string authsecret = "secret1234";
        // GET: Account
        public ActionResult Register()
        {
            if (Session["user"] == null)
            {
                return View();
            }
            else{
                return Redirect("/Home/Index");

            }
        }

        [HttpPost]
        public ActionResult Register(Register users)

        {
            MyDatabaseDataContext db = new MyDatabaseDataContext();
            int count = db.V_users.Where(x => x.Email == users.Email).Count();
            if (String.IsNullOrEmpty(users.Name) || String.IsNullOrEmpty(users.Surname) || String.IsNullOrEmpty(users.Email) || String.IsNullOrEmpty(users.Password) || String.IsNullOrEmpty(users.ConfirmPassword))
            {
                ViewBag.error = "შეავსეთ ყველა ველი!";
                return View();

            }

            else if (users.ConfirmPassword != users.Password)
            {
                ViewBag.error = "პაროლები არ მთხვევა ერთმანეთს";
                return View();
            }
            else if (count > 0)
            {
                
                ViewBag.error = "მომხამრებელი ასეთი ელ.ფოსტით უკვე დარეგისტრირებულია";
                return View();
            }
            else
            {
                string confirmationcode = Random32();
                V_Unuser unuser = new V_Unuser()
                {
                    
                    Name = users.Name,
                    SurName = users.Surname,
                    Email = users.Email,
                    Password = MD5Hash(users.Password + authsecret),
                    CreateDate = DateTime.Now,
                    ConfCode = confirmationcode,
                };
                db.V_Unusers.InsertOnSubmit(unuser);
                db.SubmitChanges();
                string body = "http://localhost:51328/Account/Confirmation/"+confirmationcode;
                SendMail(users.Email, "Confirmation",body,false);
                return RedirectToAction("conf");
            }

        }
        public ActionResult Confirmation (string id)
        {
            MyDatabaseDataContext db = new MyDatabaseDataContext();
            V_Unuser unuser = db.V_Unusers.Where(x => x.ConfCode == id).FirstOrDefault();

            V_user user = new V_user()
            {
                Name =unuser.Name,
                SurName = unuser.SurName,
                CreateDate = DateTime.Now,
                Password = unuser.Password,
                Email = unuser.Email,
                Secret = Random32(),

            };
            using (var transaction = new TransactionScope())
            {
                db.V_users.InsertOnSubmit(user);
                db.SubmitChanges();

                db.V_Unusers.DeleteAllOnSubmit(db.V_Unusers.Where(x => x.Email == unuser.Email).ToList());
                db.SubmitChanges();
                transaction.Complete();
            }

            return RedirectToAction("Success");
        }
        public ActionResult conf()
        {
            return View();
        }


        public ActionResult Login()
        {
            return View();

        }
        [HttpPost]
        public ActionResult Login(Login login)
        {
            if(String.IsNullOrEmpty(login.Email)   || String.IsNullOrEmpty(login.Password))
            {
                ViewBag.error = "შეავსეთ ყველა ველი";
                return View();
            }

            MyDatabaseDataContext db = new MyDatabaseDataContext();
          int count =   db.V_users.Where(x => x.Email == login.Email && x.Password == MD5Hash(login.Password + authsecret)).Count();
            if(count > 0 )
            {
                
                string  name = db.V_users.Where(x => x.Email == login.Email).Select(x => x.Name).FirstOrDefault();
                int id = db.V_users.Where(x => x.Email == login.Email).Select(x => x.Id).FirstOrDefault();
                Session["user"] = name;
                Session["id"] = id;
                return RedirectToAction("/Loginsuccess");
             
            }
            else
            {
                ViewBag.error = "ასეთი მომხამრებელი არ არსებობს";
                return View();
            }
            
        }
        public ActionResult Loginsuccess()
        {

            return View();

        }
        public ActionResult success()
        {
           
            return View();

        }
        public static string MD5Hash(string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            string md5 = "";
            for (int i = 0; i < data.Length; i++)
            {
                md5 += data[i].ToString("x2");
            }
            return md5;
        }
        public static string Random32()
        {
            return Guid.NewGuid().ToString("N");
        }
        public ActionResult list()
        {
            
                MyDatabaseDataContext db = new MyDatabaseDataContext();


                return View(db.V_Unusers.ToList());

        }

        public ActionResult LogOut()
        {
            Session["user"] = null;
            Session["id"] = null;
            Session.Clear();
            return Redirect("/Home/Index");

        }
        public static bool SendMail(string userMail, string subject, string body, bool isHtml = false)
        {
            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.IsBodyHtml = isHtml;
                mail.From = new System.Net.Mail.MailAddress("Geolab@gmail.com");
                mail.To.Add(userMail);
                mail.Subject = subject;
                mail.Body = body;

                System.Net.Mail.SmtpClient SmtpServer = new System.Net.Mail.SmtpClient("smtp.mandrillapp.com");
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("myshoptest123@gmail.com", "oI7J2rMzdEgam8veOZ6rCA");
                SmtpServer.EnableSsl = false;

                SmtpServer.Send(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public ActionResult Forgot()
        {


            return View();

        }
        [HttpPost]
        public ActionResult forgot (Forgot f)
        {
            MyDatabaseDataContext db = new MyDatabaseDataContext();
            V_user us = db.V_users.Where(x => x.Email == f.Email).FirstOrDefault();
            if (us == null)
            {
                ViewBag.error = "ასეთი მომხმარებელი არ არსებობს";
                return View();

            }

            string body = "http://localhost:51328/Account/Recovery/" + us.Secret;
            SendMail(us.Email, "Recovery", body, false);
            ViewBag.error = "მეილი წარმატებით გაიგზავნა";

            return View();
        }
        public ActionResult Recovery (string id)
        {
            MyDatabaseDataContext db = new MyDatabaseDataContext();
            int userid = db.V_users.Where(x => x.Secret == id).Select(x => x.Id).FirstOrDefault();
            Session["userid"] = userid;



            return View();
        }
        [HttpPost]
        public ActionResult Recovery (Password pass)
        {
            if(String.IsNullOrEmpty(pass.FirsPassword)|| String.IsNullOrEmpty(pass.SecondPassword))
            {
                ViewBag.error = "შეავსეთ ყველა ველი";
                return View();
            }
            else if(pass.FirsPassword != pass.SecondPassword)
            {
                ViewBag.error = "პაროლები არ ემთხვევა ერთმანეთს";
                return View();
            }
            MyDatabaseDataContext db = new MyDatabaseDataContext();
            int id = Convert.ToInt32(Session["userid"]);
            V_user u = db.V_users.Where(x => x.Id == id).First();
            u.Password = MD5Hash(pass.FirsPassword + authsecret);
            db.SubmitChanges();


            Session["text"] = "პაროლი შეიცვალა წარმატებით გთხოვთ შეხვიდეთ სისტემაში";

            return RedirectToAction("Login");

        }
        [HttpPost]
       public ActionResult AddProduct(V_Product v,HttpPostedFileBase Product_image)
        {
          
            string path = HttpContext.Server.MapPath("/Content/Images/");
            string ext = System.IO.Path.GetExtension(Product_image.FileName);
            string name = Guid.NewGuid().ToString("N");
           
            string imagesrc = "/Content/Images/" + name + ext;
            Product_image.SaveAs(path + name + ext);

            V_Product pr = new V_Product()
            {
                Product_name = v.Product_name,
                Product_description = v.Product_description,
                Price = v.Price,
                User_Id = Convert.ToInt32(Session["id"]),
                Product_image = imagesrc,



            };
            MyDatabaseDataContext db = new MyDatabaseDataContext();
            db.V_Products.InsertOnSubmit(pr);
            db.SubmitChanges();

           
            

            return View();
        }
       
        public ActionResult AddProduct()
        {

          
            return View();
        }


        public ActionResult Products()
        {
            MyDatabaseDataContext db = new MyDatabaseDataContext();
            

                return View(db.V_Products.ToList());
        }

        public ActionResult shop()
        {
            MyDatabaseDataContext db = new MyDatabaseDataContext();


            return View(db.V_Products.ToList());
        }
    }

}