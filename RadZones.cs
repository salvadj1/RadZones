using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Fougerite;
using Fougerite.Events;
using UnityEngine;

namespace RadZones
{
    public class RadZones : Fougerite.Module
    {

        #region Variables
        public IniParser SettingsFile;
        public IniParser ZonesFile;

        public string green = "[color #00EB7E]";
        public string orange = "[color #FB9A50]";
        public string orange2 = "[color #FFA500]";
        public string white = "[color #FFFFFF]";

        public static Dictionary<string, string> ZonesDictionary = new Dictionary<string, string>();

        public bool EnableRads;
        public float RadLevelLow = 20f;
        public float RadLevelHight = 50f;
        public int TimeRad = 3;
        public int Radius;
        public int RadiusHightRad;

        public override string Name { get { return "RadZones"; } }
        public override string Author { get { return "Pompeyo & salva/juli"; } }
        public override string Description { get { return "Zonas Radioactivas"; } }
        public override Version Version { get { return new Version("1.0"); } }
        #endregion Variables

        #region Initialize
        public override void Initialize()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Zones.ini")))
                File.Create(Path.Combine(ModuleFolder, "Zones.ini"));
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                SettingsFile = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                SettingsFile.AddSetting("Settings", "EnableRads", "true");
                SettingsFile.AddSetting("Settings", "RadLevelLow", RadLevelLow.ToString());
                SettingsFile.AddSetting("Settings", "RadLevelHight", RadLevelHight.ToString());
                SettingsFile.AddSetting("Settings", "TimeRad", TimeRad.ToString());
                SettingsFile.Save();
            }
            else
            {
                SettingsFile = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                EnableRads = bool.Parse(SettingsFile.GetSetting("Settings", "EnableRads"));
                RadLevelLow = int.Parse(SettingsFile.GetSetting("Settings", "RadLevelLow"));
                RadLevelHight = int.Parse(SettingsFile.GetSetting("Settings", "RadLevelHight"));
                TimeRad = int.Parse(SettingsFile.GetSetting("Settings", "TimeRad"));
            }

