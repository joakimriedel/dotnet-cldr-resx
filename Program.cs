using System;
using System.Resources;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using System.CommandLine.Parsing;
using System.Text.Json;
using System.Threading;

namespace Monotio.DotnetCldrResx
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // source option
            var optionSource = new Option<string>(
                    new[] { "--source", "-s" },
                    "The source section of cldr-localenames-modern to load resources from"
                    );
            optionSource.AddSuggestions("languages", "territories");
            optionSource.IsRequired = true;

            // languages option
            var optionLanguages = new Option<string[]>(
                    new[] { "--languages", "-l" },
                    "Space separated list of two-letter ISO 639-1 languages codes to generate resx files for"
                    );
            optionLanguages.IsRequired = true;

            // output option
            var optionOutput = new Option<string>(
                    new[] { "--output", "-o" },
                    "The output path for resx file; Resources/_Languages would output Resources/_Languages.<language>.resx");
            optionOutput.IsRequired = true;

            // force option
            var optionForce = new Option<bool>(
                    new[] { "--force", "-f" },
                    "Forces overwriting the output path if it already exists"
                    );
            optionForce.IsRequired = false;

            // cldr path option
            var optionCldrPath = new Option<string>(
                "--cldr-path",
                getDefaultValue: () => "node_modules\\cldr-localenames-modern",
                description: "Path to cldr-localenames-modern folder installed by 'npm i cldr-localenames-modern'"
            );

            var rootCommand = new RootCommand
            {
                optionSource,
                optionLanguages,
                optionOutput,
                optionForce,
                optionCldrPath
            };

            rootCommand.Description = "Generates resx files for the specified languages based on CLDR data from cldr-localenames-modern.";

            rootCommand.Handler = CommandHandler.Create<string, string[], string, bool, string, CancellationToken>(WriteResx);

            // Parse the incoming args and invoke the handler
            return await rootCommand.InvokeAsync(args);
        }

        // NOTE: the parameters of the handler method are matched according to the names of the options
        static async Task WriteResx(string source, string[] languages, string output, bool force, string cldrPath,
            CancellationToken cancellationToken)
        {
            foreach (var language in languages)
            {
                var inputPath = Path.Combine(cldrPath, $"main\\{language}\\{source}.json");
                if (!File.Exists(inputPath))
                {
                    throw new ApplicationException($"{inputPath}: no such file");
                }

                var outputPath = Path.ChangeExtension(output, $".{language}.resx");

                if (File.Exists(outputPath) && !force)
                {
                    throw new ApplicationException($"{outputPath}: file exists and --force not set");
                }

                var cldrJson = await File.ReadAllTextAsync(inputPath, cancellationToken);
                using var cldrDocument = JsonDocument.Parse(cldrJson);

                using ResXResourceWriter resx = new ResXResourceWriter(outputPath);

                var sourceElement = cldrDocument.RootElement.GetProperty("main")
                    .GetProperty(language)
                    .GetProperty("localeDisplayNames")
                    .GetProperty(source);

                foreach (var item in sourceElement.EnumerateObject())
                {
                    resx.AddResource(item.Name, item.Value.GetString());
                }
            }
        }
    }
}
