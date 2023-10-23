using UniscaleDemo.Messages;
using UniscaleDemo.Messages.Messages;
using UniscaleDemo.Messages_1_0;
using Uniscale.Core;

namespace Streams {
    public class TimelineInterceptors {
        private static Dictionary<Guid, MessageFull> messages = new Dictionary<Guid, MessageFull>();

        public static void registerInterceptors(PlatformInterceptorBuilder builder) {
            var patterns = Patterns.Messages;
            builder
                // Register an interceptor for the message feature SendMessage
                .InterceptMessage(
                    // Specify the AllMessageUsages pattern so that the implementation
                    // picks up features for all use case instances this feature
                    // is used in
                    patterns.SendMessage.AllMessageUsages,
                    // Define a handler for the feature
                    patterns.SendMessage.Handle(async (input, ctx) => {
                        if (input.Message.Length < 3 || input.Message.Length > 60)
                            return Result.BadRequest(ErrorCodes.Messages.InvalidMessageLength);
                        var msg = new MessageFull {
                            MessageIdentifier = Guid.NewGuid(),
                            Message = input.Message,
                            Created = new UserTag { By = input.By, At = DateTime.Now }
                        };
                        messages.Add(msg.MessageIdentifier, msg);
                        return Result.Ok();
                    }))
                // Register an interceptor for the request/response feature GetMessageList
                .InterceptRequest(
                    patterns.GetMessageList.AllRequestUsages,
                    patterns.GetMessageList.Handle((input, ctx) => {
                        return messages.Values
                            .OrderByDescending(m => m.Created.At)
                            .Take(50)
                            .ToList();
                    }));
        }
    }
}
