using OneWare.Essentials.Converters;

namespace FEntwumS.CrossCompiler.Assistents;

public class CrossCompileConstants
{
    public static string pathSettingKey = "CrossCompilePathSetting";
    public static string toolchainRootDirectory = "xpack-riscv-none-elf-gcc-14.2.0-3";
           
    public static string gnuCrossCompiler = "riscv-none-elf-gcc";
    public static string objectDump = "riscv-none-elf-objdump";
    public static string objectCopy = "riscv-none-elf-objcopy";
            
    public static string architectureSettingKey = "ArchitectureSetting";
    public static string machineArchitecture = "-march=rv32i_zicsr";
            
    public static string abiSettingKey = "abi-setting";
    public static string applicationBinaryInterface = "-mabi=ilp32";
    
    public static string optOptimize = "-Os";
            
    public static string optionSettingKey = "OptionSetting";
    public static string optionsDefault = "-Wall -ffunction-sections -fdata-sections -nostartfiles -mno-fdiv -Wl,--gc-sections -lm -lc -lgcc -lc -falign-functions=4 -falign-labels=4 -falign-loops=4 -falign-jumps=4";

    
    public static string optCompile = "-c";
    public static string optDebug = "-g";
    public static string optInclude = "-I";
    public static string optOutput = "-o";
    public static string optUseLinkScript = "-T";
    public static string optNoCLibary = "-lm";
    public static string optDisassembler = "-d";
    public static string optWithSourceCode = "-S";
    public static string optZeroTerminate = "-z";
    public static string optJoin = "-j";
    public static string optOutTarget = "-O";
    public static string optFormat = "binary";
    public static string elfFormat = "elf32-little";
    public static string[] elfComponents = { ".text", ".rodata", ".data" };

    public static string imageToolPath = @"C:\Users\morit\OneWareStudio\Projects\testCompile\build\compile\image_gen.exe";
    public static string appBinaryImg = "-app_bin";
    public static string appVhdImg = "-app_img";
    public static string appHexImg = "-app_hex";



}