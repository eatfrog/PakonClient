using System;
using TLXLib;
using PakonLib.Enums;

public class Global
{
    public static int BufferSize(int width, int height, MemoryFileFormat memoryFormat, bool fourChannel)
    {
        switch (memoryFormat.NativeValue)
        {
            case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8:
                if (width > height)
                {
                    return (height + 3) * width * 3 + 62;
                }

                return (width + 3) * height * 3 + 62;
            case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_16:
                if (fourChannel)
                {
                    return 8 * width * height + 16;
                }

                return 6 * width * height + 16;
            case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_8:
                if (fourChannel)
                {
                    return 4 * width * height + 60;
                }

                return 3 * width * height + 60;
            default:
                throw new ArgumentException("File format not supported");
        }
    }

    public static void Convert(int scannerTypeRaw, int scannerVersionRaw, out ScannerType scannerType, out ScannerHW135 hardware135, out ScannerHW235 hardware235, out ScannerHW335 hardware335)
    {
        scannerType = ScannerType.Unknown;
        hardware135 = ScannerHW135.Unknown;
        hardware235 = ScannerHW235.Unknown;
        hardware335 = ScannerHW335.Unknown;

        if (!Enum.IsDefined(typeof(SCANNER_TYPE_000), scannerTypeRaw))
        {
            return;
        }

        SCANNER_TYPE_000 nativeScannerType = (SCANNER_TYPE_000)scannerTypeRaw;
        scannerType = ScannerType.FromNative(nativeScannerType);

        if (!Enum.IsDefined(typeof(SCANNER_VERSION_HW_000), scannerVersionRaw))
        {
            return;
        }

        SCANNER_VERSION_HW_000 nativeVersion = (SCANNER_VERSION_HW_000)scannerVersionRaw;

        switch (nativeScannerType)
        {
            case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
            case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
                switch (nativeVersion)
                {
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_ALPHA_000:
                        hardware135 = ScannerHW135.Alpha;
                        break;
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_CHARLIE_000:
                        hardware135 = ScannerHW135.Charlie;
                        break;
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_PRE_PRODUCTION_000:
                        hardware135 = ScannerHW135.Preproduction;
                        break;
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_BETA_000:
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_PRODUCTION_000:
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_FLATBELT:
                        hardware135 = ScannerHW135.Production;
                        break;
                }

                break;
            case SCANNER_TYPE_000.SCANNER_TYPE_F_235:
            case SCANNER_TYPE_000.SCANNER_TYPE_F_235C:
                switch (nativeVersion)
                {
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_PRODUCTION:
                        hardware235 = ScannerHW235.Rev_A;
                        break;
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_BRIDGE:
                        hardware235 = ScannerHW235.Bridge;
                        break;
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_FLATBELT:
                        hardware235 = ScannerHW235.Rev_D;
                        break;
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_RFT_SPLICE_235:
                        hardware235 = ScannerHW235.RFTSplice;
                        break;
                }

                break;
            case SCANNER_TYPE_000.SCANNER_TYPE_F_335:
            case SCANNER_TYPE_000.SCANNER_TYPE_F_335C:
                switch (nativeVersion)
                {
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_ALPHA_000:
                        hardware335 = ScannerHW335.Alpha;
                        break;
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_CHARLIE_000:
                        hardware335 = ScannerHW335.Charlie;
                        break;
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_PRE_PRODUCTION_000:
                        hardware335 = ScannerHW335.Preproduction;
                        break;
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_BETA_000:
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_PRODUCTION_000:
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_FLATBELT:
                        hardware335 = ScannerHW335.Production;
                        break;
                    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_RFT_SPLICE_335:
                        hardware335 = ScannerHW335.RFTSplice;
                        break;
                }

                break;
        }
    }

    public static int GetResolutionHeight(ScannerType scannerType, Resolution resolution, FilmFormat filmFormat)
    {
        FRAME_SIZES_000 result = FRAME_SIZES_000.FRAME_SIZES_HR_HEIGHT_BASE16_35;
        switch (filmFormat.NativeValue)
        {
            case FILM_FORMAT_000.FILM_FORMAT_24MM:
                switch (resolution.NativeValue)
                {
                    case RESOLUTION_000.RESOLUTION_BASE_4:
                        result = FRAME_SIZES_000.FRAME_SIZES_HR_HEIGHT_BASE4_24;
                        break;
                    case RESOLUTION_000.RESOLUTION_BASE_8:
                        result = FRAME_SIZES_000.FRAME_SIZES_HR_HEIGHT_BASE8_24;
                        break;
                    case RESOLUTION_000.RESOLUTION_BASE_16:
                        result = FRAME_SIZES_000.FRAME_SIZES_HR_HEIGHT_BASE16_24;
                        break;
                }

                break;
            case FILM_FORMAT_000.FILM_FORMAT_35MM:
                switch (resolution.NativeValue)
                {
                    case RESOLUTION_000.RESOLUTION_BASE_4:
                        result = FRAME_SIZES_000.FRAME_SIZES_HR_HEIGHT_BASE4_35;
                        break;
                    case RESOLUTION_000.RESOLUTION_BASE_8:
                        switch (scannerType.NativeValue)
                        {
                            case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
                            case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
                                result = FRAME_SIZES_000.FRAME_SIZES_HR_WIDTH_BASE4_24;
                                break;
                            default:
                                result = FRAME_SIZES_000.FRAME_SIZES_HR_HEIGHT_BASE8_35;
                                break;
                        }

                        break;
                    case RESOLUTION_000.RESOLUTION_BASE_16:
                        result = FRAME_SIZES_000.FRAME_SIZES_HR_HEIGHT_BASE16_35;
                        break;
                }

                break;
            default:
                throw new FormatException("GetResolutionHeight Film Format not supported");
        }

        return (int)result;
    }
}
