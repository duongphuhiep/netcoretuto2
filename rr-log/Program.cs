using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Net.Http.Headers;
using System.Text;

var configBuilder = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
var config = configBuilder.Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging(loggingBuilder =>
	{
		loggingBuilder.AddSeq(config.GetSection("Seq"));
	});
builder.Services.AddHttpLogging(opts =>
{
	opts.LoggingFields = HttpLoggingFields.All;

	//log the body as text for the following types of requests
	opts.MediaTypeOptions.AddText("application/json", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("text/json", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("text/plain", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("application/xml", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("application/x-www-form-urlencoded", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("multipart/form-data", Encoding.UTF8);

	opts.RequestBodyLogLimit = 4096;
	opts.ResponseBodyLogLimit = 4096;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//log all request and response
app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapPost("/hello", (Person p) => new {greet = $"hello, {p.name}", young = p.age<50});
app.Run();


record Person(string name, int age);