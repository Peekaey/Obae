using System;

namespace Obae.Models;

public class UserCookie
{
    public string Name { get; set; } = "osu_session";
    public string Domain { get; set; } = ".ppy.sh";
    public string Path { get; set; } = "/";
    public DateTime Expires { get; set; }
    public string Value { get; set; }
    
}