using Microsoft.EntityFrameworkCore;
using AudioUploadApi.Data;
using AudioUploadApi.Services;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Azure Blob Storage
builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration.GetConnectionString("AzureStorage")));
builder.Services.AddScoped<IAudioStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IAudioCompressionService, FFmpegAudioCompressionService>();
builder.Services.AddScoped<IAudioSummaryService, AudioSummaryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
