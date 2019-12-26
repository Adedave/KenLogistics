using System;
using System.Collections.Generic;
using System.Text;

namespace KenLogistics.Infrastructure.Configurations
{
    public class EmailSettings : IEmailSettings
    {
        public string SendGridKey { get; set; }
        public string FromEmailAddress { get; set; }
    }
}
