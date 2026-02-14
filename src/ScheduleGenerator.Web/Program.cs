using ScheduleGenerator.Web.Components;
using ScheduleGenerator.Application.Services;
using ScheduleGenerator.Application.Interfaces;
using ScheduleGenerator.Application.Validators;
using ScheduleGenerator.Infrastructure.Algorithms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register Application services
builder.Services.AddScoped<IMatchGenerationService, MatchGenerationService>();
builder.Services.AddScoped<ISchedulingService, SchedulingService>();
builder.Services.AddScoped<ISlotGenerationService, SlotGenerationService>();
builder.Services.AddScoped<ITournamentOrchestrator, TournamentOrchestrator>();

// Register Infrastructure services
builder.Services.AddScoped<ISchedulingEngine, BacktrackingScheduler>();

// Register validators
builder.Services.AddScoped<TournamentDefinitionValidator>();
builder.Services.AddScoped<FieldDefinitionValidator>();
builder.Services.AddScoped<FormatConfigurationValidator>();
builder.Services.AddScoped<SchedulingRulesValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
