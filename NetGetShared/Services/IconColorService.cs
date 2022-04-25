using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace NetGetShared
{
    public class IconColorService
    {
        public string GetFolderIcon(bool expanded)
        {
            if (expanded)
            {
                return Icons.Custom.Uncategorized.FolderOpen;
            }
            else
            {
                return Icons.Custom.Uncategorized.Folder;
            }
        }
        public Color GetTextColor(bool switchValue)
        {
            if (switchValue)
            {
                return Color.Primary;
            }
            else
            {
                return Color.Default;
            }
        }
        public Color GetIconColor(string href, string navUri, bool exactMatch)
        {
            if (exactMatch)
            {
                return GetIconColor(href, navUri);
            }
            else
            {
                if (navUri.Contains("https://0.0.0.0" + href))
                {
                    return MudBlazor.Color.Primary;
                }
                else
                {
                    return MudBlazor.Color.Default;
                }
            }
        }
        public Color GetIconColor(string href, string navUri)
        {
            if (navUri == "https://0.0.0.0" + href)
            {
                return MudBlazor.Color.Primary;
            }
            else
            {
                return MudBlazor.Color.Default;
            }
        }
    }
}
