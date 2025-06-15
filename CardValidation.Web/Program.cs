using CardValidation.Core.Services;
using CardValidation.Core.Services.Interfaces;
using CardValidation.Infrustructure;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    services.AddTransient<ICardValidationService, CardValidationService>();

    services.AddMvc(options =>
    {
        options.Filters.Add(typeof(CreditCardValidationFilter)); ;
    });
}
public partial class Program { }

