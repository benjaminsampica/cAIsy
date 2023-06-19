﻿using static Caisy.Web.Features.Shared.Models.ConversationBase;

namespace Caisy.Web.Infrastructure.Models;

public class ChatHistory : BaseEntity<ChatHistory>
{
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
    public string Summary
    {
        get
        {
            var message = string.Empty;
            if (Messages.Any())
            {
                message = string.Concat(Messages[1].Content!.AsSpan(0, Math.Min(Messages[1].Content!.Length, 100)), " ...");
            }
            return message;
        }
    }

    public required List<ChatHistoryMessage> Messages { get; set; }

    public class ChatHistoryMessage
    {
        public required MessageRole Role { get; set; }
        public required string Content { get; set; }
    }
}