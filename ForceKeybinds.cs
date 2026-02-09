﻿using Terraria.ModLoader;
using JediForceMod.Content.Items;

namespace JediForceMod
{
    public class ForceKeybinds : ModSystem
    {
        public static ModKeybind OpenForceMenu { get; private set; }
        public static ModKeybind ForcePush { get; private set; }
        public static ModKeybind ForceSpeed { get; private set; }
        public static ModKeybind ForceHealth { get; private set; }
        public static ModKeybind ForceLightning { get; private set; }
        public static ModKeybind ForceMindTrick { get; private set; }
        public static ModKeybind ForceProtection { get; private set; }
        public static ModKeybind ForceSight { get; private set; }
        public static ModKeybind ForceChoke { get; private set; }
        public static ModKeybind ForceSaberThrow { get; private set; }

        public override void Load()
        {
            OpenForceMenu = KeybindLoader.RegisterKeybind(Mod, "Open Force Menu", "Q");
            ForcePush = KeybindLoader.RegisterKeybind(Mod, "Force Push", "Z");
            ForceSpeed = KeybindLoader.RegisterKeybind(Mod, "Force Speed", "X");
            ForceHealth = KeybindLoader.RegisterKeybind(Mod, "Force Heal", "C");
            ForceLightning = KeybindLoader.RegisterKeybind(Mod, "Force Lightning", "V");
            ForceMindTrick = KeybindLoader.RegisterKeybind(Mod, "Force Mind Trick", "G");
            ForceProtection = KeybindLoader.RegisterKeybind(Mod, "Force Protection", "F");
            ForceSight = KeybindLoader.RegisterKeybind(Mod, "Force Sight", "R");
            ForceChoke = KeybindLoader.RegisterKeybind(Mod, "Force Choke", "T");
            ForceSaberThrow = KeybindLoader.RegisterKeybind(Mod, "Force Saber Throw", "Y");
        }

        public override void Unload()
        {
            OpenForceMenu = null;
            ForcePush = null;
            ForceSpeed = null;
            ForceHealth = null;
            ForceLightning = null;
            ForceMindTrick = null;
            ForceProtection = null;
            ForceSight = null;
            ForceChoke = null;
            ForceSaberThrow = null;
        }
    }
}
