using System.Text;
using Avalonia.Xaml.Interactions.Custom;
using DynamicData;
using OneWare.Essentials.Enums;
using OneWare.Essentials.Extensions;
using OneWare.Essentials.Models;
using OneWare.Essentials.Services;
using FEntwumS.CrossCompiler.Assistents;
using FEntwumS.CrossCompiler.ViewModel;
using Prism.Ioc;

namespace FEntwumS.CrossCompiler.Services;

public class ToolchainService : IToolchainService
{
    //Attribute
    public string workingDirectory=String.Empty;
    public string mainSourcesName=String.Empty;
    public ICommandService commandService;
    public ILoggingService loggingService;
    public List<object> toolchains;
    public bool debugCheck;
   

    //Constructor
    public ToolchainService()
    {
        commandService= TransferredContainerProvider.getService<ICommandService>();
        loggingService = TransferredContainerProvider.getService<ILoggingService>();
        toolchains = new List<object>();
        if(!toolchains.Contains("NeoRV32"))toolchains.Add("NeoRV32");
    }

    
    //Helper-Method
    public List<object> returnAvailableToolchains() {
        return toolchains;
    }

    public void setDebugCheck(bool value)
    {
        debugCheck = value;
    }
    public void setWorkingInformation(IProjectFile sourceFile)
    {
        workingDirectory = Path.Combine(sourceFile.Root!.FullPath, "build","compile");
        if (!Directory.Exists(workingDirectory))
        {
            Directory.CreateDirectory(workingDirectory);
        }
        mainSourcesName = sourceFile.FullPath;
    }
    
    //Method for performing the CrossCompiler-Toolchain
    public async Task<bool> performCrossCompilerToolchain(string chosenToolchainId)
    {
        bool check = false;
        
        switch (chosenToolchainId)
        {
            case "NeoRV32":
                check = await doObjectFilesFromStartUpAssembly();
                if (!check) return check;
                check = await doObjectFilesFromMainSources();
                if (!check) return check;
                check = await doObjectFilesFromDriverSources();
                if (!check) return check;
                check = await doELFFileFromObjectFiles();
                if (!check) return check;
                check = await doAssemblyListingFromELFFile();
                if (!check) return check;
                check = await doMainBinaryFromELFFile();
                if (!check) return check;
                check = await generateNeoRVBinaryImage();
                if (!check) return check;
                check = await generateNeoRVVhdImage();
                if (!check) return check;
                check = await generateNeoRVHexImage();
                if (!check) return check;
                cleanup();
                return check;
            default: return check;
        }
    }
    
    

    //Methods for first Step of Toolchain: Generating the Object files /////////////////////////////////////////////////
    private async Task<bool> doObjectFilesFromMainSources()
    {
        //Locals
        string driverLibPath = string.Empty;
        string sourceName = mainSourcesName;
        string outputName = "main.c.o";
        bool success = false;
        
        //Declaration
        driverLibPath = Path.Combine(workingDirectory, "lib", "include");
        
        //Process
        success = await translateSourcesToObjectFiles(driverLibPath, sourceName, outputName);
        
        //Output
        return success;
    }
    
    private async Task<bool> doObjectFilesFromStartUpAssembly()
    {
        //Locals
        string driverLibPath = string.Empty;
        string sourceName = @"C:\Users\morit\OneWareStudio\Projects\testCompile\build\compile\crt0.S";
        string outputName = "startUp.S.o";
        bool success = false;
        
        //Declaration
        driverLibPath = Path.Combine(workingDirectory, "lib", "include");
        
        //Process
        success = await translateSourcesToObjectFiles(driverLibPath, sourceName, outputName);
        
        //Output
        return success;
    }
    
    private async Task<bool> doObjectFilesFromDriverSources()
    {
        //Locals
        string driverLibPath = string.Empty;
        string sourceName = @"C:\Users\morit\OneWareStudio\Projects\testCompile\build\compile\lib\source\iceduino_led.c";
        string outputName = "iceduino_led.c.o";
        bool success = false;
        
        //Declaration
        driverLibPath = Path.Combine(workingDirectory, "lib", "include");
        
        //Process
        success = await translateSourcesToObjectFiles(driverLibPath, sourceName, outputName);
        
        //Output
        return success;
    }

