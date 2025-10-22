using Ubiminds.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddLoggingConfiguration(builder.Configuration);

builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddApiConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}

app.UseHttpsRedirection();
app.UseCorsConfiguration();
app.UseAuthorization();
app.MapControllers();

app.Run();
