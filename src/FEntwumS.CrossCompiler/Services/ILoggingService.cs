﻿namespace FEntwumS.CrossCompiler.Services;

public interface ILoggingService
{
    public void Log(string message, bool showOutput = false);
    
    public void Error(string message, bool showOutput = true);
}