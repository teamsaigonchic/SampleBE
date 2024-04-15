using SampleBE.Extensions;
using Service.MapperProfile;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers((opt) => opt.Filters.Add<ExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigCors();
builder.Services.ConfigJwt(configuration);
builder.Services.AddSwaggerWithAuthentication("SampleBE API", "v1.0");

builder.Services.ConfigIdentityDbContext(configuration);
builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.BusinessServices();
//builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
