using EutherBoot;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var root = Path.GetFullPath(builder.Configuration["EUTHERBOOT_ROOT"] ?? Path.Combine(builder.Environment.ContentRootPath, "..", "..", "eutherboot"));
var bootUrl = builder.Configuration["EUTHERBOOT_BOOT_URL"] ?? "http://127.0.0.1:8080";

var profileStore = new ProfileStore(Path.Combine(root, "profiles"));
var assignmentStore = new AssignmentStore(Path.Combine(root, "assignments.txt"));

var app = builder.Build();

Directory.CreateDirectory(root);
Directory.CreateDirectory(Path.Combine(root, "generated"));
Directory.CreateDirectory(Path.Combine(root, "www", "boot"));

var staticRoot = Path.Combine(root, "www", "boot");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(staticRoot),
    RequestPath = ""
});

app.MapGet("/", () => Results.Content(AdminPage.Render(profileStore.LoadProfiles(), assignmentStore.Load(), bootUrl), "text/html"));

app.MapGet("/api/profiles", () => profileStore.LoadProfiles());
app.MapGet("/api/assignments", () => assignmentStore.Load());

app.MapPost("/api/assignments", (string mac, string profile) =>
{
    if (profileStore.FindProfile(profile) is null)
        return Results.NotFound($"Unknown profile: {profile}");

    assignmentStore.Set(mac, profile);
    return Results.Ok(new BootAssignment(AssignmentStore.NormalizeMac(mac), profile));
});

app.MapGet("/api/boot", (string? mac) =>
{
    var profiles = profileStore.LoadProfiles();
    var assignedProfileName = string.IsNullOrWhiteSpace(mac) ? null : assignmentStore.GetProfileForMac(mac);
    var assignedProfile = assignedProfileName is null
        ? null
        : profiles.FirstOrDefault(profile => string.Equals(profile.Name, assignedProfileName, StringComparison.OrdinalIgnoreCase));

    var script = assignedProfile is null
        ? MenuGenerator.GenerateDefaultMenu(profiles, bootUrl)
        : MenuGenerator.GenerateSingleBoot(assignedProfile, bootUrl);

    return Results.Text(script, "text/plain");
});

app.MapPost("/api/generate", () =>
{
    var menu = MenuGenerator.GenerateDefaultMenu(profileStore.LoadProfiles(), bootUrl);
    var generatedPath = Path.Combine(root, "generated", "menu.ipxe");
    var publicPath = Path.Combine(root, "www", "boot", "menu.ipxe");

    File.WriteAllText(generatedPath, menu);
    File.WriteAllText(publicPath, menu);

    return Results.Ok(new { generatedPath, publicPath });
});

app.MapGet("/generated/menu.ipxe", () =>
{
    var menu = MenuGenerator.GenerateDefaultMenu(profileStore.LoadProfiles(), bootUrl);
    return Results.Text(menu, "text/plain");
});

app.Run();
