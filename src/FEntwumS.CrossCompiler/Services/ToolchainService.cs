using System.Text;
using System.Text.Json.Nodes;
using Avalonia.Xaml.Interactions.Custom;
using DynamicData;
using OneWare.Essentials.Enums;
using OneWare.Essentials.Extensions;
using OneWare.Essentials.Models;
using OneWare.Essentials.Services;
using FEntwumS.CrossCompiler.Assistents;
using FEntwumS.CrossCompiler.ViewModel;
using OneWare.UniversalFpgaProjectSystem.Models;
using Prism.Ioc;

namespace FEntwumS.CrossCompiler.Services;

public class ToolchainService : IToolchainService
{
    //Attribute
    public string workingDirectory=String.Empty;
    public string mainSourcesPath=String.Empty;
    public string mainSourcesName=String.Empty;
    public ICommandService commandService;
    public ILoggingService loggingService;
    public IProjectExplorerService projExplorerService;
    public bool debugCheck;
    public List<string> includePaths;
    public List<string> compileSources;
    public List<string> objectFiles;
    public string toolchain;
    public string linkerSkript;
   

    //Constructor
    public ToolchainService()
    {
        commandService= TransferredContainerProvider.getService<ICommandService>();
        loggingService = TransferredContainerProvider.getService<ILoggingService>();
        projExplorerService = TransferredContainerProvider.getService<IProjectExplorerService>();
        compileSources = new List<string>();
        includePaths = new List<string>();
        objectFiles = new List<string>();
    }

    public void setDebugCheck(bool value)
    {
        debugCheck = value;
    }

    public void setWorkingInformation(IProjectFile sourceFile)
    {
        JsonArray tempJsonArray = (projExplorerService.ActiveProject.Root as UniversalFpgaProjectRoot)
            .Properties["SourceFiles"].AsArray();
        toolchain = (projExplorerService.ActiveProject.Root as UniversalFpgaProjectRoot)
            .Properties["TargetSystem"].ToString();
        linkerSkript = (projExplorerService.ActiveProject.Root as UniversalFpgaProjectRoot)
            .Properties["LinkerSkript"].ToString();
        workingDirectory = Path.Combine(sourceFile.Root!.FullPath, "build", "compile");
        if (!Directory.Exists(workingDirectory))
        {
            Directory.CreateDirectory(workingDirectory);
        }

        mainSourcesPath = (projExplorerService.ActiveProject.Root as UniversalFpgaProjectRoot)
            .Properties["MainFile"].ToString();;
        mainSourcesName = Path.GetFileName(mainSourcesPath);
        mainSourcesName = mainSourcesName.Remove(mainSourcesName.LastIndexOf('.'));
        string checkExtension;
        if (tempJsonArray.Count > 0)
        {
            compileSources.Clear();
            compileSources.Add(mainSourcesPath);
            foreach (var element in tempJsonArray)
            {
                checkExtension = element.ToString().Substring(element.ToString().LastIndexOf('.') + 1);
                switch (checkExtension)
                {
                    case "c":
                        if(!compileSources.Contains(element.ToString()))compileSources.Add(element.ToString());
                        break;
                    case "cpp":
                        if(!compileSources.Contains(element.ToString()))compileSources.Add(element.ToString());
                        break;
                    case "s":
                        if(!compileSources.Contains(element.ToString()))compileSources.Add(element.ToString());
                        break;
                    case "S":
                        if(!compileSources.Contains(element.ToString()))compileSources.Add(element.ToString());
                        break;
                    default:
                        break;
                }
            }
            tempJsonArray = (projExplorerService.ActiveProject.Root as UniversalFpgaProjectRoot)
                .Properties["IncludePaths"].AsArray();
            if (tempJsonArray.Count > 0)
            {
                includePaths.Clear();
                foreach (var path in tempJsonArray)includePaths.Add(path.ToString());
            }
        }
           
        
    }
    
