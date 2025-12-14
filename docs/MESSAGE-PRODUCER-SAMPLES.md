# Message Producer Sample Script

This script demonstrates how to send inspection request messages to RabbitMQ for testing the Inspection Worker.

## Prerequisites

Install RabbitMQ client for your language:

### Python
```bash
pip install pika
```

### Node.js
```bash
npm install amqplib
```

### .NET
```bash
dotnet add package RabbitMQ.Client
```

## Python Example

```python
import pika
import json
from datetime import datetime

# Connection parameters
credentials = pika.PlainCredentials('guest', 'guest')
parameters = pika.ConnectionParameters('localhost', 5672, '/', credentials)

# Create connection
connection = pika.BlockingConnection(parameters)
channel = connection.channel()

# Declare queue
channel.queue_declare(queue='inspection-requests', durable=True)

# Create inspection request
request = {
    "requestId": "req-12345",
    "productId": "PROD-001",
    "batchId": "BATCH-2024-001",
    "timestamp": datetime.utcnow().isoformat() + "Z",
    "measurements": {
        "temperature": 25.5,
        "pressure": 101.3,
        "humidity": 45.2
    }
}

# Publish message
channel.basic_publish(
    exchange='',
    routing_key='inspection-requests',
    body=json.dumps(request),
    properties=pika.BasicProperties(
        delivery_mode=2,  # Make message persistent
        content_type='application/json'
    )
)

print(f"Sent inspection request: {request['requestId']}")

connection.close()
```

## Node.js Example

```javascript
const amqp = require('amqplib');

async function sendMessage() {
  const connection = await amqp.connect('amqp://guest:guest@localhost:5672');
  const channel = await connection.createChannel();
  
  const queue = 'inspection-requests';
  await channel.assertQueue(queue, { durable: true });
  
  const request = {
    requestId: 'req-12345',
    productId: 'PROD-001',
    batchId: 'BATCH-2024-001',
    timestamp: new Date().toISOString(),
    measurements: {
      temperature: 25.5,
      pressure: 101.3,
      humidity: 45.2
    }
  };
  
  channel.sendToQueue(
    queue,
    Buffer.from(JSON.stringify(request)),
    { persistent: true, contentType: 'application/json' }
  );
  
  console.log(`Sent inspection request: ${request.requestId}`);
  
  await channel.close();
  await connection.close();
}

sendMessage().catch(console.error);
```

## .NET Example

```csharp
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(
    queue: "inspection-requests",
    durable: true,
    exclusive: false,
    autoDelete: false
);

var request = new
{
    requestId = "req-12345",
    productId = "PROD-001",
    batchId = "BATCH-2024-001",
    timestamp = DateTime.UtcNow,
    measurements = new
    {
        temperature = 25.5,
        pressure = 101.3,
        humidity = 45.2
    }
};

var message = JsonSerializer.Serialize(request);
var body = Encoding.UTF8.GetBytes(message);

var properties = channel.CreateBasicProperties();
properties.Persistent = true;
properties.ContentType = "application/json";

channel.BasicPublish(
    exchange: "",
    routingKey: "inspection-requests",
    basicProperties: properties,
    body: body
);

Console.WriteLine($"Sent inspection request: {request.requestId}");
```

## Consuming Results

To consume inspection results:

```python
import pika
import json

credentials = pika.PlainCredentials('guest', 'guest')
parameters = pika.ConnectionParameters('localhost', 5672, '/', credentials)
connection = pika.BlockingConnection(parameters)
channel = connection.channel()

channel.queue_declare(queue='inspection-results', durable=True)

def callback(ch, method, properties, body):
    result = json.loads(body)
    print(f"Received result: {result['requestId']} - Status: {result['status']}")
    ch.basic_ack(delivery_tag=method.delivery_tag)

channel.basic_consume(
    queue='inspection-results',
    on_message_callback=callback
)

print('Waiting for inspection results...')
channel.start_consuming()
```

## Testing with RabbitMQ Management UI

1. Open http://localhost:15672 (guest/guest)
2. Go to "Queues" tab
3. Click on "inspection-requests"
4. Expand "Publish message"
5. Set Payload:
```json
{
  "requestId": "req-12345",
  "productId": "PROD-001",
  "batchId": "BATCH-2024-001",
  "timestamp": "2024-12-14T07:52:00Z",
  "measurements": {
    "temperature": 25.5,
    "pressure": 101.3
  }
}
```
6. Click "Publish message"
7. Check "inspection-results" queue for the result
