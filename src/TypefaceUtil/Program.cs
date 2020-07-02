﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using TypefaceUtil.OpenType;

namespace TypefaceUtil
{
    class Settings
    {
        // Input
        public FileInfo[]? InputFiles { get; set; }
        public DirectoryInfo? InputDirectory { get; set; }
        public string Pattern { get; set; } = "*.ttf";
        public string? FontFamily { get; set; }
        // Output
        public DirectoryInfo? OutputDirectory { get; set; }
        // Png Export
        public bool PngExport { get; set; } = false;
        public float PngTextSize { get; set; } = 20f;
        public int PngCellSize { get; set; } = 40;
        public int PngColumns { get; set; } = 20;
        // Svg Export
        public bool SvgExport { get; set; } = false;
        public float SvgTextSize { get; set; } = 16f;
        public string SvgPathFill { get; set; } = "black";
        // Xaml Export
        public bool XamlExport { get; set; } = false;
        public float XamlTextSize { get; set; } = 16f;
        public string XamlBrush { get; set; } = "Black";
        // Other
        public bool Quiet { get; set; }
    }

    class Program
    {
        static void Log(string message)
        {
            Console.WriteLine(message);
        }

        static void Error(Exception ex)
        {
            Log($"{ex.Message}");
            Log($"{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Error(ex.InnerException);
            }
        }

        static void GetFiles(DirectoryInfo directory, string pattern, List<FileInfo> paths)
        {
            var files = Directory.EnumerateFiles(directory.FullName, pattern);
            if (files != null)
            {
                foreach (var path in files)
                {
                    paths.Add(new FileInfo(path));
                }
            }
        }

        static void Run(Settings settings)
        {
            var paths = new List<FileInfo>();

            if (settings.InputFiles != null)
            {
                foreach (var file in settings.InputFiles)
                {
                    paths.Add(file);
                }
            }

            if (settings.InputDirectory != null)
            {
                var directory = settings.InputDirectory;
                var pattern = settings.Pattern;
                GetFiles(directory, pattern, paths);
            }

            if (settings.OutputDirectory != null && !string.IsNullOrEmpty(settings.OutputDirectory.FullName))
            {
                if (!Directory.Exists(settings.OutputDirectory.FullName))
                {
                    Directory.CreateDirectory(settings.OutputDirectory.FullName);
                }
            }

            for (int i = 0; i < paths.Count; i++)
            {
                var inputPath = paths[i];
                using var typeface = SKTypeface.FromFile(inputPath.FullName);
                if (typeface != null)
                {
                    var characterMaps = Read(typeface);
                    Export(settings, characterMaps, typeface);
                }
            }

            var fontFamily = settings.FontFamily;
            if (!string.IsNullOrEmpty(fontFamily))
            {
                using var typeface = SKTypeface.FromFamilyName(fontFamily);
                if (typeface != null)
                {
                    var characterMaps = Read(typeface);
                    Export(settings, characterMaps, typeface);
                }
            }
        }

        static void Export(Settings settings, List<CharacterMap> characterMaps, SKTypeface typeface)
        {
            foreach (var characterMap in characterMaps)
            {
                if (characterMap.CharacterToGlyphMap != null)
                {
                    if (settings.PngExport)
                    {
                        var outputPath = $"charmap_({typeface.FamilyName})_{characterMap.Name}.png";
                        if (settings.OutputDirectory != null && !string.IsNullOrEmpty(settings.OutputDirectory.FullName))
                        {
                            outputPath = Path.Combine(settings.OutputDirectory.FullName, outputPath);
                        }
                        using var stream = File.OpenWrite(outputPath);
                        CharacterMapPngExporter.Save(characterMap.CharacterToGlyphMap, typeface, settings.PngTextSize, settings.PngCellSize, settings.PngColumns, stream);
                    }

                    if (settings.SvgExport)
                    {
                        var outputPath = $"charmap_({typeface.FamilyName})_{characterMap.Name}.svg.txt";
                        if (settings.OutputDirectory != null && !string.IsNullOrEmpty(settings.OutputDirectory.FullName))
                        {
                            outputPath = Path.Combine(settings.OutputDirectory.FullName, outputPath);
                        }
                        using var streamWriter = File.CreateText(outputPath);
                        CharacterMapSvgExporter.Save(characterMap.CharacterToGlyphMap, typeface, settings.SvgTextSize, settings.SvgPathFill, streamWriter);
                    }

                    if (settings.XamlExport)
                    {
                        var outputPath = $"charmap_({typeface.FamilyName})_{characterMap.Name}.xaml.txt";
                        if (settings.OutputDirectory != null && !string.IsNullOrEmpty(settings.OutputDirectory.FullName))
                        {
                            outputPath = Path.Combine(settings.OutputDirectory.FullName, outputPath);
                        }
                        using var streamWriter = File.CreateText(outputPath);
                        CharacterMapXamlExporter.Save(characterMap.CharacterToGlyphMap, typeface, settings.XamlTextSize, settings.XamlBrush, streamWriter);
                    }
                }
            }
        }