    private async Task<bool> translateSourcesToObjectFiles(string driverLibPath, string sourceName, string outputName)
    {
        //Locals
        string gccToolPath = string.Empty;
        List<string> gccArgs = new List<string>();
        string coreLibPath = string.Empty;
        string outputPath = string.Empty;
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;

        //Path declaration
        coreLibPath = Path.Combine(workingDirectory, "include");
        outputPath = Path.Combine(workingDirectory, outputName);
        gccToolPath = commandService.getToolPath(Constants.gnuCrossCompiler);

        //Argument declaration
        gccArgs.Add(Constants.optCompile);
        gccArgs.Add(Constants.machineArchitecture);
        gccArgs.Add(Constants.applicationBinaryInterface);
        gccArgs.Add(Constants.optOptimize);
        string[] options = Constants.optionsDefault.Split(' ');
        foreach (string option in options) gccArgs.Add(option);
        if(debugCheck)gccArgs.Add(Constants.optDebug);
        gccArgs.Add(Constants.optInclude);
        gccArgs.Add(coreLibPath);
        gccArgs.Add(Constants.optInclude);
        gccArgs.Add(driverLibPath);
        gccArgs.Add(sourceName);
        gccArgs.Add(Constants.optOutput);
        gccArgs.Add(outputPath);
        
        //Process
        (success, stdout, stderr) = await commandService.ExecuteCommandAsync(gccToolPath, gccArgs, workingDirectory);

        //Output
        
        return success;
    }
    
    //Method for second step of Toolchain: Linking the Objectfiles ///////////////////////////////////////////////////
    private async Task<bool> doELFFileFromObjectFiles()
    {
        //Locals
        
        List<string> gccArgs = new List<string>();
        string [] objectFiles= new string[3];
        string gccToolPath = commandService.getToolPath(Constants.gnuCrossCompiler);
        string linkerScript = Path.Combine(workingDirectory, "neorv32.ld");
        string outputPath =  Path.Combine(workingDirectory, "main.elf");
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        objectFiles[0] = Path.Combine(workingDirectory, "startUp.S.o");
        objectFiles[1] = Path.Combine(workingDirectory, "main.c.o");
        objectFiles[2] = Path.Combine(workingDirectory, "iceduino_led.c.o");
        
        gccArgs.Add(Constants.machineArchitecture);
        gccArgs.Add(Constants.applicationBinaryInterface);
        gccArgs.Add(Constants.optOptimize);
        string[] options = Constants.optionsDefault.Split(' ');
        foreach (string option in options) gccArgs.Add(option);
        if(debugCheck)gccArgs.Add(Constants.optDebug);
        gccArgs.Add(Constants.optUseLinkScript);
        gccArgs.Add(linkerScript);
        foreach (var objectPath in objectFiles) gccArgs.Add(objectPath);
        gccArgs.Add(Constants.optOutput);
        gccArgs.Add(outputPath);
        gccArgs.Add(Constants.optNoCLibary);
        
        //Processing 
        (success, stdout, stderr) = await commandService.ExecuteCommandAsync(gccToolPath, gccArgs, workingDirectory);

        //Output
        return success;
    }
    
    //Methods for third step of toolchain: Create the Assembly Listing file ////////////////////////////////////////////
    private async Task<bool> doAssemblyListingFromELFFile()
    {
        //Locals
        List<string> objDumpArgs = new List<string>();
        string objDumpToolPath = commandService.getToolPath(Constants.objectDump);
        string sourcePath =  Path.Combine(workingDirectory, "main.elf");
        string outputPath = Path.Combine(workingDirectory,"main.asm");
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        objDumpArgs.Add(Constants.optDisassembler);
        objDumpArgs.Add(Constants.optWithSourceCode);
        objDumpArgs.Add(Constants.optZeroTerminate);
        objDumpArgs.Add(sourcePath);
        
        //Processing 
        (success, stdout, stderr) = await commandService.ExecuteCommandAsync(objDumpToolPath, objDumpArgs, workingDirectory);
        createFileFromString(stdout, outputPath);
        
        //Output
        return success;
    }

