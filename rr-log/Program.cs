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
	opts.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.RequestProperties | HttpLoggingFields.RequestBody | HttpLoggingFields.ResponseBody;
	opts.LoggingFields = HttpLoggingFields.All;

/* 	//log thess headers
	opts.RequestHeaders.Add(HeaderNames.ContentType);
	opts.RequestHeaders.Add(HeaderNames.ContentEncoding);
	opts.RequestHeaders.Add(HeaderNames.ContentLength);
 */    

	//log the body as text for the following types of requests
	opts.MediaTypeOptions.AddText("application/json", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("text/json", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("text/plain", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("application/xml", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("application/x-www-form-urlencoded", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("multipart/form-data", Encoding.UTF8);

    opts.MediaTypeOptions.AddText("application/json; charset=utf-8", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("text/json; charset=utf-8", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("text/plain; charset=utf-8", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("application/xml; charset=utf-8", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("application/x-www-form-urlencoded; charset=utf-8", Encoding.UTF8);
	opts.MediaTypeOptions.AddText("multipart/form-data; charset=utf-8", Encoding.UTF8);

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