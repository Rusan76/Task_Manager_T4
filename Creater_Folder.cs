using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using System.Text;
using ProjectT4;

class CreaterFolder
{
    static string GetDesktopPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    
    static string GetDocumentsPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    
    static string GetAppDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }

    
    static string GetLocalAppDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }

    public static string CreateReportFolder(string folderName = "test001111111")
    {
        
        string basePath = GetDesktopPath(); 
        string folderPath = Path.Combine(basePath, folderName);
        
        try
        {
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Console.WriteLine($"[+] Папка создана: {folderPath}");
            }
            else
            {
                Console.WriteLine($"[+] Папка уже существует: {folderPath}");
            }
            
            return folderPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Не удалось создать папку: {ex.Message}");
            
            
            string fallbackPath = Path.Combine(GetLocalAppDataPath(), folderName);
            Directory.CreateDirectory(fallbackPath);
            Console.WriteLine($"[+] Папка создана в альтернативном месте: {fallbackPath}");
            
            return fallbackPath;
        }
    }

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}