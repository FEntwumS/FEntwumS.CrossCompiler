using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OneWare.Essentials.Models;
using OneWare.Essentials.ViewModels;
using FEntwumS.CrossCompiler.Assistents;
using FEntwumS.CrossCompiler.Services;
using FEntwumS.CrossCompiler.View;
using Prism.Ioc;

namespace FEntwumS.CrossCompiler.ViewModel;

public class gccFrontendViewModel : ObservableObject
{
    //Attributes
    public ICommand DoCrossCompiling { get; }
    public IToolchainService toolchainService;
    public ObservableCollection<object> toolchains { get; }
    public string viewTargetSystem;
    public bool doDebug;
    public bool doUpload;

    //Constructor
    public gccFrontendViewModel()
    {
        toolchainService = TransferredContainerProvider.getService<IToolchainService>();
        toolchains = new ObservableCollection<object>(toolchainService.returnAvailableToolchains());
        DoCrossCompiling = new AsyncRelayCommand(() => toolchainService.performCrossCompilerToolchain(viewTargetSystem));
    }

    public void debugSelectRoutine(bool isChecked)
    {
        toolchainService.setDebugCheck(isChecked);
    }
}