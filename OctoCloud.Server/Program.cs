using System.Runtime.InteropServices;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using SystemInfo = OctoCloud.Settings.SystemInfo;
using MusicSettings = OctoCloud.Settings.Music;
using RedisSettings = OctoCloud.Settings.Redis;
using DatabaseSettings = OctoCloud.Settings.Database;
using AcoustIDClient = OctoCloud.Server.Clients.Musicbrainz.AcoustIDClient;
using AudioFingerprint = OctoCloud.Server.Clients.Musicbrainz.AudioFingerprint;
using MusicbrainzMatch = OctoCloud.Server.Clients.Musicbrainz.MusicMatch;
using DatabaseClass = OctoCloud.Server.Data.Database;
// User Model
using UserModel = OctoCloud.Server.Models.User;
// Music
using MusicModel    = OctoCloud.Server.Models.Music.Music;
using ArtistModel   = OctoCloud.Server.Models.Music.Artist;
using AlbumModel    = OctoCloud.Server.Models.Music.Album;
using OctoCloud.Server.Clients.Musicbrainz;
using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
if(builder.Environment.EnvironmentName == "Development")
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
else 
    builder.Configuration.AddJsonFile("appsettings.json", optional: false);
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<MusicSettings>(builder.Configuration.GetSection("Music"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));