    private void createFileFromString(string inputString, string outputPath)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(inputString);
        File.WriteAllBytes(outputPath, bytes);
    }

    //Methods for fourth step of toolchain: Create main binary file /////////////////////////////////////////////////////
    private async Task<bool> doMainBinaryFromELFFile()
    {
        //Locals
        string textPath = Path.Combine(workingDirectory,"text.bin");
        string rodataPath = Path.Combine(workingDirectory,"rodata.bin");
        string dataPath = Path.Combine(workingDirectory,"data.bin");
        string [] inputFiles = {textPath, rodataPath, dataPath};
        bool check = false;
        

        //Process
        check = await translateELFToBinary(Constants.elfComponents[0], textPath);
        if (!check) return check;
        check = await translateELFToBinary(Constants.elfComponents[1], rodataPath);
        if (!check) return check;
        check = await translateELFToBinary(Constants.elfComponents[2], dataPath);
        if(check) concatToMainBinary(inputFiles);
        return check;
        
    }

    private async Task<bool> translateELFToBinary(string elfComponent, string output)
    {
        //Locals
        List<string> objCopyArgs = new List<string>();
        string objCopyToolPath = commandService.getToolPath(Constants.objectCopy);
        string sourcePath =  Path.Combine(workingDirectory, "main.elf");
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        objCopyArgs.Add(Constants.optInclude);
        objCopyArgs.Add(Constants.elfFormat);
        objCopyArgs.Add(sourcePath);
        objCopyArgs.Add(Constants.optJoin);
        objCopyArgs.Add(elfComponent);
        objCopyArgs.Add(Constants.optOutTarget);
        objCopyArgs.Add(Constants.optFormat);
        objCopyArgs.Add(output);
        
        //Process
        (success, stdout, stderr) = await commandService.ExecuteCommandAsync(objCopyToolPath, objCopyArgs, workingDirectory);

        //Output
        return success;
    }
    
    private void concatToMainBinary(string [] inputFiles)
    {
        //Locals
        FileStream outputStream;
        FileStream inputStream;
        string outputFile = Path.Combine(workingDirectory, "main.bin");

        // Process
        using (outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
        {
            foreach (string inputFile in inputFiles)
            {
                using (inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                {
                    inputStream.CopyTo(outputStream);
                }
            }
        }
    }
    
    //Methods for last Step of Toolchain: Generating Uploadimage for NeoRV32
    public async Task<bool> generateNeoRVBinaryImage()
    {
        //Locals
        List<string> imageGenArgs = new List<string>();
        string imageToolPath = Constants.imageToolPath;
        string sourcePath = Path.Combine(workingDirectory, "main.bin");
        string outputPath = Path.Combine(workingDirectory, "neorv32_exe.bin"); 
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        imageGenArgs.Add(Constants.appBinaryImg);
        imageGenArgs.Add(sourcePath);
        imageGenArgs.Add(outputPath);
        
        //Process
        (success, stdout, stderr) = await commandService.ExecuteCommandAsync(imageToolPath, imageGenArgs, workingDirectory);
        
        //Output
        return success;
    }
    
    public async Task<bool> generateNeoRVVhdImage()
    {
        //Locals
        List<string> imageGenArgs = new List<string>();
        string imageToolPath = Constants.imageToolPath;
        string sourcePath = Path.Combine(workingDirectory, "main.bin");
        string outputPath = Path.Combine(workingDirectory, "neorv32_application_image.vhd"); 
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        imageGenArgs.Add(Constants.appVhdImg);
        imageGenArgs.Add(sourcePath);
        imageGenArgs.Add(outputPath);
        
        //Process
        (success, stdout, stderr) = await commandService.ExecuteCommandAsync(imageToolPath, imageGenArgs, workingDirectory);
        
        //Output
        return success;
    }

    public async Task<bool> generateNeoRVHexImage()
    {
        //Locals
        List<string> imageGenArgs = new List<string>();
        string imageToolPath = Constants.imageToolPath;
        string sourcePath = Path.Combine(workingDirectory, "main.bin");
        string outputPath = Path.Combine(workingDirectory, "neorv32_exe.hex"); 
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        imageGenArgs.Add(Constants.appHexImg);
        imageGenArgs.Add(sourcePath);
        imageGenArgs.Add(outputPath);
        
        //Process
        (success, stdout, stderr) = await commandService.ExecuteCommandAsync(imageToolPath, imageGenArgs, workingDirectory);
        
        //Output
        return success;
    }
    
    //Final cleanup
    public void cleanup()
    {
        //Locals
        List<string> cleanUpArgs = new List<string>();
        string[] tempFiles = { "text.bin", "rodata.bin", "data.bin", "startUp.S.o", "iceduino_led.c.o", "main.c.o" }; 
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;

        //Process
        foreach (string tempFile in tempFiles) File.Delete(Path.Combine(workingDirectory, tempFile));
    }

    
    
}