using System.Diagnostics;
using System.Threading.Channels;
using Channels;

const int numTables = 5;

var restaurantOpenDuration = TimeSpan.FromSeconds(10);

IReadOnlyList<Food> menu =
[
	new Turkey(),
	new Turkey(),
	new Turkey(),
	new Gravy(),
	new MashedPotatoes(),
	new Stuffing()
];

var options = new BoundedChannelOptions(numTables)
{
	Capacity = numTables,
	SingleReader = true,
	FullMode = BoundedChannelFullMode.Wait,
	AllowSynchronousContinuations = true,
	SingleWriter = false
};
var ordersChannel = Channel.CreateBounded<Food>(options);

var kitchenTask = KitchenTask();

Trace.WriteLine("Restaurant Opened");
var restaurantOpenedTime = DateTimeOffset.UtcNow;

while (restaurantOpenedTime + restaurantOpenDuration > DateTimeOffset.UtcNow)
{
	// Take Order
	var food = GetOrder();

	Trace.WriteLine("");
	Trace.WriteLine($"Submitting Order for {food.Name}. Total Unread Orders: {ordersChannel.Reader.Count}.");
	Trace.WriteLine("");

	var numSecondsBeforeNextOrder = Random.Shared.Next(1, 2);
	await ordersChannel.Writer.WriteAsync(food);

	// Wait for next order
	await Task.Delay(TimeSpan.FromSeconds(numSecondsBeforeNextOrder));
}

Trace.WriteLine("");
Trace.WriteLine($"Restaurant closing with {ordersChannel.Reader.Count} orders remaining to cook in the kitchen");
Trace.WriteLine("");

ordersChannel.Writer.Complete();
await kitchenTask;

Trace.WriteLine("");
Trace.WriteLine("Kitchen Closed");
Trace.WriteLine("");

Food GetOrder()
{
	var randomOrderNumber = Random.Shared.Next(0, menu.Count - 1);
	return menu[randomOrderNumber];
}

async Task KitchenTask(CancellationToken token = default)
{
	await foreach (var food in ordersChannel.Reader.ReadAllAsync(token))
	{
		Trace.WriteLine("");
		Trace.WriteLine($"Reading Order for {food.Name}");
		Trace.WriteLine("");

		await food.Cook(token);
	}
}