// Add Redis services
builder.Services.AddStackExchangeRedisCache(options => {
    var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisSettings>();
    options.Configuration = redisSettings.Configuration;
    options.InstanceName = redisSettings.InstanceName;
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
    {
        EndPoints = { redisSettings.Configuration },
        User = redisSettings.Username ,
        Password = redisSettings.Password
    };
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Health Check
builder.Services.AddHealthChecks();
// Remove Server header, for security purposes
builder.WebHost.UseKestrel(option => option.AddServerHeader = false);

builder.Services.AddDataProtection();
// Add Authentication services
builder.Services.AddAuthentication("UserSessionAuth").AddCookie("UserSessionAuth", options => {
    options.Cookie.Name = "SessionToken";
    options.LoginPath = "/Account/Login";
});

// Add Session services
builder.Services.AddDistributedMemoryCache();
// Adds a default in-memory implementation of IDistributedCache
builder.Services.AddSession(options => { 
    options.IdleTimeout = TimeSpan.FromDays(1); // Set session timeout 
    options.Cookie.HttpOnly = true; // Ensure the session cookie is accessible only by the server 
    options.Cookie.IsEssential = true; // Make the session cookie essential
    options.Cookie.Name = "SessionToken"; // Change session cookie name
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
// Allow Reserve Proxy
app.UseForwardedHeaders();
// Add health check
app.MapHealthChecks("/healthz");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add session
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

// Get music settings
var musicSettings = app.Services.GetRequiredService<IOptions<MusicSettings>>().Value;
// Get database settings 
var databaseSettings = app.Services.GetRequiredService<IOptions<DatabaseSettings>>().Value; 

// Initialize the DB
DatabaseClass database = DatabaseClass.Instance(
   databaseSettings.Host, databaseSettings.Port,
   databaseSettings.Name,
   databaseSettings.User, databaseSettings.Password
);

// Musicbrainz
AcoustIDClient acoustIDClient = new AcoustIDClient();
MusicbrainzClient musicbrainzClient = new MusicbrainzClient(musicSettings.ApiKey);

Console.WriteLine("================ SYSTEM INFORMATION ================");
Console.WriteLine("OS: " + SystemInfo.getOS());
Console.WriteLine("Platform: " + SystemInfo.GetPlatform());
Console.WriteLine("Architecture: " + SystemInfo.getCPUArch());
Console.WriteLine("Chromaprint location: " + acoustIDClient._chromaPath);
//Console.WriteLine("Current Path: " + System.IO.Directory.GetCurrentDirectory());
Console.WriteLine("Database " + database.DatabaseInfo());
Console.WriteLine("====================================================");

// Create tables
UserModel.CreateTable();
Console.WriteLine("Initialized User's Db Table");
MusicModel.CreateTable();
Console.WriteLine("Initialized Music's Db Table");
ArtistModel.CreateTable();
Console.WriteLine("Initialized Artist's Db Table");
AlbumModel.CreateTable();
Console.WriteLine("Initialized Album's Db Table");

// Scan and parse the songs
bool isExecuting = false;
async void UpdateContent() {
    isExecuting = true;
    Console.WriteLine("Updating music database...");
    foreach (string localFilePath in Directory.GetFiles(Path.GetFullPath(musicSettings.Location), "*", SearchOption.AllDirectories))
    {
        string extension = Path.GetExtension(localFilePath).ToLowerInvariant();
        if (extension != ".mp3" && extension != ".wav" && extension != ".ogg" && extension != ".flac") { 
            continue;
        }

        // Do not parse again if the music file's info already exists on the database
        try {
            MusicModel.FindMusicByLocalFilePath(localFilePath);
            Console.WriteLine("Already parsed: " + localFilePath);
            continue;
        } catch (Exception ex) {}

        // If song does not exist, insert it.

        string remoteFilePath = localFilePath.Replace(Path.GetFullPath(musicSettings.Location), "/Music/files").Replace("\\", "/");

        // Try to find music info based on its fingerprint
        MusicbrainzMatch musicMatch;
        try{ 
            Console.WriteLine("Finding match for song...");
            AudioFingerprint musicFingerprint = acoustIDClient.GetFingerprint($"\"{localFilePath}\"");
            musicMatch = await musicbrainzClient.GetBestMatchFromFingerprint(musicFingerprint.Fingerprint, musicFingerprint.Duration);
        }catch(Exception ex){
            if(ex.Message == "Music not found" || ex.Message == "Could not parse audio duration, got 0"){
                Console.WriteLine("Could not parse audio file: " + localFilePath);
                continue;
            } else {
                MethodInfo preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace",
                BindingFlags.Instance | BindingFlags.NonPublic);
                preserveStackTrace.Invoke(ex, null);
                throw;
            }
        }
        // Create artists if does not exist
        LinkedList<ArtistModel> artists = new LinkedList<ArtistModel>();
        foreach(Artist artist in musicMatch.Artists) {
            try{ 
                artists.AddLast(new ArtistModel(artist.Id));
            } catch(Exception ex) {
                if(ex.Message == "Artist not found")
                    artists.AddLast(ArtistModel.Create(artist.Id, artist.Name));
                else {
                    MethodInfo preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                    preserveStackTrace.Invoke(ex, null);
                    throw;
                }
            }
        }

        // Create album if it does not exist
        AlbumModel album;
        try{ 
            album = new AlbumModel(musicMatch.ReleaseGroup.Id);
        } catch(Exception ex) {
            if(ex.Message == "Album not found")
                album = AlbumModel.Create(musicMatch.ReleaseGroup.Id, musicMatch.ReleaseGroup.Title);
            else{
                MethodInfo preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace",
                BindingFlags.Instance | BindingFlags.NonPublic);
                preserveStackTrace.Invoke(ex, null);
                throw;
            }
        }


        // Create music if does not exist
        MusicModel music;
        try{ 
            music = new MusicModel(musicMatch.Id);
        } catch(Exception ex) {
            if(ex.Message == "Music not found")
                music = MusicModel.Create(
                    musicMatch.Id, musicMatch.Title,
                    album, artists.ToArray<ArtistModel>(),
                    remoteFilePath, localFilePath
                );
            else throw ex;
        }

        Console.WriteLine("================ Music Match ================");
        Console.WriteLine("-> Title: " + music.Title);
        Console.WriteLine("-> Artists: " + string.Join(", ", music.Artists.Select(artist => artist.Name).ToArray<string>()));
        Console.WriteLine("-> Album: " + music.Album.Name);
        Console.WriteLine("-> Album Image: " + music.Album.ImageUrl);
        Console.WriteLine("-> API Path: " + music.StreamUrl);
        Console.WriteLine("-> File Path: " + music.LocalPath);
        Console.WriteLine("=======================================");

    }
    isExecuting = false;
}

// Watch for file changes
/*
FileSystemWatcher musicChangeWatcher = new FileSystemWatcher();
musicChangeWatcher.Path = musicSettings.Location;
musicChangeWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size;
musicChangeWatcher.Changed += FsUpdateContent;
musicChangeWatcher.Created += FsUpdateContent;
musicChangeWatcher.Deleted += FsUpdateContent;
musicChangeWatcher.Renamed += FsUpdateContent;
musicChangeWatcher.EnableRaisingEvents = true;

void FsUpdateContent(object sender, FileSystemEventArgs e)
{
    if(isExecuting) return;

    isExecuting = true;
    UpdateContent();
}*/

//UpdateContent();
app.Run();