﻿#region

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

#endregion

namespace UlteriusServer.Api.Network.Messages
{
    public static class PacketLoader
    {
        /// <summary>
        /// This dictionary contains all of the valid endpoints
        /// </summary>
        public static ConcurrentDictionary<string, PacketManager.PacketInfo> Packets =
            new ConcurrentDictionary<string, PacketManager.PacketInfo>();

        private static T ParseEnum<T>(string value)
        {
            return (T) Enum.Parse(typeof(T), value, true);
        }
        /// <summary>
        /// Load all the endpoints and their handlers from the Packets.json file
        /// </summary>
        public static void LoadPackets()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "UlteriusServer.Packets.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                var packetList = JObject.Parse(json)["endPoints"];
                foreach (var packetEntry in packetList)
                {
                    var packet = (JProperty) packetEntry;
                    var name = packet.Name;
                    var packetData = packet.Value;
                    var packetInfo = new PacketManager.PacketInfo
                    {
                        Handler = GetType(packetData["packetHandler"].ToString()),
                        EndPoint = ParseEnum<PacketManager.EndPoints>(packetData["endPoint"].ToString())
                    };
                    Packets.TryAdd(name, packetInfo);
                }
            }
            Console.WriteLine($"{Packets.Count} packet handlers loaded!");
        }
        /// <summary>
        /// Turns a string into a class Type
        /// </summary>
        private static Type GetType(string v)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).First(x => x.Name == v);
        }
    }
}