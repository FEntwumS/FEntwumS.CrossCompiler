using OneWare.Essentials.Models;

namespace FEntwumS.CrossCompiler.Services;

public interface IToolchainService
{
    public void setDebugCheck(bool value);
    public void setWorkingInformation(IProjectFile chosenFile);
    public Task<bool> performCrossCompilerToolchain(string chosenToolchainId);

   // public void Test();
}