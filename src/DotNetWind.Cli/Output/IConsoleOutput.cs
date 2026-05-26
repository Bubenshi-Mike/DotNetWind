namespace DotNetWind.Cli.Output;

public interface IConsoleOutput
{
    void WriteHeader(string title);
    void WriteSuccess(string message);
    void WriteWarning(string message);
    void WriteError(string message);
    void WriteInfo(string message);
    void WriteAction(string message);
    void WriteLine(string message = "");
    void WriteTable(IEnumerable<(string Label, string Value)> rows);
    void WriteRule();
}
