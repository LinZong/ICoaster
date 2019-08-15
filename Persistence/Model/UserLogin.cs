using System;
using System.Collections.Generic;

namespace ICoaster.Model
{
    public partial class UserLogin
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public string Credentials { get; set; }
    }
}
