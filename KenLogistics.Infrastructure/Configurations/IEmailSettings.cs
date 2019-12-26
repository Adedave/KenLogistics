using System;
using System.Collections.Generic;
using System.Text;

namespace KenLogistics.Infrastructure.Configurations
{
    public interface IEmailSettings
    {
        string SendGridKey { get; set; }
        string FromEmailAddress { get; set; }
    }
}