    //Method for performing the CrossCompiler-Toolchain
    public async Task<bool> performCrossCompilerToolchain(string chosenToolchainId)
    {
        bool check = false;
        switch (toolchain)
        {
            case "NeoRV32":
                check = await doObjectFilesFromSources();
                if (!check) return check;
                check = await doELFFileFromObjectFiles();
                if (!check) return check;
                if (!debugCheck)
                {
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
                }
                cleanup();
                return check;
            default: return check;
        }
    }
    
    

    //Methods for first Step of Toolchain: Generating the Object files /////////////////////////////////////////////////

    private async Task<bool> doObjectFilesFromSources()
    {
        if (compileSources.Count.Equals(0))
        {
            loggingService.Error("[CrossCompiler] compile source list is empty.");
            return false;
        }
        string fileName;
        string outputName;
        string[] directoryFiles;
        string wildcardExtension;
        bool success = false;
        objectFiles.Clear();
        foreach (var sourceFile in compileSources)
        {
            if (sourceFile.Contains("*"))
            {
                directoryFiles = Directory.GetFiles(sourceFile.Remove(sourceFile.IndexOf('*') - 1));
                wildcardExtension = Path.GetExtension(sourceFile); 
             foreach (var directoryFile in directoryFiles)
             {
                 if (Path.GetExtension(directoryFile).Equals(wildcardExtension))
                 {
                     fileName = Path.GetFileName(directoryFile);
                     outputName = $"{fileName}.o";
                     outputName = Path.Combine(workingDirectory, outputName);
                     objectFiles.Add(outputName);
                     success = await translateSourceToObjectFile(includePaths.ToArray(), directoryFile, outputName);
                     if (!success)
                     {
                         loggingService.Error("[CrossCompiler] compiling failed. Please try again.");
                         return success;
                     }  
                 }
             }
            }
            else
            {
                fileName = Path.GetFileName(sourceFile);
                outputName = $"{fileName}.o";
                outputName = Path.Combine(workingDirectory, outputName);
                objectFiles.Add(outputName);
                success = await translateSourceToObjectFile(includePaths.ToArray(), sourceFile, outputName);
                if (!success)
                {
                    loggingService.Error("[CrossCompiler] compiling failed. Please try again.");
                    return success;
                }   
            }    
        }
        return success;
    }

    private async Task<bool> translateSourceToObjectFile(string [] includePaths, string sourceFile, string outputFile)
    {
        //Locals
        string gccToolPath = string.Empty;
        List<string> gccArgs = new List<string>();
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;

        //Path declaration
        gccToolPath = commandService.getToolPath(CrossCompileConstants.gnuCrossCompiler);

        //Argument declaration
        gccArgs.Add(CrossCompileConstants.optCompile);
        gccArgs.Add(CrossCompileConstants.machineArchitecture);
        gccArgs.Add(CrossCompileConstants.applicationBinaryInterface);
        gccArgs.Add(CrossCompileConstants.optOptimize);
        string[] options = CrossCompileConstants.optionsDefault.Split(' ');
        foreach (string option in options) gccArgs.Add(option);
        if(debugCheck)gccArgs.Add(CrossCompileConstants.optDebug);
        foreach (string include in includePaths)
        {
            gccArgs.Add(CrossCompileConstants.optInclude);
            gccArgs.Add(include);
        }
        gccArgs.Add(sourceFile);
        gccArgs.Add(CrossCompileConstants.optOutput);
        gccArgs.Add(outputFile);
        
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
        string gccToolPath = commandService.getToolPath(CrossCompileConstants.gnuCrossCompiler);
        string outputPath =  Path.Combine(workingDirectory, $"{mainSourcesName}.elf");
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        gccArgs.Add(CrossCompileConstants.machineArchitecture);
        gccArgs.Add(CrossCompileConstants.applicationBinaryInterface);
        gccArgs.Add(CrossCompileConstants.optOptimize);
        string[] options = CrossCompileConstants.optionsDefault.Split(' ');
        foreach (string option in options) gccArgs.Add(option);
        if(debugCheck)gccArgs.Add(CrossCompileConstants.optDebug);
        gccArgs.Add(CrossCompileConstants.optUseLinkScript);
        gccArgs.Add(linkerSkript);
        foreach (var objectPath in objectFiles) gccArgs.Add(objectPath);
        gccArgs.Add(CrossCompileConstants.optOutput);
        gccArgs.Add(outputPath);
        gccArgs.Add(CrossCompileConstants.optNoCLibary);
        
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
        string objDumpToolPath = commandService.getToolPath(CrossCompileConstants.objectDump);
        string sourcePath =  Path.Combine(workingDirectory, $"{mainSourcesName}.elf");
        string outputPath = Path.Combine(workingDirectory,$"{mainSourcesName}.asm");
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        objDumpArgs.Add(CrossCompileConstants.optDisassembler);
        objDumpArgs.Add(CrossCompileConstants.optWithSourceCode);
        objDumpArgs.Add(CrossCompileConstants.optZeroTerminate);
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
        check = await translateELFToBinary(CrossCompileConstants.elfComponents[0], textPath);
        if (!check) return check;
        check = await translateELFToBinary(CrossCompileConstants.elfComponents[1], rodataPath);
        if (!check) return check;
        check = await translateELFToBinary(CrossCompileConstants.elfComponents[2], dataPath);
        if(check) concatToMainBinary(inputFiles);
        return check;   
    }

