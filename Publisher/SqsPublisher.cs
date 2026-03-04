using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace ms_games.Publisher
{
  public class SqsPublisher
  {
    private readonly IAmazonSQS _sqs;
    private readonly string _queueUrl;

    public SqsPublisher(IAmazonSQS sqs)
    {
      _sqs = sqs;
      _queueUrl = Environment.GetEnvironmentVariable("PAYMENT_QUEUE_URL");
    }

    public async Task Publish<T>(T message)
    {
      var body = JsonSerializer.Serialize(message);

      await _sqs.SendMessageAsync(new SendMessageRequest
      {
        QueueUrl = _queueUrl,
        MessageBody = body
      });
    }
  }
}
