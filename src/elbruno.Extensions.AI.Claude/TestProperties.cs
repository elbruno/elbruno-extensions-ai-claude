using Microsoft.Extensions.AI;
using System.Collections.Generic;

namespace elbruno.Extensions.AI.Claude;

public class TestProperties
{
    public void Test()
    {
        var r = new ChatResponse(new List<ChatMessage>());
        var m = r.Messages;
        var u = r.Usage; // Check if this exists
    }
}
