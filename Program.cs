using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace harunadev
{
    public class Configuration
    {
        [JsonPropertyName("language")]
        public required string Language { get; set; }
        [JsonPropertyName("skipCheckVRChatPhoto")]
        public bool SkipCheckVRChatPhoto { get; set; }
        [JsonPropertyName("skipCheckVRChatDirectory")]
        public bool SkipCheckVRChatDirectory { get; set; }
        [JsonPropertyName("overwriteExistingPhoto")]
        public bool OverwriteExistingPhoto { get; set; }
    }

    internal class VRCMetadataStripper
    {
        static Dictionary<string, Dictionary<string, string>> lang = new Dictionary<string, Dictionary<string, string>>()
        {
        { "en", new Dictionary<string, string >()
            {
                { "CurrentDirectory", "Current Directory: {0}"},
                { "CurrentDirectoryCheckSkipped", "Skipping VRChat Directory check..."},
                { "CurrentDirectoryMatchFail", "Current Directory do not match with VRChat Photo folder!"},
                { "VRChatPhotoDirectory", "VRChat Photo Directory: {0}"},
                { "IgnorePromptY", "Ignore [y]?: "},
                { "ProgramExit", "Press any key or close this window to exit."},
                { "DirectoryAccessDenied", "Access Denied while searching '{0}'"},
                { "ErrorOccurred", "An error occurred. Please capture the output below and send it to developer or create an issue in GitHub page."},
                { "ScanningInProgress", "Scanning all JPG, JPEG, PNGs..."},
                { "FilteringInProgress", "Filtering images..."},
                { "NoFilesToProcess", "All scanned files does not contain any metadata."},
                { "ProcessFileCount", "{0} file(s) will be processed."},
                { "ProcessOverwrite", "WARNING: All images will be overwritten."},
                { "ProcessClone", "All images will have a non-metadata clone with postfix '-nometadata'."},
                { "ProcessConfirmPrompt", "Proceed [Any key], Show All images as list [a], Overwrite All [o], Cancel [c]?: "},
                { "ProcessConfirmPromptOverwrite", "Proceed [Enter], Show All images as list [a], Clone All [o], Cancel [Any key]?: "},
                { "ProcessComplete", "Processing completed with {0} error(s)."},
            }
        },
        { "ko", new Dictionary<string, string>()
        {
            { "CurrentDirectory", "현재 디렉터리: {0}"},
            { "CurrentDirectoryCheckSkipped", "VRChat 디렉터리 확인을 건너뜁니다..."},
            { "CurrentDirectoryMatchFail", "현재 디렉터리가 VRChat 사진 폴더와 일치하지 않습니다!"},
            { "VRChatPhotoDirectory", "VRChat 사진 디렉터리: {0}"},
            { "IgnorePromptY", "무시 [y]?: "},
            { "ProgramExit", "아무 키나 누르거나 창을 닫아 종료하세요."},
            { "DirectoryAccessDenied", "'{0}' 검색 중 접근이 거부되었습니다"},
            { "ErrorOccurred", "오류가 발생했습니다. 아래 출력 내용을 캡처하여 개발자에게 보내거나 GitHub 페이지에 이슈를 등록해주세요."},
            { "ScanningInProgress", "JPG, JPEG, PNG 파일을 스캔 중..."},
            { "FilteringInProgress", "이미지 필터링 중..."},
            { "NoFilesToProcess", "스캔된 모든 파일에 메타데이터가 없습니다."},
            { "ProcessFileCount", "{0}개의 파일이 처리됩니다."},
            { "ProcessOverwrite", "경고: 모든 이미지가 덮어쓰기 됩니다."},
            { "ProcessClone", "모든 이미지가 '-nometadata' 접미사가 붙은 메타데이터 없는 복제본으로 생성됩니다."},
            { "ProcessConfirmPrompt", "진행 [아무 키], 전체 이미지 목록 보기 [a], 모두 덮어쓰기 [o], 취소 [c]?: "},
            { "ProcessConfirmPromptOverwrite", "진행 [Enter], 전체 이미지 목록 보기 [a], 모두 복제 [o], 취소 [아무 키]?: "},
            { "ProcessComplete", "{0}개의 오류와 함께 처리가 완료되었습니다."},
        }
        },
        { "ja", new Dictionary<string, string>()
        {
            { "CurrentDirectory", "現在のディレクトリ: {0}"},
            { "CurrentDirectoryCheckSkipped", "VRChatディレクトリの確認をスキップします..."},
            { "CurrentDirectoryMatchFail", "現在のディレクトリがVRChatの写真フォルダと一致しません！"},
            { "VRChatPhotoDirectory", "VRChat写真ディレクトリ: {0}"},
            { "IgnorePromptY", "無視する [y]?: "},
            { "ProgramExit", "何かキーを押すかウィンドウを閉じて終了してください。"},
            { "DirectoryAccessDenied", "'{0}' の検索中にアクセスが拒否されました"},
            { "ErrorOccurred", "エラーが発生しました。以下の出力をキャプチャして開発者に送るか、GitHubページにIssueを作成してください。"},
            { "ScanningInProgress", "JPG、JPEG、PNGをスキャン中..."},
            { "FilteringInProgress", "画像をフィルタリング中..."},
            { "NoFilesToProcess", "スキャンされたすべてのファイルにメタデータが含まれていません。"},
            { "ProcessFileCount", "{0} 件のファイルを処理します。"},
            { "ProcessOverwrite", "警告：すべての画像が上書きされます。"},
            { "ProcessClone", "すべての画像に '-nometadata' が付いたメタデータなしのコピーが作成されます。"},
            { "ProcessConfirmPrompt", "実行 [任意のキー], 全画像一覧表示 [a], 全て上書き [o], キャンセル [c]?: "},
            { "ProcessConfirmPromptOverwrite", "実行 [Enter], 全画像一覧表示 [a], 全てコピー [o], キャンセル [任意のキー]?: "},
            { "ProcessComplete", "{0} 件のエラーで処理が完了しました。"},
        }
        }
        };
        static Dictionary<string, string> currentLang;

        static string configPath = "VRCMetadataStripper.config.json";
        static Configuration config;
        static List<FileInfo> targetFiles = new();

        static void Main(string[] args)
        {
            config = LoadConfiguration();

            switch (config.Language)
            {
                case "en":
                    Console.WriteLine("Current Language: English");
                    break;
                case "ko":
                    Console.WriteLine("언어: 한국어");
                    break;
                case "ja":
                    Console.WriteLine("言語: 日本語");
                    break;
                default:
                    config.Language = "en";
                    Console.WriteLine("Unknown Language! Fallback to English.");
                    break;
            }
            currentLang = lang[config.Language];

            DirectoryInfo currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            Console.WriteLine(string.Format(currentLang["CurrentDirectory"], currentDirectory.FullName));

            if (!config.SkipCheckVRChatDirectory)
            {
                string targetPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") ?? "", "Pictures", "VRChat");
                if (currentDirectory.FullName != targetPath)
                {
                    Console.WriteLine(currentLang["CurrentDirectoryMatchFail"]);
                    Console.WriteLine(string.Format(currentLang["VRChatPhotoDirectory"], targetPath));
                    Console.Write(currentLang["IgnorePromptY"]);
                    ConsoleKeyInfo k = Console.ReadKey();
                    Console.WriteLine("");
                    if (k.Key != ConsoleKey.Y)
                    {
                        Console.WriteLine(currentLang["ProgramExit"]);
                        Console.ReadKey();
                        return;
                    }
                }
            } else
            {
                Console.WriteLine(currentLang["CurrentDirectoryCheckSkipped"]);
            }

            {
                Console.WriteLine(currentLang["ScanningInProgress"]);
                List<string> allImageFiles = GetImageFiles(currentDirectory.FullName, config.SkipCheckVRChatPhoto, Console.GetCursorPosition());
                Console.WriteLine("");
                Console.WriteLine(currentLang["FilteringInProgress"]);

                (int, int) cursor = Console.GetCursorPosition();
                int scannedFile = 0;

                foreach (string file in allImageFiles)
                {
                    ProgressBar(cursor, ((float)scannedFile + 1) / allImageFiles.Count);
                    if (HasAnyMetadata(file))
                    {
                        targetFiles.Add(new FileInfo(file));
                    }
                    scannedFile++;
                }

                Console.WriteLine("");

                if (targetFiles.Count > 0)
                {
                    Console.WriteLine(string.Format(currentLang["ProcessFileCount"], targetFiles.Count));

                    if (config.OverwriteExistingPhoto)
                    {
                        Console.WriteLine(currentLang["ProcessOverwrite"]);
                    } else
                    {
                        Console.WriteLine(currentLang["ProcessClone"]);
                    }
                    
                    bool overwrite = config.OverwriteExistingPhoto;

                    while (true)
                    {
                        if (overwrite)
                        {
                            Console.Write(currentLang["ProcessConfirmPromptOverwrite"]);
                            ConsoleKeyInfo l = Console.ReadKey();
                            Console.WriteLine("");

                            if (l.Key == ConsoleKey.Enter)
                            {
                                break;
                            }
                            else if (l.Key == ConsoleKey.A)
                            {
                                Console.WriteLine("");
                                foreach (FileInfo f in targetFiles) Console.WriteLine(f.Name);
                                Console.WriteLine("");
                            }
                            else if (l.Key == ConsoleKey.O)
                            {
                                overwrite = false; 
                                break;
                            }
                            else
                            {
                                Console.WriteLine(currentLang["ProgramExit"]);
                                Console.ReadKey();
                                return;
                            }
                        } else
                        {
                            Console.Write(currentLang["ProcessConfirmPrompt"]);
                            ConsoleKeyInfo l = Console.ReadKey();
                            Console.WriteLine("");

                            if (l.Key == ConsoleKey.C)
                            {
                                Console.WriteLine(currentLang["ProgramExit"]);
                                Console.ReadKey();
                                return;
                            }
                            else if (l.Key == ConsoleKey.A)
                            {
                                Console.WriteLine("");
                                foreach (FileInfo f in targetFiles) Console.WriteLine(f.Name);
                                Console.WriteLine("");
                            }
                            else if (l.Key == ConsoleKey.O)
                            {
                                overwrite = true;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    Console.WriteLine("");
                    cursor = Console.GetCursorPosition();
                    int errorCount = 0;

                    for (int i = 0; i < targetFiles.Count; i++)
                    {
                        ProgressBar(cursor, ((float)i + 1) / targetFiles.Count);
                        if (!OverwriteClone(targetFiles[i].FullName, overwrite))
                        {
                            errorCount++;
                        }
                    }

                    Console.WriteLine("");
                    Console.WriteLine(string.Format(currentLang["ProcessComplete"], errorCount));
                    Console.WriteLine(currentLang["ProgramExit"]);
                    Console.ReadKey();
                } else
                {
                    Console.WriteLine(currentLang["NoFilesToProcess"]);
                    Console.WriteLine(currentLang["ProgramExit"]);
                    Console.ReadKey();
                    return;
                }
            }
        }

        static void ProgressBar((int, int) cursor, float percent)
        {
            int filled = (int)(percent * 36);
            string bar = new string('#', filled).PadRight(36);
            Console.SetCursorPosition(cursor.Item1, cursor.Item2);
            Console.Write($"\r[{bar}] {percent:P0}");
        }

        static int ScanImageFileProgress = 0;
        public static List<string> GetImageFiles(string rootDirectory, bool skipVRChatPhoto, (int, int) cursor)
        {
            List<string> imageFileList = new List<string>();
            string[] imageExtensions = [".jpg", ".jpeg", ".png"];

            try
            {
                var files = Directory.GetFiles(rootDirectory)
                                     .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()));
                
                if (skipVRChatPhoto)
                {
                    files = files.Where(file => Path.GetFileName(file).StartsWith("VRChat_2"));
                }
                
                imageFileList.AddRange(files);
                ScanImageFileProgress += files.Count();

                {
                    Console.SetCursorPosition(cursor.Item1, cursor.Item2);
                    Console.Write(ScanImageFileProgress);
                }

                string[] subDirectories = Directory.GetDirectories(rootDirectory);

                foreach (string subDirectory in subDirectories)
                {
                    imageFileList.AddRange(GetImageFiles(subDirectory, skipVRChatPhoto, cursor));
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(string.Format(currentLang["DirectoryAccessDenied"], rootDirectory));
            }
            catch (Exception e)
            {
                Console.WriteLine(currentLang["ErrorOccurred"]);
                Console.WriteLine(e);
            }

            return imageFileList;
        }

        static bool HasAnyMetadata(string path)
        {
            using var image = Image.Load(path);
            var meta = image.Metadata;

            return
                (meta.ExifProfile?.Values.Count ?? 0) > 0 ||
                (meta.IccProfile != null) ||
                (meta.XmpProfile != null) ||
                (meta.GetPngMetadata()?.TextData?.Count > 0);
        }

        static bool OverwriteClone(string filePath, bool overwrite)
        {
            try
            {
                using var originalImage = Image.Load<Rgba32>(filePath);
                var cleanImage = originalImage.Clone();
                cleanImage.Metadata.ExifProfile = null;
                cleanImage.Metadata.IccProfile = null;
                cleanImage.Metadata.XmpProfile = null;
                var pngMetadata = cleanImage.Metadata.GetPngMetadata();
                pngMetadata.TextData.Clear();
                if (overwrite)
                {
                    cleanImage.Save(filePath);
                }
                else
                {
                    cleanImage.Save(Path.Combine(Path.GetDirectoryName(filePath) ?? "/", $"{Path.GetFileNameWithoutExtension(filePath)}-nometadata{Path.GetExtension(filePath)}"));
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(currentLang["ErrorOccurred"]);
                Console.WriteLine(e);

                return false;
            }
        }

        static Configuration LoadConfiguration()
        {
            Configuration conf;
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                conf = JsonSerializer.Deserialize<Configuration>(json);
            }
            else
            {
                conf = new Configuration
                {
                    Language = CultureInfo.CurrentUICulture.Name.Substring(0, 2),
                    SkipCheckVRChatPhoto = false,
                    SkipCheckVRChatDirectory = false,
                    OverwriteExistingPhoto = false,
                };
                SaveConfiguration(conf);
            }

            return conf ?? new Configuration() 
            { 
                Language = "en",
                SkipCheckVRChatPhoto = false,
                SkipCheckVRChatDirectory = false,
                OverwriteExistingPhoto = false,
            };
        }

        static void SaveConfiguration(Configuration config)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configPath, json);
        }
    }
}