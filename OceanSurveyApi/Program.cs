var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b =>
         b.WithOrigins("https://ocean-personality.netlify.app")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();


app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ocean Survey API v1");
    c.RoutePrefix = "swagger"; // /swagger
});

// Optional: quick checks
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok("OK"));

app.MapControllers();
app.Run();
