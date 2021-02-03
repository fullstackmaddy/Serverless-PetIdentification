using System;
using System.Collections.Generic;
using System.Text;

namespace PetIdentificationUI.UITests.Helpers
{
    public static class InstanceFactory
    {
        public static string DisclaimerExpectedHtml = @"<div class:ignore=""disclaimer"">
            <p>© Copyright 2021-2022 Pet Identification Project</p>
            <p>This website is ficticious. This is designed as part of the demo done for azure dev stories 2020</p>
            <p>Go to <a href=""https://devstories.konfhub.com/"" target=""_blank"">Azure Developer Stories</a></p>
            </div>";

        public static string NavMenuExpectedHtml = @"<div class:ignore=""navbar"">
            <a href="""" class:ignore>
                <img src=""images/logo.png"" class=""logo"">
            </a>
             <nav>
            <ul id=""menuList"">
                <li>
                    <a href = ""blobstorageuploadhttp"" class:ignore>Blobstorage Upload Http</a>
                </li>
                <li>
                    <a href = ""blobstorageuploadeventgrid"" class:ignore>Blobstorage Upload EventGrid</a>
                </li>
                <li>
                    <a href = ""formupload"" class:ignore>Form Upload</a>
                </li>
            </ul>
                </nav>
            </div>";

    }
}
