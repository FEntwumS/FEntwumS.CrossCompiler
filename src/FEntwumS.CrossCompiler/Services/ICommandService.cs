

using OneWare.Essentials.Enums;

namespace FEntwumS.CrossCompiler.Services;

public interface ICommandService
{
    public Task<(bool success, string stdout, string stderr)> ExecuteCommandAsync(string toolPath,
        IReadOnlyList<string> args, string workingDirectory);
    
    public string getToolPath(string toolName);
    
    public string getCompileOptions();
}