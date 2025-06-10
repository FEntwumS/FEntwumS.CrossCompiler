using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using OneWare.Essentials.Services;
using Prism.Ioc;
using OneWare.Essentials.Enums;
using FEntwumS.CrossCompiler.Assistents;


namespace FEntwumS.CrossCompiler.Services;

public class CommandService : ICommandService
{
    //Attribute
    private readonly IChildProcessService childProcessService;
    private readonly ILoggingService logger;
    private readonly ISettingsService settingsService;

    //Constructor
    public CommandService()
    {
        childProcessService = TransferredContainerProvider.getService<IChildProcessService>();
        logger = TransferredContainerProvider.getService<ILoggingService>();
        settingsService = TransferredContainerProvider.getService<ISettingsService>();
    }
    
    /*
     Description: Methode for executing commands of console tools
     
     */
    public async Task<(bool success, string stdout, string stderr)> ExecuteCommandAsync(string toolPath,
        IReadOnlyList<string> args, string workingDirectory)
    {
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;

        (success, _) = await childProcessService.ExecuteShellAsync(toolPath, args, workingDirectory, "", AppState.Idle,
            false,
            x =>
            {
                if (x.Contains("error:") || x.Contains("fatal error:"))
                {
                    logger.Error(x);
                    return false;
                }

                stdout += x + "\n";
                return true;
            },
            x =>
            {
                if (x.Contains("error:") || x.Contains("fatal error:"))
                {
                    logger.Error(x);
                    return false;
                }

                stderr += x + "\n";
                logger.Log(x);
                return true;
            });

        return (success, stdout, stderr);
    }
    
    /*
     Description: Methode for getting complete path of tool
     
     */
    public string getToolPath(string toolName)
    {
        string toolPath = String.Empty;
        settingsService.GetSettingObservable<string>(Constants.pathSettingKey)
            .Subscribe(x => toolPath = Path.Combine(x, "bin", toolName)); 
        return toolPath;
    }

    public string getCompileOptions()
    {
        string compileOptions = string.Empty;
        List<string> keys = new List<string>();
        keys.Add(Constants.architectureSettingKey);
        keys.Add(Constants.abiSettingKey);
        keys.Add(Constants.optionSettingKey);
        foreach (var key in keys)
        {
            settingsService.GetSettingObservable<string>(key).Subscribe(x => compileOptions = string.Concat(compileOptions,x," "));  
        }
        return compileOptions;
    }
}
    
