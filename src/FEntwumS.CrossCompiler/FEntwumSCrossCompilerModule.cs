using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using OneWare.Essentials.Enums;
using OneWare.Essentials.Models;
using OneWare.Essentials.Services;
using OneWare.Essentials.ViewModels;
using OneWare.Essentials.PackageManager;
using FEntwumS.CrossCompiler.Assistents;
using FEntwumS.CrossCompiler.Services;
using FEntwumS.CrossCompiler.View;
using FEntwumS.CrossCompiler.ViewModel;
using Prism.Ioc;
using Prism.Modularity;
using CrossCompileViewModel = FEntwumS.CrossCompiler.View.CrossCompileViewModel;

namespace FEntwumS.CrossCompiler;

  



public class FEntwumSCrossCompilerModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<ILoggingService, LoggingService>();
        containerRegistry.RegisterSingleton<ICommandService, CommandService>();
        containerRegistry.RegisterSingleton<IToolchainService, ToolchainService>();
        containerRegistry.Register<ViewModel.CrossCompileViewModel>();
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        addPackageBinaries(containerProvider);
        editGlobalSettings(containerProvider);
        //editProjectSettings(containerProvider);
        registerCrossCompileExtension(containerProvider);
        transferContainerProvider(containerProvider);
        
        //gccFrontendViewModel vm = new gccFrontendViewModel(); 
        //containerProvider.Resolve<IDockService>().Show(vm, DockShowLocation.Document);
        //This example adds a context menu for .vhd files
        containerProvider.Resolve<IProjectExplorerService>().RegisterConstructContextMenu((selected, menuItems) =>
        {
            if (selected is [IProjectRootWithFile root])
            {
                foreach (var item in menuItems)
                {

                }
            }
        });
    }
    
    
    //Add Package Binaries
    public void addPackageBinaries(IContainerProvider containerProvider)
    {
        containerProvider.Resolve<IPackageService>().RegisterPackage(PackageProvider.gccToolchain);
    }
    
    //Edit Global Settings for RISCV-Toolchain
    public void editGlobalSettings(IContainerProvider containerProvider)
    {
        var settingsService = containerProvider.Resolve<ISettingsService>();
        
        settingsService.RegisterSetting("Tools", "GCC-Toolchain for RiscV", CrossCompileConstants.pathSettingKey, 
            new FolderPathSetting("CrossCompile Toolchain Path", CrossCompileConstants.toolchainRootDirectory, "", CrossCompileConstants.pathSettingKey, Path.Exists));
    }

    //Edit ProjectSettings for CrossCompiler
    public void editProjectSettings(IContainerProvider containerProvider)
    {
        var projectSettingsService = containerProvider.Resolve<IProjectSettingsService>();
        
        projectSettingsService.AddProjectSetting("LinkerSkript", new FilePathSetting("Linker skript path for cross compiling: ", "", null, "", Path.Exists,FilePickerFileTypes.All),
            projectRoot =>
            {
                return activationCheckOfCrossCompileSettings(projectRoot);
            });
        projectSettingsService.AddProjectSetting("SourceFiles", new ListBoxSetting("Define sources of c, cpp, s and S files for cross compiling: ",""),
            projectRoot =>
            {
                return activationCheckOfCrossCompileSettings(projectRoot);
            });
        projectSettingsService.AddProjectSetting("IncludePaths", new ListBoxSetting("Define paths of included header files for cross compiling: ",""),
            projectRoot =>
            {
                return activationCheckOfCrossCompileSettings(projectRoot);
            });
        projectSettingsService.AddProjectSetting("CompileFlags", new ListBoxSetting("Additional user flags for cross compiling: ",""),
            projectRoot =>
            {
                return activationCheckOfCrossCompileSettings(projectRoot);
            });
        
    }

    public bool activationCheckOfCrossCompileSettings(IProjectRootWithFile projectRoot)
    {
        foreach (var file in projectRoot.Files)
            if(file.Name.Contains(".cpp") || file.Name.Contains(".c"))return true;
        return false;
    }

    public string[] getProjectCFiles()
    {
        return new string[] {};
    } 

    //Register CrossCompile-Menu
    public void registerCrossCompileExtension(IContainerProvider containerProvider)
    {
        var windowService = containerProvider.Resolve<IWindowService>();
        windowService.RegisterUiExtension("MainWindow_RoundToolBarExtension", new UiExtension(selectedFile =>
        {
            return generateCrossCompileMenu(selectedFile, containerProvider);
        }));
        
        windowService.RegisterUiExtension("EditView_Top", new UiExtension(selectedFile =>
        {
            return generateCrossCompileMenu(selectedFile, containerProvider);
        }));
        
    }

    //Generate View of Cross-Compile-Menu
    public CrossCompileViewModel generateCrossCompileMenu(object file, IContainerProvider containerProvider)
    {
        var toolchainService = containerProvider.Resolve<IToolchainService>();
        if (file is IProjectFile {Extension: ".c"} or IProjectFile {Extension: ".cpp"} )
        {   
            
            toolchainService.setWorkingInformation(file as IProjectFile);
            return new CrossCompileViewModel()
            {
                DataContext =
                    containerProvider.Resolve<ViewModel.CrossCompileViewModel>()
            };
        }
        return null; 
    }

    //Transfer Injector 
    public void transferContainerProvider(IContainerProvider containerProvider)
    {
        TransferredContainerProvider transContainerProvider = new TransferredContainerProvider(containerProvider);
    }
    
     
    
    
}