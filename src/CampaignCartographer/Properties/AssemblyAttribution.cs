// ReSharper disable StringLiteralTypo

using System.Reflection;

[assembly: AssemblyMetadata("ModDB Page", "https://mods.vintagestory.at/campaigncartographer")]
[assembly: AssemblyMetadata("Issue Tracker", "https://github.com/ApacheTech-VintageStory-Mods/CampaignCartographer/issues")]

[assembly: ModDependency("game", "1.20.4")]
[assembly: ModDependency("survival", "1.20.4")]

[assembly: ModInfo(
    "Campaign Cartographer",
    "campaigncartographer",
    Description = "The original map mod, reborn. Adds multiple Cartography related features to the game, such as custom player pins, GPS, auto waypoint markers, and more.",
    Side = "Universal",
    Version = "4.5.0",
    RequiredOnClient = true,
    RequiredOnServer = true,
    NetworkVersion = "1.0.0",
    Website = "https://apachetech.co.uk",
    Contributors = ["Apache", "Doombox", "Melchior", "Novocain", "egocarib", "Craluminum2413", "Aledark", "Th3Dilly"],
    Authors = ["ApacheTech Solutions"])]