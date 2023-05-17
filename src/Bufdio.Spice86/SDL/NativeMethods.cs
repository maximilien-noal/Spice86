namespace Bufdio.Spice86.SDL;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// Native methods for the SDL2 library.
/// </summary>
public static class NativeMethods {
    private const string Sdl2Library = "sdl2";

    [DllImport(Sdl2Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_InitSubSystem(SDLInitFlags flags);

    [DllImport(Sdl2Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_NumJoysticks();

    [DllImport(Sdl2Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_GameControllerOpen(int deviceIndex);

    [DllImport(Sdl2Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_GameControllerClose(int gamecontroller);

    [DllImport(Sdl2Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDLControllerAxisState SDL_GameControllerGetAxis(int gamecontroller, SDLControllerAxis axis);

    [DllImport(Sdl2Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte SDL_GameControllerGetButton(int gamecontroller, SDLControllerButton button);

    [DllImport(Sdl2Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_PumpEvents();

    [DllImport(Sdl2Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDL_PollEvent(out SDLEvent e);

    [StructLayout(LayoutKind.Sequential)]
    public struct SDLEvent
    {
        public uint Type;
        public SDLControllerAxisEvent ControllerAxisEvent;
        public SDLControllerButtonEvent ControllerButtonEvent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDLControllerAxisEvent
    {
        public uint Type;
        public uint Timestamp;
        public int Which;
        public byte Axis;
        [MarshalAs(UnmanagedType.I2)]
        public short Value;
        public byte Padding1;
        public byte Padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDLControllerButtonEvent
    {
        public uint Type;
        public uint Timestamp;
        public int Which;
        public byte Button;
        public byte State;
        public byte Padding1;
        public byte Padding2;
    }

    [Flags]
    public enum SDLInitFlags : uint
    {
        GameController = 0x00002000
    }

    public enum SDLControllerAxis
    {
        LeftX,
        LeftY,
        RightX,
        RightY,
        TriggerLeft,
        TriggerRight
    }

    public enum SDLControllerButton
    {
        A,
        B,
        X,
        Y,
        Back,
        Guide,
        Start,
        LeftStick,
        RightStick,
        LeftShoulder,
        RightShoulder,
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDLControllerAxisState
    {
        [MarshalAs(UnmanagedType.I2)]
        public short Value;
        public byte Padding1;
        public byte Padding2;
    }
}

