﻿using System;
using System.Linq;

namespace Haley.Enums
{
    public enum ResizeAffectMode
    {
        pixelSize,
        pixelSize_dpi,
        pixelSize_imageSize,
    }

    public enum DialogMode
    {
        Notification,
        Confirmation,
        GetInput
    }
}
