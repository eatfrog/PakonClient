using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLXLib;
namespace PakonLib.Interfaces
{
    public interface ISavePictures3
    {
        void GetPictureInfo3(int iIndex, 
            out int iRollIndexFromStrip, 
            out int iStripIndexFromStrip, 
            out int iFilmProductFromStrip, 
            out int iFilmSpecifierFromStrip, 
            out string strFrameName, 
            out int iFrameNumber, 
            out int iPrintAspectRatio, 
            out string strFileName, 
            out string strDirectory, 
            out int iRotation, 
            out S_OR_H_000 eiSelectedHidden);
    }

}
