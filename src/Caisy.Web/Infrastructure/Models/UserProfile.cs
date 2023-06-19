﻿using Caisy.Web.Features.Shared.Models;

namespace Caisy.Web.Infrastructure.Models;

public class UserProfile : BaseEntity<UserProfile>, IUser
{
    public required string ApiKey { get; set; }
    public bool PrefersDarkMode { get; set; }
}
