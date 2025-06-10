using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using OneWare.Essentials.Services;
using FEntwumS.CrossCompiler.Assistents;
using Prism.Ioc;
using TextMateSharp.Themes;

namespace FEntwumS.CrossCompiler.Services;

public class LoggingService : ILoggingService
{
    private readonly ILogger logger;

    private const ConsoleColor LogMessageConsoleColor = ConsoleColor.Cyan;
    private static readonly IBrush LogMessageBrush = (Application.Current!.GetResourceObservable("ThemeAccentBrush") as IBrush)!;

    private readonly string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!;

    public LoggingService()
    {
        logger = TransferredContainerProvider.getService<ILogger>();
    }

    public void Log(string message, bool showOutput = false)
    {
        logger.Log("["+ assemblyName + "]: " + message, LogMessageConsoleColor, showOutput, LogMessageBrush);
    }

    public void Error(string message, bool showOutput = true)
    {
        logger.Error("["+ assemblyName + "]: " + message, null, showOutput);
    }
}