var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => new
{
    name = "PMS.Api",
    status = "placeholder",
    note = "Phase 1 ships placeholder controllers only. Real cloud lands in Phase 6."
});

app.MapControllers();

app.Run();
