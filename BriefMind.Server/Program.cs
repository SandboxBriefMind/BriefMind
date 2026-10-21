var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<BriefMind.Server.Services.IPdfTextExtractor, BriefMind.Server.Services.PdfTextExtractor>();
builder.Services.AddSingleton<BriefMind.Server.Services.IClauseSegmenter, BriefMind.Server.Services.ClauseSegmenter>();
builder.Services.AddHttpClient<BriefMind.Server.Infrastructure.IOllamaClient, BriefMind.Server.Infrastructure.OllamaClient>();
builder.Services.AddScoped<BriefMind.Server.Infrastructure.IDebateEngine, BriefMind.Server.Infrastructure.DebateEngine>();
builder.Services.AddSingleton<BriefMind.Server.Services.IProvenanceService, BriefMind.Server.Services.ProvenanceService>();

// Configure CORS for React/Vite dev servers
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
