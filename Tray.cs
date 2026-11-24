using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

namespace Efla;

internal sealed class Tray : IDisposable
{
    private const uint Callback = 0x8041;
    private const int ExtendedStyle = -20, LayeredWindow = 0x00080000;
    private const uint LayeredAlpha = 0x00000002;
    private readonly MainWindow owner;
    private readonly nint ownerHandle;
    private readonly SubclassProc subclass;
    private readonly uint taskbarCreated = RegisterWindowMessage("TaskbarCreated");
    private NotifyIconData icon;
    private Window popup;
    private Grid menuAnchor;
    private MenuFlyout contextMenu;
    private bool active, disposed;

    internal Tray(MainWindow owner, string iconPath)
    {
        this.owner = owner;
        ownerHandle = WinRT.Interop.WindowNative.GetWindowHandle(owner);
        subclass = WindowMessage;
        icon = new NotifyIconData
        {
            Size = (uint)Marshal.SizeOf<NotifyIconData>(), Window = ownerHandle, Id = 1,
            Flags = 0x87, CallbackMessage = Callback, Icon = LoadImage(0, iconPath, 1, 32, 32, 0x10),
            Tip = "Efla Win", Info = "", InfoTitle = "", Version = 4
        };
        if (icon.Icon != 0 && SetWindowSubclass(ownerHandle, subclass, 1, 0)) Kaydet();
    }

    private void Kaydet()
    {
        active = ShellNotifyIcon(0, ref icon);
        if (active) ShellNotifyIcon(4, ref icon);
    }

    private nint WindowMessage(nint hwnd, uint msg, nuint wparam, nint lparam, nuint id, nuint reference)
    {
        if (disposed) return DefSubclassProc(hwnd, msg, wparam, lparam);
        if (msg == taskbarCreated) Kaydet();
        if (msg == 0x0005 && wparam == 1 && active) owner.DispatcherQueue.TryEnqueue(() =>
        {
            if (!disposed && IsIconic(ownerHandle)) ShowWindow(ownerHandle, 0);
        });
        if (msg == Callback)
        {
            var notification = (ushort)(long)lparam;
            if (notification is 0x007B or 0x0205) owner.DispatcherQueue.TryEnqueue(() => MenuAc());
            else if (notification is 0x0400 or 0x0401 or 0x0203) owner.DispatcherQueue.TryEnqueue(() => SahibiAc(owner.TepsidenAc));
            return 0;
        }
        return DefSubclassProc(hwnd, msg, wparam, lparam);
    }

    private void SahibiAc(Action ac)
    {
        if (disposed) return;
        if (IsIconic(ownerHandle)) ShowWindow(ownerHandle, 9);
        ac();
    }

    private void MenuAc()
    {
        if (disposed) return;
        GetCursorPos(out var nokta);
        if (popup == null)
        {
            popup = new Window { Title = "Efla Win" };
            menuAnchor = new Grid { Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent) };
            popup.Content = menuAnchor;
            popup.AppWindow.IsShownInSwitchers = false;
            if (popup.AppWindow.Presenter is OverlappedPresenter sunum)
            {
                sunum.IsResizable = false;
                sunum.IsMinimizable = false;
                sunum.IsMaximizable = false;
                sunum.IsAlwaysOnTop = true;
                sunum.SetBorderAndTitleBar(false, false);
            }
            var pencere = WinRT.Interop.WindowNative.GetWindowHandle(popup);
            SetWindowLong(pencere, ExtendedStyle, GetWindowLong(pencere, ExtendedStyle) | LayeredWindow);
            SetLayeredWindowAttributes(pencere, 0, 0, LayeredAlpha);
            var bolge = CreateRectRgn(0, 0, 0, 0);
            if (bolge != 0 && SetWindowRgn(pencere, bolge, false) == 0) DeleteObject(bolge);
            contextMenu = MenuGovdesi();
            contextMenu.Closed += (_, _) => { if (!disposed) popup.AppWindow.Hide(); };
        }
        menuAnchor.RequestedTheme = ((FrameworkElement)owner.Content).RequestedTheme;
        var alan = DisplayArea.GetFromPoint(new PointInt32(nokta.X, nokta.Y), DisplayAreaFallback.Nearest).WorkArea;
        int x = Math.Clamp(nokta.X, alan.X, Math.Max(alan.X, alan.X + alan.Width - 2));
        int y = Math.Clamp(nokta.Y, alan.Y, Math.Max(alan.Y, alan.Y + alan.Height - 2));
        popup.AppWindow.MoveAndResize(new RectInt32(x, y, 2, 2));
        popup.Activate();
        if (menuAnchor.IsLoaded) Goster();
        else menuAnchor.Loaded += Yuklendi;

