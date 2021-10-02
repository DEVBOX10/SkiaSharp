DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/watchos"));

#load "../../cake/native-shared.cake"
#load "../../cake/xcode.cake"

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    Build("watchsimulator", "i386", "x86");
    Build("watchos", "armv7k", "arm");
    Build("watchos", "arm64_32", "arm64");

    CreateFatFramework(OUTPUT_PATH.Combine("libSkiaSharp"));

    void Build(string sdk, string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"watchos/{arch}", "skia",
            $"target_os='watchos' " +
            $"target_cpu='{skiaArch}' " +
            $"skia_enable_gpu=false " +
            $"skia_use_icu=false " +
            $"skia_use_metal=false " +
            $"skia_use_piex=true " +
            $"skia_use_sfntly=false " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF' ] ");

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, arch);

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"libSkiaSharp/{arch}.xcarchive"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    Build("watchsimulator", "i386");
    Build("watchos", "armv7k");
    Build("watchos", "arm64_32");

    CreateFatFramework(OUTPUT_PATH.Combine("libHarfBuzzSharp"));

    void Build(string sdk, string arch)
    {
        if (Skip(arch)) return;

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, arch);

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"libHarfBuzzSharp/{arch}.xcarchive"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
