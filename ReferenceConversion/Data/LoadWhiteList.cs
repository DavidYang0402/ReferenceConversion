using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReferenceConversion
{
    public static class WhitelistManager
    {
        private static List<WhitelistEntry> _whitelist;

        // 靜態建構子，確保載入一次白名單資料
        static WhitelistManager()
        {
            _whitelist = LoadWhitelist();
        }

        public static bool IsInAllowlist(string referenceName, out string version, out string guid)
        {
            version = string.Empty;
            guid = string.Empty;

            // 查找對應的白名單項目
            var entry = _whitelist?.FirstOrDefault(w => w.Name.Equals(referenceName, StringComparison.OrdinalIgnoreCase));


            if (entry != null)
            {
                version = entry.Version;
                guid = entry.Guid;

                return true;
            }

            return false;
        }

        private static List<WhitelistEntry> LoadWhitelist()
        {

            // 取得應用程式根目錄 (發佈後的位置)
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // 構建 Data 資料夾中的 AllowList.json 路徑
            string allowlistPath = Path.Combine(appDirectory, "Data", "AllowList.json");

            try
            {
                if (File.Exists(allowlistPath))
                {
                    // 讀取 JSON 檔案
                    string json;
                    using (var reader = new StreamReader(allowlistPath))
                    {
                        json = reader.ReadToEnd();
                    }

                    var whitelist = JsonConvert.DeserializeObject<List<WhitelistEntry>>(json);
                    return whitelist ?? new List<WhitelistEntry>();
                }
                else
                {
                    throw new FileNotFoundException($"白名單檔案不存在: {allowlistPath}");
                }
            }
            catch (Exception ex)
            {
                // 錯誤處理，顯示錯誤訊息
                Console.WriteLine($"載入白名單檔案時發生錯誤: {ex.Message}");
                return new List<WhitelistEntry>(); // 返回空列表，避免應用程式崩潰
            }
        }
    }

    public class WhitelistEntry
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }
    }
}