using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
var builder = WebApplication.CreateBuilder(args);

// Configurar puerto ANTES de build - CRÍTICO para Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Servicios mínimos necesarios
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS permisivo para desarrollo
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar headers para proxy (Render usa proxies)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Headers para proxies
app.UseForwardedHeaders();

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Solo HTTPS redirect en desarrollo - MUY IMPORTANTE para Render
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors();

// Health check para Render
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}))
.WithName("HealthCheck")
.WithTags("Health");

// Endpoint principal
app.MapGet("/", () => "🤖 Chatbot API is running! Endpoints: /health, /api/chat")
.WithName("Root")
.WithTags("Info");

// Chatbot endpoint
app.MapPost("/api/chat", (ChatRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { error = "Message is required" });
    }

    var userMessage = request.Message.ToLower().Trim();

    var response = userMessage switch
    {
        var msg when msg.Contains("hola") || msg.Contains("hello") =>
            "¡Hola! 👋 Soy tu asistente virtual. ¿En qué puedo ayudarte?",
        var msg when msg.Contains("portfolio") =>
            "Este es mi portfolio estilo VS Code. 💻 ¿Quieres ver mis proyectos?",
        var msg when msg.Contains("cv") || msg.Contains("curriculum") =>
            "Puedes descargar mi CV desde la barra lateral del portfolio. 📄",
        var msg when msg.Contains("proyectos") || msg.Contains("projects") =>
            "Tengo varios proyectos interesantes. ¡Échales un vistazo! 🚀",
        var msg when msg.Contains("contacto") || msg.Contains("contact") =>
            "Puedes contactarme a través de mi portfolio o redes sociales. 📧",
        var msg when msg == "date" || msg == "fecha" =>
       $"📅 La fecha actual es: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
        var msg when msg.Contains("ayuda") || msg.Contains("help") =>
            "Puedes preguntarme sobre: portfolio, cv, proyectos, contacto o fecha. 💡",
        _ => $"Recibí tu mensaje: '{request.Message}' 🤔 ¿Podrías ser más específico? Escribe 'ayuda' para ver qué puedo hacer."
    };

    return Results.Ok(new ChatResponse
    {
        Reply = response,
        Timestamp = DateTime.UtcNow,
        MessageId = Guid.NewGuid().ToString("N")[..8]
    });
})
.WithName("Chat")
.WithTags("Chat");


// Endpoint de información sobre la API
app.MapGet("/api/info", () => Results.Ok(new
{
    name = "Chatbot API",
    version = "1.0.0",
    description = "API simple para chatbot de portfolio",
    endpoints = new[]
    {
        "/health - Health check",
        "/api/chat - Chatbot principal",
        "/api/info - Información de la API"
    },
    author = "Tu Nombre",
    timestamp = DateTime.UtcNow
}))
.WithName("ApiInfo")
.WithTags("Info");

// Logging para debugging en Render
app.Logger.LogInformation($"🚀 Starting Chatbot API on port {port}");

app.Run();

// Records y clases
public record ChatRequest(string Message);

public record ChatResponse
{
    public string Reply { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string MessageId { get; set; } = "";
}