using Spectre.Console;

namespace DotNetWind.Cli.Output;

public sealed class SpectreConsoleOutput : IConsoleOutput
{
    public void WriteHeader(string title)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold cyan]{Markup.Escape(title)}[/]").LeftJustified());
        AnsiConsole.WriteLine();
    }

    public void WriteSuccess(string message) =>
        AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(message)}");

    public void WriteWarning(string message) =>
        AnsiConsole.MarkupLine($"[yellow]![/] {Markup.Escape(message)}");

    public void WriteError(string message) =>
        AnsiConsole.MarkupLine($"[red]✗[/] {Markup.Escape(message)}");

    public void WriteInfo(string message) =>
        AnsiConsole.MarkupLine($"[dim]{Markup.Escape(message)}[/]");

    public void WriteAction(string message) =>
        AnsiConsole.MarkupLine($"[blue]→[/] {Markup.Escape(message)}");

    public void WriteLine(string message = "") =>
        AnsiConsole.WriteLine(message);

    public void WriteTable(IEnumerable<(string Label, string Value)> rows)
    {
        var table = new Table().BorderColor(Color.Grey).AddColumn("[bold]Property[/]").AddColumn("Value");
        foreach (var (label, value) in rows)
            table.AddRow(Markup.Escape(label), Markup.Escape(value));
        AnsiConsole.Write(table);
    }

    public void WriteRule() =>
        AnsiConsole.Write(new Rule().RuleStyle(Style.Parse("dim")));
}