            Fougerite.Hooks.OnCommand += OnCommand;
            Fougerite.Hooks.OnServerLoaded += OnServerLoaded;
        }

        #endregion Initialize

        #region Deinitialize
        public override void DeInitialize()
        {
            Fougerite.Hooks.OnCommand -= OnCommand;
            Fougerite.Hooks.OnServerLoaded -= OnServerLoaded;
        }
        #endregion Deinitialize

        private void OnServerLoaded()
        {
            RadTimer(TimeRad * 1000, null).Start();
            ReloadZonesJuli();
        }

        public void ReloadZonesJuli()
        {
            ZonesDictionary.Clear();
            ZonesFile = new IniParser(Path.Combine(ModuleFolder, "Zones.ini"));
            foreach (var section in ZonesFile.Sections)
            {
                foreach (var inikey in ZonesFile.EnumSection(section))
                {
                    var inivalue = ZonesFile.GetSetting(section, inikey);
                    ZonesDictionary.Add(inikey, inivalue);
                }
            }
            return;
        }
        public bool ExisteLaZona(string NombreDeLaZona)
        {
            bool existe = false;
            ZonesFile = new IniParser(Path.Combine(ModuleFolder, "Zones.ini"));
            foreach (var Seccion in ZonesFile.Sections)
            {
                if (Seccion == NombreDeLaZona)
                {
                    existe = true;
                    break;
                }
            }
            return existe;
        }

        public void OnCommand(Fougerite.Player pl, string cmd, string[] args)
        {
            if (!pl.Admin) { pl.Notice("✖", "Usted no tiene acceso a este comando", 5f); return; }
            if (cmd == "radzone")
            {
                if (args.Length == 0)
                {
                    pl.MessageFrom(Name, green + "---------- RadZones" + white + " by Pompeyo " + green + "----------");
                    pl.MessageFrom(Name, white + "/radzone set [zonename] [radius] - Create zone");
                    pl.MessageFrom(Name, white + "/radzone edit [zonename] [radius] - Edit radius of zone");
                    pl.MessageFrom(Name, white + "/radzone list - List all rad zones");
                    pl.MessageFrom(Name, white + "/radzone delete [zonename] - Delete rad zone");
                    pl.MessageFrom(Name, green + "--- --- --- --- --- --- --- --- --- --- --- --- ---");
                }
                else
                {
                    if (args[0] == "set")
                    {
                        if (args.Length >= 2)
                        {
                            CreateZone(pl, args[1], Convert.ToInt32(args[2]));
                        }
                        else
                            pl.MessageFrom("RadZones", orange + "Wrong usage. See /zone for help.");
                    }
                    else if (args[0] == "edit")
                    {
                        if (args.Length >= 2)
                        {
                            EditZone(pl, args[1], Convert.ToInt32(args[2]));
                        }
                        else
                            pl.MessageFrom("RadZones", orange + "Wrong usage. See /radzone for help.");
                    }
                    else if (args[0] == "list")
                    {
                        ListZones(pl);
                        RadTimer(TimeRad * 1000, null).Start();
                    }
                    else if (args[0] == "delete")
                    {
                        if (args.Length >= 2)
                        {
                            DeleteZone(pl, args[1]);
                        }
                        else
                            pl.MessageFrom("RadZones", orange + "Wrong usage. Write /radzone delete NAME");
                    }
                }
            }
        }

        public void CreateZone(Player pl, string ZoneName, int Radius)
        {
            //juli method
            ZonesFile = new IniParser(Path.Combine(ModuleFolder, "Zones.ini"));
            if (!ExisteLaZona(ZoneName))
            {
                ZonesFile.AddSetting(ZoneName, "Location", pl.Location.ToString());
                ZonesFile.AddSetting(ZoneName, "Radius", Radius.ToString());
                ZonesFile.Save();
                pl.MessageFrom(Name, green + "Looks like " + white + ZoneName + green + " is our new zone!");
            }
            else
            {
                pl.MessageFrom(Name, orange + "La zona: " + white + ZoneName + orange + "Ya existe !!");
            }

            ReloadZonesJuli();
            return;
        }

        public void DeleteZone(Fougerite.Player pl, string ZoneName)
        {
            //juli method
            ZonesFile = new IniParser(Path.Combine(ModuleFolder, "Zones.ini"));
            if (ExisteLaZona(ZoneName))
            {
                try
                {
                    ZonesFile.DeleteSetting(ZoneName, "Location");
                    ZonesFile.DeleteSetting(ZoneName, "Radius");
                    ZonesFile.Save();
                    pl.MessageFrom(Name, orange + "La zona: " + white + ZoneName + orange + " ha sido eliminada.");
                }
                catch (Exception ex)
                {
                    Debug.Log("Error al borrar zona: " + ex.ToString());
                    pl.MessageFrom(Name, "hubo un error al borrar");
                }
            }

            ReloadZonesJuli();
            return;
        }

        private void EditZone(Fougerite.Player pl, string ZoneName, int Radius)
        {
            //juli method
            DeleteZone(pl, ZoneName);
            CreateZone(pl, ZoneName, Radius);
            pl.MessageFrom(Name, green + " La zona: " + white + ZoneName + green + " ha sido editada.");
            ReloadZonesJuli();
            return;
        }

        private void ListZones(Fougerite.Player pl)
        {
            //juli method
            pl.MessageFrom(Name, green + "--- --- ---" + white + " Listado de Zonas " + green + "--- --- ---");
            ZonesFile = new IniParser(Path.Combine(ModuleFolder, "Zones.ini"));
            foreach (var section in ZonesFile.Sections)
            {
                var zona = section;
                var location = ZonesFile.GetSetting(section, "Location");
                var radius = ZonesFile.GetSetting(section, "Radius"); ;

                pl.MessageFrom(Name, "Zona: " + zona + " Loc:" + location + " Radius: " + radius);
            }
            pl.MessageFrom(Name, green + "--- --- --- --- --- --- --- --- --- --- --- --- ---");
        }

        public void TryGiveRad(Fougerite.Player pl)
        {
            if (pl.IsOnline && !pl.IsDisconnecting && pl.IsAlive)
            {
                foreach (var key in ZonesDictionary)
                {
                    try
                    {
                        Vector3 loc = Util.GetUtil().ConvertStringToVector3(key.Key);
                        int range = Convert.ToInt32(key.Value);

                        var PlayerDist = Util.GetUtil().GetVectorsDistance(pl.Location, loc);

                        if (PlayerDist < range)
                        {
                            pl.AddRads(RadLevelHight);
                            pl.InventoryNotice(RadLevelHight + " Rads");
                        }
                        else
                        {
                            pl.AddRads(RadLevelLow);
                            pl.InventoryNotice(RadLevelLow + " Rads");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(Name + " Error B:" + ex.ToString());
                        continue;
                    }
                }
            }
        }
        #region Timer
        public TimedEvent RadTimer(int timeoutDelay, Dictionary<string, object> args)
        {
            TimedEvent timedEvent = new TimedEvent(timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += CallBackRad;
            return timedEvent;
        }

        public void CallBackRad(TimedEvent e)
        {
            e.Kill();
            foreach (Fougerite.Player pl in Server.GetServer().Players)
            {
                try
                {
                    TryGiveRad(pl);
                }
                catch (Exception ex)
                {
                    Debug.Log(Name + " Error A:" + ex.ToString());
                }
            }
            RadTimer(TimeRad * 1000, null).Start();
        }
        #endregion Timer
    }
}
