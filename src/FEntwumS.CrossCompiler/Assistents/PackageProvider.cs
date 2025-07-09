using OneWare.Essentials.PackageManager;
using FEntwumS.CrossCompiler.Assistents;

namespace FEntwumS.CrossCompiler.Services;

public class PackageProvider
{
    /*This Attribut references the connection to the Online-Repository of the required toolchain
 for CrossCompiling on NeoRV32.*/

    public static readonly Package gccToolchain = new()
    {
        Category = "Binaries",
        Type = "NativeTool",
        Name = "GCC-Toolchain for RiscV",
        Id = "crossCompileTools",
        Description = "This CrossCompile-Toolchain is suitable with the NeoRV32(https://github.com/stnolting/neorv32)",
        IconUrl =
            "https://raw.githubusercontent.com/swittlich/OneWare.CC-Toolchain/refs/heads/main/Icon.png",
        License = "MIT License",
        Links =
        [
            new PackageLink
            {
                Name = "GitHub",
                Url = "https://github.com/xpack-dev-tools/riscv-none-elf-gcc-xpack",
            }
        ],
        Tabs =
        [
            new PackageTab
            {
                Title = "License",
                ContentUrl =
                    "https://github.com/xpack-dev-tools/riscv-none-elf-gcc-xpack?tab=MIT-1-ov-file#readme"
            }
        ],
        Versions =
        [
            new PackageVersion
            {
                Version = "14.2.0",
                Targets =
                [
                    new PackageTarget
                    {
                        Target = "win-x64",
                        Url =
                            "https://github.com/xpack-dev-tools/riscv-none-elf-gcc-xpack/releases/download/v14.2.0-3/xpack-riscv-none-elf-gcc-14.2.0-3-win32-x64.zip",
                        AutoSetting =
                        [
                            new PackageAutoSetting
                            {
                                RelativePath = CrossCompileConstants.toolchainRootDirectory,
                                SettingKey = CrossCompileConstants.pathSettingKey,
                            }
                        ]
                       
                    },
                    new PackageTarget
                    {
                        Target = "linux-x64",
                        Url =
                            "https://github.com/xpack-dev-tools/riscv-none-elf-gcc-xpack/releases/download/v14.2.0-3/xpack-riscv-none-elf-gcc-14.2.0-3-linux-x64.tar.gz",
                        AutoSetting =
                        [
                            new PackageAutoSetting
                            {
                                RelativePath = CrossCompileConstants.toolchainRootDirectory,
                                SettingKey = CrossCompileConstants.pathSettingKey,
                            }
                        ]
                    },
                    new PackageTarget
                    {
                        Target = "osx-x64",
                        Url =
                            "https://github.com/xpack-dev-tools/riscv-none-elf-gcc-xpack/releases/download/v14.2.0-3/xpack-riscv-none-elf-gcc-14.2.0-3-darwin-x64.tar.gz",
                        AutoSetting =
                        [
                            new PackageAutoSetting
                            {
                                RelativePath = CrossCompileConstants.toolchainRootDirectory,
                                SettingKey = CrossCompileConstants.pathSettingKey,
                            }
                        ]
                    }
                ]
            }
        ]
    };
}