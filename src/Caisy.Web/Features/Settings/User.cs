﻿namespace Caisy.Web.Features.Settings;

public interface IUser
{
    public string ApiKey { get; set; }
    public string Id { get; set; }
    public bool PrefersDarkMode { get; set; }
}