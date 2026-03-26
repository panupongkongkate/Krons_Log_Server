using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
bool swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled");

builder.Services.Configure<LogUploadOptions>(
    builder.Configuration.GetSection("LogUpload"));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("X-Api-Key", new OpenApiSecurityScheme
    {
        Description = "API key required in the X-Api-Key header.",
        Type = SecuritySchemeType.ApiKey,
        Name = "X-Api-Key",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-Api-Key"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (swaggerEnabled)
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

RouteGroupBuilder logs = app.MapGroup("/api/logs");

logs.MapGet("/health", () => TypedResults.Ok(new
{
    success = true,
    service = "Krons_Log_Server",
    utcNow = DateTime.UtcNow
}));

logs.MapPost("/upload", async Task<IResult> (
    HttpRequest request,
    IFormFile? file,
    [FromForm] string? machineName,
    [FromForm] string? appVersion,
    [FromForm] string? fileName,
    [FromForm] string? createdAtUtc,
    IOptions<LogUploadOptions> options) =>
{
    LogUploadOptions settings = options.Value;

    if (!request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader) ||
        !string.Equals(apiKeyHeader.ToString(), settings.ApiKey, StringComparison.Ordinal))
    {
        return TypedResults.Unauthorized();
    }

    if (file is null || file.Length == 0)
    {
        return TypedResults.BadRequest(new
        {
            success = false,
            error = "File is required."
        });
    }

    string safeMachineName = SanitizeSegment(machineName, "unknown-machine");
    string safeFileName = SanitizeFileName(fileName, file.FileName);
    string datedDirectory = Path.Combine(
        app.Environment.ContentRootPath,
        settings.StorageRoot,
        safeMachineName,
        DateTime.UtcNow.ToString("yyyy-MM-dd"));

    Directory.CreateDirectory(datedDirectory);

    string destinationPath = EnsureUniqueFilePath(Path.Combine(datedDirectory, safeFileName));

    await using FileStream fileStream = new(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    await file.CopyToAsync(fileStream);

    return TypedResults.Ok(new
    {
        success = true,
        fileName = safeFileName,
        machineName = safeMachineName,
        appVersion = appVersion ?? "",
        createdAtUtc = createdAtUtc ?? "",
        storedPath = destinationPath
    });
})
.DisableAntiforgery();

app.Run();

static string SanitizeSegment(string? value, string fallback)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return fallback;
    }

    char[] invalidChars = Path.GetInvalidFileNameChars();
    string cleaned = new(value.Trim().Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
    return string.IsNullOrWhiteSpace(cleaned) ? fallback : cleaned;
}

static string SanitizeFileName(string? requestedFileName, string fallback)
{
    string fileName = string.IsNullOrWhiteSpace(requestedFileName) ? fallback : requestedFileName;
    string safeName = Path.GetFileName(fileName);
    return string.IsNullOrWhiteSpace(safeName) ? Path.GetFileName(fallback) : safeName;
}

static string EnsureUniqueFilePath(string path)
{
    if (!File.Exists(path))
    {
        return path;
    }

    string directory = Path.GetDirectoryName(path) ?? AppContext.BaseDirectory;
    string fileName = Path.GetFileNameWithoutExtension(path);
    string extension = Path.GetExtension(path);

    for (int i = 1; i < int.MaxValue; i++)
    {
        string candidate = Path.Combine(directory, $"{fileName}_{i}{extension}");
        if (!File.Exists(candidate))
        {
            return candidate;
        }
    }

    return path;
}
