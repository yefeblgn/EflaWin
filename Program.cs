using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Efla;

public sealed partial class App : Application
{
    private MainWindow window;

    public App() => InitializeComponent();

    [STAThread]
    private static void Main()
    {
        try
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            Application.Start(args =>
            {
                try
                {
                    SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
                    _ = new App();
                }
                catch (Exception hata) { BaslatmaHatasi(hata); Current?.Exit(); }
            });
        }
        catch (Exception hata) { BaslatmaHatasi(hata); }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            window = new MainWindow();
            window.Activate();
            window.TepsiyiBagla();
        }
        catch (Exception hata) { BaslatmaHatasi(hata); Exit(); }
    }

    private static void BaslatmaHatasi(Exception hata)
    {
        MessageBox(0, Localization.Cevir("Efla Win başlatılamadı.") + "\n\n" + hata.GetBaseException().Message, "Efla Win", 0x10);
        Environment.ExitCode = 1;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(nint hwnd, string text, string caption, uint flags);
}
