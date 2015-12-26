using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Myshop.Models
{
    public class Register
    {
        public String Name { get; set; }
        public String Surname { get; set; }
        public String Password { get; set; }
        public String ConfirmPassword { get; set; }
        public String Email { get; set; }
        public DateTime CreateDate { get; set; }
    }
}