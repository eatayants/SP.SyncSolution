using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    public interface ISysMessage
    {
        string message { get; set; }
        messageLevelEnum messageLevel { get; set; }       
    }
}