    private async Task<bool> translateELFToBinary(string elfComponent, string output)
    {
        //Locals
        List<string> objCopyArgs = new List<string>();
        string objCopyToolPath = commandService.getToolPath(CrossCompileConstants.objectCopy);
        string sourcePath =  Path.Combine(workingDirectory, $"{mainSourcesName}.elf");
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        objCopyArgs.Add(CrossCompileConstants.optInclude);
        objCopyArgs.Add(CrossCompileConstants.elfFormat);
        objCopyArgs.Add(sourcePath);
        objCopyArgs.Add(CrossCompileConstants.optJoin);
        objCopyArgs.Add(elfComponent);
        objCopyArgs.Add(CrossCompileConstants.optOutTarget);
        objCopyArgs.Add(CrossCompileConstants.optFormat);
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
        string outputFile = Path.Combine(workingDirectory, $"{mainSourcesName}.bin");

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
        string imageToolPath = CrossCompileConstants.imageToolPath;
        string sourcePath = Path.Combine(workingDirectory, $"{mainSourcesName}.bin");
        string outputPath = Path.Combine(workingDirectory, "neorv32_exe.bin"); 
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        imageGenArgs.Add(CrossCompileConstants.appBinaryImg);
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
        string imageToolPath = CrossCompileConstants.imageToolPath;
        string sourcePath = Path.Combine(workingDirectory, $"{mainSourcesName}.bin");
        string outputPath = Path.Combine(workingDirectory, "neorv32_application_image.vhd"); 
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        imageGenArgs.Add(CrossCompileConstants.appVhdImg);
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
        string imageToolPath = CrossCompileConstants.imageToolPath;
        string sourcePath = Path.Combine(workingDirectory, $"{mainSourcesName}.bin");
        string outputPath = Path.Combine(workingDirectory, "neorv32_exe.hex"); 
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;
        
        //Declaration
        imageGenArgs.Add(CrossCompileConstants.appHexImg);
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
        List<string> tempFiles = new List<string>();
        tempFiles.Add("text.bin");
        tempFiles.Add("rodata.bin");
        tempFiles.Add("data.bin");
         
        bool success = false;
        string stdout = string.Empty;
        string stderr = string.Empty;

        //Process
        foreach (string tempFile in tempFiles) File.Delete(Path.Combine(workingDirectory, tempFile));
        foreach (string objectFile in objectFiles)File.Delete(Path.Combine(workingDirectory, objectFile));
    }   
}