using Avalonia.Data;
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

namespace FEntwumS.CrossCompiler;

  



public class OneWaregnuCompilerModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<ILoggingService, LoggingService>();
        containerRegistry.RegisterSingleton<ICommandService, CommandService>();
        containerRegistry.RegisterSingleton<IToolchainService, ToolchainService>();
        containerRegistry.Register<gccFrontendViewModel>();
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        addPackageBinaries(containerProvider);
        editUISettings(containerProvider);
        transferContainerProvider(containerProvider);
        
        

        var toolchainService = containerProvider.Resolve<IToolchainService>();
        var windowService = containerProvider.Resolve<IWindowService>();
       
        //gccFrontendViewModel vm = new gccFrontendViewModel(); 
        //containerProvider.Resolve<IDockService>().Show(vm, DockShowLocation.Document);
        
        //This example adds a context menu for .vhd files
        containerProvider.Resolve<IProjectExplorerService>().RegisterConstructContextMenu((selected, menuItems) =>
        {
            if (selected is [IProjectFile {Extension: ".c"} cFile])
            {
                
                menuItems.Add(new MenuItemViewModel("")
                {

                });
            }
        });
        
        windowService.RegisterUiExtension("MainWindow_RoundToolBarExtension", new UiExtension(x =>
        {
            if (x is IProjectFile {Extension: ".c"} or IProjectFile {Extension: ".cpp"})
                return new gccFrontendView()
                {
                    DataContext =
                        containerProvider.Resolve<gccFrontendViewModel>()
                };
            return null;
        }));
        windowService.RegisterUiExtension("EditView_Top", new UiExtension(selectedFile =>
        {
            if (selectedFile is IProjectFile {Extension: ".c"} or IProjectFile {Extension: ".cpp"} )
            {   
                toolchainService.setWorkingInformation(selectedFile as IProjectFile);
                return new gccFrontendView()
                {
                    DataContext =
                        containerProvider.Resolve<gccFrontendViewModel>()
                };
            }
            return null;
        }));
    }

    public void editUISettings(IContainerProvider containerProvider)
    {
        containerProvider.Resolve<ISettingsService>().RegisterSetting("Tools", "GCC-Toolchain for RiscV", Constants.pathSettingKey, 
            new FolderPathSetting("CrossCompile Toolchain Path", Constants.toolchainRootDirectory, "", Constants.pathSettingKey, Path.Exists));
        
        containerProvider.Resolve<ISettingsService>().RegisterSetting("Tools", "GCC-Toolchain for RiscV", Constants.architectureSettingKey, 
            new TextBoxSetting("Option for Machine Architecture", "-march=rv32i_zicsr_zifencei", ""));
        containerProvider.Resolve<ISettingsService>().RegisterSetting("Tools", "GCC-Toolchain for RiscV", Constants.abiSettingKey, 
            new TextBoxSetting("Option for Application Binary Interface", Constants.applicationBinaryInterface,""));
        containerProvider.Resolve<ISettingsService>().RegisterSetting("Tools", "GCC-Toolchain for RiscV", Constants.optionSettingKey, 
            new TextBoxSetting("Options for compiling and linking", Constants.optionsDefault,""));
    }

    public void addPackageBinaries(IContainerProvider containerProvider)
    {
        containerProvider.Resolve<IPackageService>().RegisterPackage(PackageProvider.gccToolchain);
    }

    public void transferContainerProvider(IContainerProvider containerProvider)
    {
        TransferredContainerProvider transContainerProvider = new TransferredContainerProvider(containerProvider);
    }
}