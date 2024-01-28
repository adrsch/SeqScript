using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public static class SaveUtils 
    {

        public static bool SaveExists(string name)
        {

            var folderPath = Path.Combine(FileUtil.GetUserDataPath(), Constants.SaveFolder);
            var path = Path.Combine(folderPath, $"{name}.json");
            return File.Exists(path);
        }

        public static void Save(string name)
        {
            Cvars.Current.SaveTime = DateTime.Now;
            Cvars.Current.Build = Constants.Build;

            var folderPath = FileUtil.GetUserDataPath();
            string path;
            if (name == Constants.AutosaveFile || name == Constants.OldAutosaveFile || name == Constants.OlderAutosaveFile)
            {
                path = Path.Combine(folderPath, Constants.AutosaveFile);
                if (File.Exists(path))
                {
                    var lastSave = File.ReadAllBytes(path);
                    var oldPath = Path.Combine(folderPath, Constants.OldAutosaveFile);
                    if (File.Exists(oldPath))
                    {
                        var oldSave = File.ReadAllBytes(oldPath);
                        var olderPath = Path.Combine(folderPath, Constants.OlderAutosaveFile);
                        File.WriteAllBytes(olderPath, oldSave);
                    }
                    File.WriteAllBytes(oldPath, lastSave);
                }
            }
            else
            {

                var a = Path.Combine(folderPath, Constants.SaveFolder);
                path = Path.Combine(a, $"{name}.json");
            }

            var json = JsonConvert.SerializeObject(Cvars.Current, Formatting.Indented, FileUtil.Serializer);
            File.WriteAllText(path, json);
        }


        public static void Load(string name)
        {
            var folderPath = FileUtil.GetUserDataPath();
            string path;
            if (name == Constants.AutosaveFile || name == Constants.OldAutosaveFile || name == Constants.OlderAutosaveFile)
            {
                path = Path.Combine(folderPath, name);
            }
            else
            {
                var a = Path.Combine(folderPath, Constants.SaveFolder);
                path = Path.Combine(a, $"{name}.json");
            }

            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var ob = JsonConvert.DeserializeObject<CvarMap>(text, FileUtil.Serializer);
                Cvars.SetCurrent(ob);
            }
        }

        public static void Load(CvarMap data)
        {
            Cvars.SetCurrent(data);
        }
        public static void Delete(SaveDataInfo data)
        {
            File.Delete(data.Path);
        }

        static CvarMap Read(string path)
        {
            var text = File.ReadAllText(path);
            var ob = JsonConvert.DeserializeObject<CvarMap>(text, FileUtil.Serializer);
            return ob;
        }

        public static IEnumerable<SaveDataInfo> GetAllSaves(bool includeAuto = true)
        {
            List<SaveDataInfo> saves = new List<SaveDataInfo>();

            var userdata = FileUtil.GetUserDataPath();
            if (!Directory.Exists(userdata))
            {
                Directory.CreateDirectory(userdata);
            }
            if (includeAuto)
            {
                if (File.Exists(Path.Combine(userdata, Constants.AutosaveFile)))
                    saves.Add(new SaveDataInfo
                    {
                        Name = "Autosave",
                        Path = Path.Combine(userdata, Constants.AutosaveFile),
                        Data = Read(Path.Combine(userdata, Constants.AutosaveFile))
                    });
                if (File.Exists(Path.Combine(userdata, Constants.OldAutosaveFile)))
                    saves.Add(new SaveDataInfo
                    {
                        Name = "Old Autosave",
                        Path = Path.Combine(userdata, Constants.OldAutosaveFile),
                        Data = Read(Path.Combine(userdata, Constants.OldAutosaveFile))
                    });
                if (File.Exists(Path.Combine(userdata, Constants.OlderAutosaveFile)))
                    saves.Add(new SaveDataInfo
                    {
                        Name = "Older Autosave",
                        Path = Path.Combine(userdata, Constants.OlderAutosaveFile),
                        Data = Read(Path.Combine(userdata, Constants.OlderAutosaveFile))
                    });
            }

            var savedata = Path.Combine(userdata, Constants.SaveFolder);
            if (!Directory.Exists(savedata))
            {
                Directory.CreateDirectory(savedata);
            }
            foreach (var f in Directory.GetFiles(savedata, "*.json", SearchOption.TopDirectoryOnly))
            {
                saves.Add(new SaveDataInfo
            {Name = Path.GetFileName(f), Data = Read(f), Path = f,
                });
            }

            return saves.OrderByDescending(x => x.Data.SaveTime).Where(x => x.Data.Build > 0);
        }

        public static SaveDataInfo GetLastSave()
        {
            return GetAllSaves().FirstOrDefault();
        }

        public static void NewSave()
        {
            CameraPositioner.Inst.ClearCurrent();
            ActorRegistry.ResetAll();
            Cvars.ResetAll();
            EventManager.Raise(new NewSaveEvent());
            ScriptRunner.Exec("init-save");
            ActorSpeciesRegistry.S.DoResetSpawns();
        }
    }

    public class NewSaveEvent
    {

    }


    public class SaveDataInfo
    {
        public string Name;
        public CvarMap Data;
        public string Path;
    }
}