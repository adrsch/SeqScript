// MIT License

using Newtonsoft.Json;
using Stride.Core.IO;
using Stride.Engine.Network;

namespace SEQ.Script.Core
{
    public static class FileUtil
    {
        public static JsonSerializerSettings Serializer = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        /*
        public static Vector3 RemoveOriginFix(this Vector3 pos) => pos + SECTR_FloatingPointFix.Instance.totalOffset;
        public static Vector3 ApplyOriginFix(this Vector3 pos) => pos - SECTR_FloatingPointFix.Instance.totalOffset;

        public static bool TryDamage(Component obj, int dmg)
        {
            if (obj.GetComponent<IIntDamageable>() is IIntDamageable iid)
            {
                iid.TakeDamage(dmg);
                return true;
            }
            if (obj.GetComponentInParent<IIntDamageable>() is IIntDamageable _iid)
            {
                _iid.TakeDamage(dmg);
                return true;
            }
            if (obj.GetComponentInParent<HerdSimCore>() is HerdSimCore hsc)
            {
                hsc.Damage(dmg);
                return true;
            }
            return false;
        }*/

        public static string GetUserDataPath()
        {
            string path;
            path = $"/roaming/SEQ/";
            if (!VirtualFileSystem.DirectoryExists(path))
                VirtualFileSystem.CreateDirectory(path);
            return path;
        }
        public static string GetUserPrefsFilePath()
        {
            string path;
            if (!VirtualFileSystem.DirectoryExists("/local/SEQ/"))
                VirtualFileSystem.CreateDirectory("/local/SEQ/");
            path = "/local/SEQ/prefs.seq";
            return path;
        }

        public static string GetGameDataPath()
        {
            string path;
            //path = Path.Combine($"/binary/", "GAMEDATA");
            path = "/data/";
            if (!VirtualFileSystem.DirectoryExists(path))
                Logger.Log(Channel.Data, LogPriority.Error, "Cant find game data dir");

            return path;
        }
        public static string GetLogPath()
        {
            string path;
            path = $"/local/SEQ/Logs/";
            if (!VirtualFileSystem.DirectoryExists(path))
                VirtualFileSystem.CreateDirectory(path);
            return path;
        }

        public static string ReadDataFile(string file)
        {

            var path = GetDataFilePath(file);
            if (VirtualFileSystem.FileExists(path))
            {
                var s = VirtualFileSystem.OpenStream(path, VirtualFileMode.Open, VirtualFileAccess.Read);

                var t = s.ReadStringAsync();
                t.RunSynchronously();
                return t.Result;

            }
            Logger.Log(Channel.Data, LogPriority.Error, $"Utils.ReadDataFile: Could not find data file {file}");
            return "";
        }
        public static async Task<string> ReadDataFileAsync(string file)
        {
            var path = GetDataFilePath(file);
            return await ReadFileAsync(path);
        }
        public static async Task<string> ReadFileAsync(string path)
        {


            if (VirtualFileSystem.FileExists(path))
            {
                var socket = await VirtualFileSystem.OpenStreamAsync(path, VirtualFileMode.Open, VirtualFileAccess.Read);

                //    var bufferSize = await socket.Read7BitEncodedInt();
                var buffer = new byte[65536];
                var size = buffer.Length;
                var offset = 0;
                while (size > 0)
                {
                    int read = await socket.ReadAsync(buffer, offset, size).ConfigureAwait(false);
                    if (read == 0)
                    {
                        var asS = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        return asS;
                    }
                    // throw new IOException("Socket closed");
                    size -= read;
                    offset += read;
                }
                return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                // await s.ReadAllAsync( buffer, 0, buffer.Length).ConfigureAwait(false);
                // return buffer.ToString();
                //    return await s.ReadStringAsync();

            }
            Logger.Log(Channel.Data, LogPriority.Error, $"Utils.ReadFileAsync: Could not find data file {path}");
            return "";
        }

        public static bool DataFileExists(string file)
        {
            var path = GetDataFilePath(file);
            if (VirtualFileSystem.FileExists(path))
            {
                return true;
            }
            return false;
        }

        public static bool UserPrefsFileExists()
        {
            var path = GetUserPrefsFilePath();
            var full = VirtualFileSystem.GetAbsolutePath(path);
            if (VirtualFileSystem.FileExists(path))
            {
                return true;
            }
            return false;
        }

        public static async Task<string> GetUserPrefs()
        {
            if (UserPrefsFileExists())
            {
                return await ReadFileAsync(GetUserPrefsFilePath());
            }
            return null;
        }


        public static async Task WriteFileAsync(string path, string contents)
        {
            var d = Path.GetDirectoryName(path);
            if (!VirtualFileSystem.DirectoryExists(d))
                VirtualFileSystem.CreateDirectory(d);
            if (VirtualFileSystem.FileExists(path))
            {
                var socket = await VirtualFileSystem.OpenStreamAsync(path, VirtualFileMode.Truncate, VirtualFileAccess.Write).ConfigureAwait(false); ;

                await socket.WriteStringAsync(contents);
            }
            else
            {
                var socket = await VirtualFileSystem.OpenStreamAsync(path, VirtualFileMode.OpenOrCreate, VirtualFileAccess.Write).ConfigureAwait(false); ;

                await socket.WriteStringAsync(contents);
            }
        }

        public static async Task WriteUserPrefs(string p)
        {
            await WriteFileAsync(GetUserPrefsFilePath(), p);
        }

        static string GetDataFilePath(string file)
        {
            var path = System.IO.Path.Combine(GetGameDataPath(), file);
            if (VirtualFileSystem.FileExists(path)) return path;
            if (VirtualFileSystem.FileExists($"{path}.seq"))
                return $"{path}.seq";
            if (VirtualFileSystem.FileExists($"{path}.json"))
                return $"{path}.json";

            return path;
        }
    }
}
