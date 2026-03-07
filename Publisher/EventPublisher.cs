using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace ms_games.Publisher
{
  public class EventPublisher
  {
    private readonly IAmazonSQS _sqs;

    public EventPublisher(IAmazonSQS sqs)
    {
      _sqs = sqs;
    }

    public async Task PublishAsync(string queueUrl, object message)
    {
      var body = JsonSerializer.Serialize(message);

      await _sqs.SendMessageAsync(new SendMessageRequest
      {
        QueueUrl = queueUrl,
        MessageBody = body
      });
    }
  }
}