        void Yuklendi(object sender, RoutedEventArgs e) { menuAnchor.Loaded -= Yuklendi; Goster(); }
        void Goster() { if (!disposed) contextMenu.ShowAt(menuAnchor); }
    }

    private MenuFlyout MenuGovdesi()
    {
        var flyout = new MenuFlyout { ShouldConstrainToRootBounds = false, Placement = FlyoutPlacementMode.TopEdgeAlignedRight };
        flyout.Items.Add(Madde(Localization.Cevir("Efla Win’i aç"), "\uE8A7", () => SahibiAc(owner.TepsidenAc)));
        flyout.Items.Add(Madde(Localization.Cevir("İndirilenler klasörü"), "\uE838", () => SahibiAc(owner.KlasoruAc)));
        flyout.Items.Add(Madde(Localization.Cevir("Hafıza"), "\uE964", () => SahibiAc(owner.TepsidenHafiza)));
        flyout.Items.Add(new MenuFlyoutSeparator());
        flyout.Items.Add(Madde(Localization.Cevir("Çıkış"), "\uE8BB", owner.Close));
        return flyout;
    }

    private MenuFlyoutItem Madde(string ad, string glyph, Action action)
    {
        var madde = new MenuFlyoutItem { Text = ad, Icon = new FontIcon { Glyph = glyph, FontSize = 16 } };
        madde.Click += (_, _) => owner.DispatcherQueue.TryEnqueue(() => { if (!disposed) action(); });
        return madde;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        popup?.Close();
        if (active) ShellNotifyIcon(2, ref icon);
        RemoveWindowSubclass(ownerHandle, subclass, 1);
        if (icon.Icon != 0) DestroyIcon(icon.Icon);
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint SubclassProc(nint hwnd, uint message, nuint wparam, nint lparam, nuint id, nuint reference);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NotifyIconData
    {
        internal uint Size;
        internal nint Window;
        internal uint Id, Flags, CallbackMessage;
        internal nint Icon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] internal string Tip;
        internal uint State, StateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] internal string Info;
        internal uint Version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] internal string InfoTitle;
        internal uint InfoFlags;
        internal Guid Guid;
        internal nint BalloonIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Point { internal int X, Y; }

    [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIconW")] private static extern bool ShellNotifyIcon(uint message, ref NotifyIconData data);
    [DllImport("comctl32.dll")] private static extern bool SetWindowSubclass(nint handle, SubclassProc callback, nuint id, nuint reference);
    [DllImport("comctl32.dll")] private static extern bool RemoveWindowSubclass(nint handle, SubclassProc callback, nuint id);
    [DllImport("comctl32.dll")] private static extern nint DefSubclassProc(nint handle, uint msg, nuint wparam, nint lparam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern nint LoadImage(nint instance, string name, uint type, int width, int height, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern uint RegisterWindowMessage(string name);
    [DllImport("user32.dll")] private static extern bool DestroyIcon(nint icon);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern bool IsIconic(nint hwnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(nint hwnd, int command);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")] private static extern int GetWindowLong(nint hwnd, int index);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")] private static extern int SetWindowLong(nint hwnd, int index, int value);
    [DllImport("user32.dll")] private static extern bool SetLayeredWindowAttributes(nint hwnd, uint colorKey, byte alpha, uint flags);
    [DllImport("user32.dll")] private static extern int SetWindowRgn(nint hwnd, nint region, [MarshalAs(UnmanagedType.Bool)] bool redraw);
    [DllImport("gdi32.dll")] private static extern nint CreateRectRgn(int left, int top, int right, int bottom);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(nint handle);
}
