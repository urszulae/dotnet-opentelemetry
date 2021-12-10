using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var resourceBuilder = ResourceBuilder.CreateDefault() 
     .AddService("Data-Service-NR")
     .AddTelemetrySdk(); //adds a suite of standard attributes defined within open telemetry's semantic conventions - name version etc 

builder.Services.AddOpenTelemetryTracing(builder => 
{
        builder
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter( options => 
            {
                options.Endpoint = new Uri("https://otlp.nr-data.net:4317");
                options.Headers = $"{Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS")}"; //New Relic API key goes here

            });
});


builder.Services.AddOpenTelemetryMetrics(builder => 
{
        builder
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter( options => 
            {
                options.Endpoint = new Uri("https://otlp.nr-data.net:4317");
                options.Headers = $"{Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS")}";

                options.AggregationTemporality = AggregationTemporality.Delta; //applicable to Metrics - new relic does not currently support cumulative, only delta

            });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
