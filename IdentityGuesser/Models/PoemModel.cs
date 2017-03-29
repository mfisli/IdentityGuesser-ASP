using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IdentityGuesser.Models
{
    public class PoemModel
    {
        [Key]
        public int PoemModelNumber { get; set; }
        public string ImagePath { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string Caption { get; set; }
        public IEnumerable<String> Tags { get; set; }
    }
}