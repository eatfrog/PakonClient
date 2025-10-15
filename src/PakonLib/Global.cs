using System;
using TLXLib;
using PakonLib.Enums;
public class Global
{
    public static int BufferSize(int nWidth, int nHeight, FILE_FORMAT_SAVE_TO_MEMORY_000 eMemoryFormat, bool bFourChannel)
    {
        switch (eMemoryFormat)
        {
            case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8:
                if (nWidth > nHeight)
                {
                    return (nHeight + 3) * nWidth * 3 + 62;
                }
                return (nWidth + 3) * nHeight * 3 + 62;
            case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_16:
                if (bFourChannel)
                {
                    return 8 * nWidth * nHeight + 16;
                }
                return 6 * nWidth * nHeight + 16;
            case FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_8:
                if (bFourChannel)
                {
                    return 4 * nWidth * nHeight + 60;
                }
                return 3 * nWidth * nHeight + 60;
            default:
                throw new ArgumentException("File format not supported");
        }
    }

    public static void Convert(int iScannerType, int iScannerVersionHw, out SCANNER_TYPE_000 nScannerType, out ScannerHW135 sh135, out ScannerHW235 sh235, out ScannerHW335 sh335)
    {
	    nScannerType = SCANNER_TYPE_000.SCANNER_TYPE_UNKNOWN;
	    sh135 = ScannerHW135.Unknown;
	    sh235 = ScannerHW235.Unknown;
	    sh335 = ScannerHW335.Unknown;
	    if (!Enum.IsDefined(nScannerType.GetType(), iScannerType))
	    {
		    return;
	    }
	    nScannerType = (SCANNER_TYPE_000)iScannerType;
	    SCANNER_VERSION_HW_000 sCANNER_VERSION_HW_ = SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_UNKNOWN;
	    if (!Enum.IsDefined(sCANNER_VERSION_HW_.GetType(), iScannerVersionHw))
	    {
		    return;
	    }
	    sCANNER_VERSION_HW_ = (SCANNER_VERSION_HW_000)iScannerVersionHw;
	    switch (nScannerType)
	    {
	    case SCANNER_TYPE_000.SCANNER_TYPE_F_135:
	    case SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS:
		    switch (sCANNER_VERSION_HW_)
		    {
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_ALPHA_000:
			    sh135 = ScannerHW135.Alpha;
			    break;
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_CHARLIE_000:
			    sh135 = ScannerHW135.Charlie;
			    break;
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_PRE_PRODUCTION_000:
			    sh135 = ScannerHW135.Preproduction;
			    break;
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_BETA_000:
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_PRODUCTION_000:
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_FLATBELT:
			    sh135 = ScannerHW135.Production;
			    break;
		    }
		    break;
	    case SCANNER_TYPE_000.SCANNER_TYPE_F_235:
	    case SCANNER_TYPE_000.SCANNER_TYPE_F_235C:
		    switch (sCANNER_VERSION_HW_)
		    {
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_PRODUCTION:
			    sh235 = ScannerHW235.Rev_A;
			    break;
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_BRIDGE:
			    sh235 = ScannerHW235.Bridge;
			    break;
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_FLATBELT:
			    sh235 = ScannerHW235.Rev_D;
			    break;
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_RFT_SPLICE_235:
			    sh235 = ScannerHW235.RFTSplice;
			    break;
		    }
		    break;
	    case SCANNER_TYPE_000.SCANNER_TYPE_F_335:
	    case SCANNER_TYPE_000.SCANNER_TYPE_F_335C:
		    switch (sCANNER_VERSION_HW_)
		    {
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_ALPHA_000:
			    sh335 = ScannerHW335.Alpha;
			    break;
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_CHARLIE_000:
			    sh335 = ScannerHW335.Charlie;
			    break;
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_PRE_PRODUCTION_000:
			    sh335 = ScannerHW335.Preproduction;
			    break;
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_BETA_000:
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_PRODUCTION_000:
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_FLATBELT:
			    sh335 = ScannerHW335.Production;
			    break;
		    case SCANNER_VERSION_HW_000.SCANNER_VERSION_HW_RFT_SPLICE_335:
			    sh335 = ScannerHW335.RFTSplice;
			    break;
		    }
		    break;
	    }
    }

    public static int GetResolutionHeight(SCANNER_TYPE_000 stType, RESOLUTION_000 srResolution, FILM_FORMAT_000 ffFormat)
    {
	    FRAME_SIZES_000 result = FRAME_SIZES_000.FRAME_SIZES_HR_HEIGHT_BASE16_35;
	    switch (ffFormat)
	    {
	    case FILM_FORMAT_000.FILM_FORMAT_24MM:
		    switch (srResolution)
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
		    switch (srResolution)
		    {
		    case RESOLUTION_000.RESOLUTION_BASE_4:
			    result = FRAME_SIZES_000.FRAME_SIZES_HR_HEIGHT_BASE4_35;
			    break;
		    case RESOLUTION_000.RESOLUTION_BASE_8:
			    switch (stType)
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
