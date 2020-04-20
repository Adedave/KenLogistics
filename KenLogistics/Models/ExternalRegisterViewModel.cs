using KenLogistics.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KenLogistics.Web.Models
{
    public class ExternalRegisterViewModel
    {
        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email Address is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [StringLength(11,ErrorMessage ="Phone Number must be 11 digits")]
        public string PhoneNumber { get; set; }

        [Required]
        [MustBeTrue(ErrorMessage = "Please agree to the Terms and Conditions")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "Please agree to Terms and Conditions")]
        //[Compare("IsTrue", ErrorMessage = "Please agree to Terms and Conditions")]
        public bool TermsAndConditions { get; set; }
        public string ReturnUrl { get; set; }
    }
}
