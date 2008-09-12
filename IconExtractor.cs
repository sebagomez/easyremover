using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections;

namespace Program_Finder
{
    class IconHelper
    {
        [DllImport("Shell32.dll",EntryPoint="ExtractIconExW",CharSet=CharSet.Unicode,ExactSpelling=true,CallingConvention=CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile,int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        //static IntPtr large, small;
        //static Icon extractedIcon;
        //ArrayList items;

        public IconHelper()
        {
            //items = new ArrayList();

            //// Extract Normal folder
            //ExtractIconEx("Shell32.dll", 3, out large,out small, 1);
            //extractedIcon= Icon.FromHandle(small);
            //items.Add(extractedIcon);

            //// Extract Open folder
            //ExtractIconEx("Shell32.dll", 4, out large,out small, 1);
            //extractedIcon= Icon.FromHandle(small);
            //items.Add(extractedIcon);

            //// Extract A drive
            //ExtractIconEx("Shell32.dll", 6, out large,out small, 1);
            //extractedIcon= Icon.FromHandle(small);
            //items.Add(extractedIcon);

            //// Extract Harddisk
            //ExtractIconEx("Shell32.dll", 8, out large,out small, 1);
            //extractedIcon= Icon.FromHandle(small);
            //items.Add(extractedIcon);

            //// Extract Network drive
            //ExtractIconEx("Shell32.dll", 9, out large,out small, 1);
            //extractedIcon= Icon.FromHandle(small);
            //items.Add(extractedIcon);

            //// Extract CDROM drive
            //ExtractIconEx("Shell32.dll", 11, out large,out small, 1);
            //extractedIcon= Icon.FromHandle(small);
            //items.Add(extractedIcon);
        }
        
        public static Icon[] GetIcons(string filePath)
        {
            IntPtr large, small;

            filePath = filePath.Replace("\"", "");

            //if (filePath.EndsWith(".ico"))
            //    return new Icon(filePath);

            string[] location = filePath.Split(',');
            string file = location[0];
            
            int index = 0;
            if (location.Length > 1)
                index = int.Parse(location[1]);

            ExtractIconEx(file, index, out large, out small, 1);

            Icon[] icons = new Icon[2];
            
            if (large.ToInt32() > 0)
                icons[0] = Icon.FromHandle(large);

            if (small.ToInt32() > 0)
                icons[1] = Icon.FromHandle(small);

            return icons;
        }
    }
}