        static List<CharacterMap> Read(SKTypeface typeface)
        {
            var cmap = typeface.GetTableData(TableReader.GetIntTag("cmap"));
            var characterMaps = TableReader.ReadCmapTable(cmap);
            return characterMaps;
        }

        static async Task<int> Main(string[] args)
        {
            // Input

            var optionInputFiles = new Option(new[] { "--inputFiles", "-f" }, "The relative or absolute path to the input files")
            {
                Argument = new Argument<FileInfo[]?>()
            };

            var optionInputDirectory = new Option(new[] { "--inputDirectory", "-d" }, "The relative or absolute path to the input directory")
            {
                Argument = new Argument<DirectoryInfo?>()
            };

            var optionPattern = new Option(new[] { "--pattern", "-p" }, "The search string to match against the names of files in the input directory")
            {
                Argument = new Argument<string?>(getDefaultValue: () => "*.ttf")
            };

            var optionFontFamily = new Option(new[] { "--fontFamily" }, "The input font family")
            {
                Argument = new Argument<string?>()
            };

            // Output

            var optionOutputDirectory = new Option(new[] { "--outputDirectory", "-o" }, "The relative or absolute path to the output directory")
            {
                Argument = new Argument<DirectoryInfo?>()
            };

            // Png Export

            var optionPngExport = new Option(new[] { "--pngExport", "--png" }, "Export text as Png")
            {
                Argument = new Argument<bool>()
            };

            var optionPngTextSize = new Option(new[] { "--pngTextSize" }, "Png text size")
            {
                Argument = new Argument<float>(getDefaultValue: () => 20f)
            };

            var optionPngCellSize = new Option(new[] { "--pngCellSize" }, "Png cell size")
            {
                Argument = new Argument<int>(getDefaultValue: () => 40)
            };

            var optionPngColumns = new Option(new[] { "--pngColumns" }, "Png number of columns")
            {
                Argument = new Argument<int>(getDefaultValue: () => 20)
            };

            // Svg Export

            var optionSvgExport = new Option(new[] { "--svgExport", "--svg" }, "Export text as Svg")
            {
                Argument = new Argument<bool>()
            };

            var optionSvgTextSize = new Option(new[] { "--svgTextSize" }, "Svg text size")
            {
                Argument = new Argument<float>(getDefaultValue: () => 16f)
            };

            var optionSvgPathFill = new Option(new[] { "--svgPathFill" }, "Svg path fill")
            {
                Argument = new Argument<string>(getDefaultValue: () => "black")
            };

            // Xaml Export

            var optionXamlExport = new Option(new[] { "--xamlExport", "--xaml" }, "Export text as Xaml")
            {
                Argument = new Argument<bool>()
            };

            var optionXamlTextSize = new Option(new[] { "--xamlTextSize" }, "Xaml text size")
            {
                Argument = new Argument<float>(getDefaultValue: () => 16f)
            };

            var optionXamlBrush = new Option(new[] { "--xamlBrush" }, "Xaml brush")
            {
                Argument = new Argument<string>(getDefaultValue: () => "Black")
            };

            // Other

            var optionQuiet = new Option(new[] { "--quiet" }, "Set verbosity level to quiet")
            {
                Argument = new Argument<bool>()
            };

            var rootCommand = new RootCommand()
            {
                Description = "An OpenType typeface utilities."
            };

            // Input
            rootCommand.AddOption(optionInputFiles);
            rootCommand.AddOption(optionInputDirectory);
            rootCommand.AddOption(optionPattern);
            rootCommand.AddOption(optionFontFamily);
            // Output
            rootCommand.AddOption(optionOutputDirectory);
            // Png Export
            rootCommand.AddOption(optionPngExport);
            rootCommand.AddOption(optionPngTextSize);
            rootCommand.AddOption(optionPngCellSize);
            rootCommand.AddOption(optionPngColumns);
            // Svg Export
            rootCommand.AddOption(optionSvgExport);
            rootCommand.AddOption(optionSvgTextSize);
            rootCommand.AddOption(optionSvgPathFill);
            // Xaml Export
            rootCommand.AddOption(optionXamlExport);
            rootCommand.AddOption(optionXamlTextSize);
            rootCommand.AddOption(optionXamlBrush);
            // Other
            rootCommand.AddOption(optionQuiet);

            rootCommand.Handler = CommandHandler.Create((Settings settings) =>
            {
                try
                {
                    Run(settings);
                }
                catch (Exception ex)
                {
                    if (settings.Quiet == false)
                    {
                        Error(ex);
                    }
                }
            });

            return await rootCommand.InvokeAsync(args);
        }
    }
}