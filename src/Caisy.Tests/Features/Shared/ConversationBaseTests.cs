using Caisy.Web.Features.Shared;

namespace Caisy.Tests.Features.Shared;

public class ConversationBaseTests
{
    [Fact]
    public void GivenOneMessage_WhenUndone_ThenIsRemoved()
    {
        var conversation = new TestConversation();
        conversation.AddUserMessage(RandomString);

        conversation.UndoLastConversation();

        conversation.MessageCount.Should().Be(0);
    }

    [Fact]
    public void GivenThreeOrMoreMessages_WhenUndone_ThenOnlyTwoAreRemoved()
    {
        var conversation = new TestConversation();
        conversation.AddUserMessage(RandomString);
        conversation.AddCaisyMessage(RandomString);
        conversation.AddUserMessage(RandomString);
        conversation.AddCaisyMessage(RandomString);

        conversation.UndoLastConversation();

        conversation.MessageCount.Should().Be(2);
    }
}

public class TestConversation : ConversationBase { }
