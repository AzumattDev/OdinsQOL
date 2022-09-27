/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using YamlDotNet.Serialization;

namespace OdinQOL.Patches.BiFrost;

public class BiFrost
{
    public static ConfigEntry<Vector2> UIAnchor = null!;
    public static ConfigEntry<bool> DisableBiFrost = null!;
    public static ConfigEntry<bool> ShowPasswordPrompt = null!;
}

internal class BiFrostServers
{
    internal static string ConfigFileName = "com.odinplusqol.mod_servers.yml";

    public static string ConfigPath = Paths.ConfigPath +
                                      Path.DirectorySeparatorChar + ConfigFileName;

    public static List<BiFrostDefinition> entries = new();

    public static void Init()
    {
        if (BiFrost.DisableBiFrost.Value) return;
        entries.Clear();
        try
        {
            if (!File.Exists(ConfigPath))
            {
                using StreamWriter streamWriter = File.CreateText(ConfigPath);
                streamWriter.Write(new StringBuilder()
                    .AppendLine(
                        "# Configure your servers for the Bifröst in this file. (Aka Azumatt's FastLink mod! Imported into OdinQOL in order to replace the old ConnectPanel)")
                    .AppendLine("# Servers are automatically sorted alphabetically when shown in the list.")
                    .AppendLine(
                        "# This file live updates the in-game listing. Feel free to change it while in the main menu.")
                    .AppendLine("")
                    .AppendLine("Example Server:")
                    .AppendLine("  address: example.com")
                    .AppendLine("  port: 1234")
                    .AppendLine("  password: somepassword")
                    .AppendLine("")
                    .AppendLine("Some IPv6 Server:")
                    .AppendLine("  address: 2606:2800:220:1:248:1893:25c8:1946")
                    .AppendLine("  port: 4023")
                    .AppendLine("  password: a password with spaces")
                    .AppendLine("")
                    .AppendLine("Passwordless IPv4 Server:")
                    .AppendLine("  address: 93.184.216.34")
                    .AppendLine("  port: 9999")
                    .AppendLine("")
                    .AppendLine(
                        "# You can optionally change the color of your server name. Does not work for the address and port. Also, can show PvP status.")
                    .AppendLine("<color=red>Another IPv4 Server</color>:")
                    .AppendLine("  address: 192.0.2.146")
                    .AppendLine("  port: 9999")
                    .AppendLine("  ispvp: true"));
                streamWriter.Close();
            }

            if (File.Exists(ConfigPath))
            {
                entries.AddRange(BiFrostDefinition.Parse(File.ReadAllText(ConfigPath)));

                OdinQOLplugin.QOLLogger.LogDebug($"Loaded {entries.Count} server entries");
            }
        }
        catch (Exception ex)
        {
            OdinQOLplugin.QOLLogger.LogError($"BiFrost: Error loading config {ex}");
        }
    }
}

public class BiFrostDefinition
{
    public string serverName = null!;

    public ushort port { get; set; }

    public string address { get; set; } = null!;

    public bool ispvp { get; set; } = false;

    public string password { get; set; } = "";

    public static IEnumerable<BiFrostDefinition> Parse(string yaml) => new DeserializerBuilder().IgnoreFields().Build()
        .Deserialize<Dictionary<string, BiFrostDefinition>>(yaml).Select(kv =>
        {
            BiFrostDefinition def = kv.Value;
            def.serverName = kv.Key;
            return def;
        });

    public override string ToString() => $"Server(name={serverName},address={address},port={port})";
}*/