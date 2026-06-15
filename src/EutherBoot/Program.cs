using EutherBoot;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var root = Path.GetFullPath(builder.Configuration["EUTHERBOOT_ROOT"] ?? Path.Combine(builder.Environment.ContentRootPath, "..", "..", "eutherboot"));
var bootUrl = builder.Configuration["EUTHERBOOT_BOOT_URL"] ?? "http://127.0.0.1:8080";

var profileStore = new ProfileStore(Path.Combine(root, "profiles"));
var assignmentStore = new AssignmentStore(Path.Combine(root, "assignments.txt"));
var staticRoot = Path.Combine(root, "www", "boot");
var assetChecker = new BootAssetChecker(staticRoot);
var isoLibrary = new IsoLibrary(Path.Combine(root, "isos"), assetChecker);
var mountRoot = Path.Combine(staticRoot, "mounts");
var mountManager = new MountManager(isoLibrary, assetChecker, mountRoot, builder.Configuration["EUTHERBOOT_MOUNT_HELPER"]);

var app = builder.Build();

Directory.CreateDirectory(root);
Directory.CreateDirectory(Path.Combine(root, "generated"));
Directory.CreateDirectory(staticRoot);
Directory.CreateDirectory(mountRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(staticRoot),
    RequestPath = ""
});

IReadOnlyList<BootProfile> LoadMenuProfiles()
{
    var profiles = profileStore.LoadProfiles();
    return isoLibrary.GetMenuProfiles(profiles);
}

app.MapGet("/", () =>
{
    var profiles = profileStore.LoadProfiles();
    var isos = isoLibrary.Scan(profiles);
    return Results.Content(AdminPage.Render(profiles, LoadMenuProfiles(), isos, assignmentStore.Load(), bootUrl), "text/html");
});

app.MapGet("/simulator", () => Results.Content(SimulatorPage.Render(LoadMenuProfiles(), bootUrl), "text/html"));

app.MapGet("/api/profiles", () => profileStore.LoadProfiles());
app.MapGet("/api/isos", () => isoLibrary.Scan(profileStore.LoadProfiles()));
app.MapGet("/api/assets/check", (string kernel, string initrd) => assetChecker.Check(kernel, initrd));
app.MapGet("/api/assignments", () => assignmentStore.Load());

app.MapPost("/api/assignments", (string mac, string profile) =>
{
    if (profileStore.FindProfile(profile) is null)
        return Results.NotFound($"Unknown profile: {profile}");

    assignmentStore.Set(mac, profile);
    return Results.Ok(new BootAssignment(AssignmentStore.NormalizeMac(mac), profile));
});

app.MapDelete("/api/assignments", (string mac) =>
{
    assignmentStore.Remove(mac);
    return Results.NoContent();
});

app.MapGet("/api/boot", (string? mac) =>
{
    var profiles = LoadMenuProfiles();
    var assignedProfileName = string.IsNullOrWhiteSpace(mac) ? null : assignmentStore.GetProfileForMac(mac);
    var assignedProfile = assignedProfileName is null
        ? null
        : profiles.FirstOrDefault(profile => string.Equals(profile.Name, assignedProfileName, StringComparison.OrdinalIgnoreCase));

    var script = assignedProfile is null
        ? MenuGenerator.GenerateDefaultMenu(profiles, bootUrl)
        : MenuGenerator.GenerateSingleBoot(assignedProfile, bootUrl);

    return Results.Text(script, "text/plain");
});

app.MapGet("/api/boot/profile/{profileName}", (string profileName) =>
{
    var profiles = LoadMenuProfiles();
    var profile = profiles.FirstOrDefault(item => string.Equals(item.Name, profileName, StringComparison.OrdinalIgnoreCase));
    if (profile is null)
        return Results.Text("#!ipxe\necho Unknown EutherBoot profile\nshell\n", "text/plain");

    var mountResult = mountManager.EnsureMounted(profile, profileStore.LoadProfiles());
    var script = mountResult.Success
        ? MenuGenerator.GenerateSingleBoot(profile, bootUrl)
        : MenuGenerator.GenerateMountError(profile, mountResult, bootUrl);

    return Results.Text(script, "text/plain");
});

app.MapPost("/api/generate", () =>
{
    var menu = MenuGenerator.GenerateDefaultMenu(LoadMenuProfiles(), bootUrl);
    var generatedPath = Path.Combine(root, "generated", "menu.ipxe");
    var publicPath = Path.Combine(root, "www", "boot", "menu.ipxe");

    File.WriteAllText(generatedPath, menu);
    File.WriteAllText(publicPath, menu);

    return Results.Ok(new { generatedPath, publicPath });
});

app.MapGet("/generated/menu.ipxe", () =>
{
    var menu = MenuGenerator.GenerateDefaultMenu(LoadMenuProfiles(), bootUrl);
    return Results.Text(menu, "text/plain");
});

app.Run();
