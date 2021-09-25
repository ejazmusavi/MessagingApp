using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
namespace MessagingApp.Data
{
    public class Message
    {
        [key]
        public int Id { get; set; }

        [MaxLength(160)]
        public string MessagDescription { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Password { get; set; }
        [ForeignKey("CreatedBy")]
       public IdentityUser Useer { get; set; }
    }
}
