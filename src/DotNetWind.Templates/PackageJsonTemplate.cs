namespace DotNetWind.Templates;

public static class PackageJsonTemplate
{
    public static string GetDefault(string inputCssRelativePath, string outputCssRelativePath)
    {
        var input = inputCssRelativePath.Replace('\\', '/');
        var output = outputCssRelativePath.Replace('\\', '/');

        var document = new
        {
            scripts = new Dictionary<string, string>
            {
                ["tw:build"] = $"npx @tailwindcss/cli -i {input} -o {output}",
                ["tw:build:min"] = $"npx @tailwindcss/cli -i {input} -o {output} --minify",
                ["tw:watch"] = $"npx @tailwindcss/cli -i {input} -o {output} --watch"
            },
            devDependencies = new Dictionary<string, string>
            {
                ["@tailwindcss/cli"] = "latest",
                ["tailwindcss"] = "latest"
            }
        };

        return JsonSerializer.Serialize(document, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
