using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Collections.Generic;

namespace PDF_RENAMER
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("使用方法: Program [/debug] <pdfFile1> <pdfFile2> ...");
                return;
            }

            bool debugMode = args[0].Equals("/debug", StringComparison.OrdinalIgnoreCase);
            string[] pdfPaths = debugMode ? args.Skip(1).ToArray() : args; // /debug が指定された場合はそれ以降の引数をPDFファイルとして扱う

            string patternsFolderPath = $"patternFolder";

            // パターンファイルをフォルダから読み込み
            var patternFiles = Directory.GetFiles(patternsFolderPath, "*.xml");
            var patternsAndOrders = new List<(List<(string Keyword, string Start, string End)> Patterns, List<string> Order, string Title)>();

            foreach (var patternFile in patternFiles)
            {
                var (patterns, order, title) = LoadPatternsAndOrderFromXml(patternFile);
                patternsAndOrders.Add((patterns, order, title));
            }

            // 処理済みのファイルを追跡するためのセット
            var processedFiles = new HashSet<string>();


            foreach (var pdfPath in pdfPaths)
            {
                if (!File.Exists(pdfPath))
                {
                    Console.WriteLine($"PDFファイル '{pdfPath}' が見つかりません。スキップします。");
                    continue;
                }

                try
                {
                    // リネーム処理済みファイルをスキップ
                    if (processedFiles.Contains(pdfPath))
                    {
                        Console.WriteLine($"ファイル '{pdfPath}' は既に処理済みです。スキップします。");
                        continue;
                    }

                    // PDFからテキストを抽出
                    string extractedText = ExtractTextFromPdf(pdfPath);

                    // デバッグモードがオンの場合はテキストをファイルに出力
                    if (debugMode)
                    {
                        string debugFilePath = Path.Combine(Path.GetDirectoryName(pdfPath), Path.GetFileNameWithoutExtension(pdfPath) + ".txt");
                        File.WriteAllText(debugFilePath, extractedText);
                        Console.WriteLine($"デバッグ用にテキストファイル '{debugFilePath}' を作成しました。");
                        continue; // デバッグモードではリネーム処理をスキップ
                    }

                    bool anyPatternMatched = false;

                    foreach (var (patterns, order, title) in patternsAndOrders)
                    {

                        // 各パターンに対してテキストを検索し、結果をディクショナリに格納
                        var extractedParts = new Dictionary<string, string>();
                        bool allPatternsMatched = true;

                        foreach (var pattern in patterns)
                        {
                            Console.WriteLine($"キーワード '{pattern.Keyword}' にマッチする文字列:");

                            // 正規表現パターンを作成 (非貪欲マッチを使用)
                            //string regexPattern = $"{Regex.Escape(pattern.Start)}(.*?){Regex.Escape(pattern.End)}";

                            string regexPattern = $"{pattern.Start}(.*?){pattern.End}";
                            Match match = Regex.Match(extractedText, regexPattern);

                            if (match.Success)
                            {                                string extractedString = match.Groups[1].Value.Trim();
                                extractedParts[pattern.Keyword] = extractedString;
                                Console.WriteLine(extractedString);
                            }
                            else
                            {
                                Console.WriteLine("マッチする文字列はありませんでした。");
                                allPatternsMatched = false; // パターンのいずれかが一致しなかった場合
                            }
                            Console.WriteLine();
                        }
                        if (allPatternsMatched)
                        {
                            anyPatternMatched = true;
                            // 定義された順序で抽出した文字列を連結
                            var orderedParts = order.Select(keyword => extractedParts.ContainsKey(keyword) ? extractedParts[keyword] : string.Empty);

                            string newFileName = string.Join("_", title, string.Join("_", orderedParts));

                            // ファイル名として使用できない文字を置換
                            newFileName = ReplaceInvalidFileNameChars(newFileName);

                            // 新しいファイル名を持つファイルが既に存在するか確認
                            string newFilePath = Path.Combine(Path.GetDirectoryName(pdfPath), newFileName + ".pdf");
                            if (File.Exists(newFilePath))
                            {
                                Console.WriteLine($"新しいファイル名 '{newFileName}.pdf' は既に存在します。リネーム処理をスキップします。");
                            }
                            else
                            {
                                // ファイル名が存在しない場合、リネーム処理を行う
                                File.Move(pdfPath, newFilePath);
                                Console.WriteLine($"PDFファイルが次の名前にリネームされました: {newFileName}.pdf");

                                // リネーム処理済みのファイルをセットに追加
                                processedFiles.Add(newFilePath);
                            }
                            break; // 一つのパターンファイルで処理が完了したら次のPDFファイルへ
                        }
                        else
                        {
                            Console.WriteLine("全てのパターンにマッチしなかったため、リネーム処理をスキップします。");
                        }
                    }
                    if (!anyPatternMatched)
                    {
                        Console.WriteLine("いずれのパターンファイルにもマッチしなかったため、リネーム処理をスキップします。");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"エラー: {ex.Message}");
                }

            }
        }

        // XMLファイルからパターンと連結順序を読み込む
        static (List<(string Keyword, string Start, string End)> Patterns, List<string> Order, string Title) LoadPatternsAndOrderFromXml(string xmlPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);

            // パターンを読み込む
            XmlNodeList patternNodes = doc.SelectNodes("//Pattern");
            var patterns = new List<(string Keyword, string Start, string End)>();

            for (int i = 0; i < patternNodes.Count; i++)
            {
                string keyword = patternNodes[i]["Keyword"].InnerText;
                string start = patternNodes[i]["Start"].InnerText;
                string end = patternNodes[i]["End"].InnerText;
                patterns.Add((keyword, start, end));
            }

            // 連結順序を読み込む
            XmlNodeList orderNodes = doc.SelectNodes("//Order/Keyword");
            var order = new List<string>();

            foreach (XmlNode node in orderNodes)
            {
                order.Add(node.InnerText);
            }
            // タイトルを読み込む
            string title = doc.SelectSingleNode("//Title")?.InnerText ?? "NoTitle";

            return (patterns, order, title);
        }

        // PDFからテキストを抽出
        static string ExtractTextFromPdf(string pdfPath)
        {
            using (PdfReader reader = new PdfReader(pdfPath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                StringWriter text = new StringWriter();
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    PdfPage page = pdfDoc.GetPage(i);
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string pageContent = PdfTextExtractor.GetTextFromPage(page, strategy);
                    text.WriteLine(pageContent);
                }
                return text.ToString();
            }
        }

        // ファイル名として使用できない文字を置換する
        static string ReplaceInvalidFileNameChars(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }
}
