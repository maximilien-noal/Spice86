﻿namespace Spice86.Core.Emulator.VM;

using System.Diagnostics;
using System.Text;

using Spice86.Core.CLI;
using Spice86.Core.Emulator;
using Spice86.Core.Emulator.Callback;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Devices;
using Spice86.Core.Emulator.Devices.DirectMemoryAccess;
using Spice86.Core.Emulator.Devices.ExternalInput;
using Spice86.Core.Emulator.Devices.Input.Joystick;
using Spice86.Core.Emulator.Devices.Input.Keyboard;
using Spice86.Core.Emulator.Devices.Sound;
using Spice86.Core.Emulator.Devices.Timer;
using Spice86.Core.Emulator.Devices.Video;
using Spice86.Core.Emulator.Errors;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.InterruptHandlers.Bios;
using Spice86.Core.Emulator.InterruptHandlers.Dos.Ems;
using Spice86.Core.Emulator.InterruptHandlers.Dos;
using Spice86.Core.Emulator.InterruptHandlers.Dos.Ems;
using Spice86.Core.Emulator.InterruptHandlers.Dos.Xms;
using Spice86.Core.Emulator.InterruptHandlers.Input.Keyboard;
using Spice86.Core.Emulator.InterruptHandlers.Input.Mouse;
using Spice86.Core.Emulator.InterruptHandlers.SystemClock;
using Spice86.Core.Emulator.InterruptHandlers.Timer;
using Spice86.Core.Emulator.InterruptHandlers.VGA;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.OperatingSystem;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Emulator.Errors;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// Emulates an IBM PC
/// </summary>
public class Machine : IDisposable {
    private readonly ProgramExecutor _programExecutor;
    private readonly List<DmaChannel> _dmaDeviceChannels = new();
    private readonly Thread _dmaThread;
    private bool _exitDmaLoop;
    private bool _dmaThreadStarted;
    private readonly ManualResetEvent _dmaResetEvent = new(true);

    private bool _disposed;

    /// <summary>
    /// Gets or set if we record execution data, for reverse engineering purposes.
    /// </summary>
    public bool RecordData { get; set; }
    
    /// <summary>
    /// Memory mapped BIOS values.
    /// </summary>
    public BiosDataArea BiosDataArea { get; set; }

    /// <summary>
    /// INT11H handler.
    /// </summary>
    public BiosEquipmentDeterminationInt11Handler BiosEquipmentDeterminationInt11Handler { get; }

    /// <summary>
    /// INT9H handler.
    /// </summary>
    public BiosKeyboardInt9Handler BiosKeyboardInt9Handler { get; }

    /// <summary>
    /// Handles all the callbacks, most notably interrupts.
    /// </summary>
    public CallbackHandler CallbackHandler { get; }

    /// <summary>
    /// The emulated CPU.
    /// </summary>
    public Cpu Cpu { get; }

    /// <summary>
    /// DOS Services.
    /// </summary>
    public Dos Dos { get; }

    /// <summary>
    /// The Gravis Ultrasound sound card.
    /// </summary>
    public GravisUltraSound GravisUltraSound { get; }

    /// <summary>
    /// The GUI. Can be null in headless mode.
    /// </summary>
    public IGui? Gui { get; }

    /// <summary>
    /// Gives the port read or write to the registered handler.
    /// </summary>
    public IOPortDispatcher IoPortDispatcher { get; }

    /// <summary>
    /// A gameport joystick
    /// </summary>
    public Joystick Joystick { get; }

    /// <summary>
    /// An IBM PC Keyboard
    /// </summary>
    public Keyboard Keyboard { get; }

    /// <summary>
    /// INT16H handler.
    /// </summary>
    public KeyboardInt16Handler KeyboardInt16Handler { get; }

    /// <summary>
    /// Contains all the breakpoints
    /// </summary>
    public MachineBreakpoints MachineBreakpoints { get; }

    /// <summary>
    /// The memory bus.
    /// </summary>
    public Memory Memory { get; }

    /// <summary>
    /// The General MIDI (MPU-401) or MT-32 device.
    /// </summary>
    public Midi MidiDevice { get; }

    /// <summary>
    /// INT33H handler.
    /// </summary>
    public MouseInt33Handler MouseInt33Handler { get; }

    /// <summary>
    /// PC Speaker device.
    /// </summary>
    public PcSpeaker PcSpeaker { get; }

    /// <summary>
    /// The dual programmable interrupt controllers.
    /// </summary>
    public DualPic DualPic { get; }

    /// <summary>
    /// The Sound Blaster card.
    /// </summary>
    public SoundBlaster SoundBlaster { get; }

    /// <summary>
    /// INT15H handler.
    /// </summary>
    public SystemBiosInt15Handler SystemBiosInt15Handler { get; }

    /// <summary>
    /// INT1A handler.
    /// </summary>
    public SystemClockInt1AHandler SystemClockInt1AHandler { get; }

    /// <summary>
    /// The Programmable Interrupt Timer
    /// </summary>
    public Timer Timer { get; }

    /// <summary>
    /// INT8H handler.
    /// </summary>
    public TimerInt8Handler TimerInt8Handler { get; }

    /// <summary>
    /// The VGA Card.
    /// </summary>
    public IVideoCard VgaCard { get; }
    
    /// <summary>
    /// The Vga Registers
    /// </summary>
    public VideoState VgaRegisters { get; set; }
    
    /// <summary>
    /// The VGA port handler
    /// </summary>
    public IIOPortHandler VgaIoPortHandler { get; }

    /// <summary>
    /// The class that handles converting video memory to a bitmap 
    /// </summary>
    public readonly IVgaRenderer VgaRenderer;
    
    /// <summary>
    /// The Video BIOS interrupt handler.
    /// </summary>
    public IVideoInt10Handler VideoInt10Handler { get; }
    
    /// <summary>
    /// The Video Rom containing fonts and other data.
    /// </summary>
    public VgaRom VgaRom { get; }

    /// <summary>
    /// The EMS device driver.
    /// </summary>
    public ExpandedMemoryManager? Ems { get; set; }

    /// <summary>
    /// The DMA controller.
    /// </summary>
    public DmaController DmaController { get; }

    public ExtendedMemoryManager? Xms { get; }

    /// <summary>
    /// Gets the current DOS environment variables.
    /// </summary>
    public EnvironmentVariables EnvironmentVariables { get; } = new EnvironmentVariables();

    /// <summary>
    /// The OPL3 FM Synth chip.
    /// </summary>
    public OPL3FM OPL3FM { get; }
    
    public IVgaFunctionality VgaFunctions { get; }
    
    public SortedList<uint, IDeviceCallbackProvider> DeviceCallbackProviders { get; } = new();

    /// <summary>
    /// The code invoked when emulation pauses.
    /// </summary>

    public event Action? Paused;

    /// <summary>
    /// The code invoked when emulation resumes.
    /// </summary>
    public event Action? Resumed;

    /// <summary>
    /// The emulator configuration.
    /// </summary>
    public Configuration Configuration { get; }
    
    /// <summary>
    /// Initializes a new instance
    /// <param name="machineCreationOptions">Describes how the machine will run, and what it will run.</param>
    /// </summary>
    public Machine(MachineCreationOptions machineCreationOptions) {
        _programExecutor = machineCreationOptions.ProgramExecutor;
        Configuration = machineCreationOptions.Configuration;
        Gui = machineCreationOptions.Gui;
        RecordData = machineCreationOptions.RecordData;

        IMemoryDevice ram = new Ram(Memory.EndOfHighMemoryArea);
        Memory = new Memory(ram, machineCreationOptions.Configuration);
        BiosDataArea = new BiosDataArea(Memory);
        Cpu = new Cpu(this, machineCreationOptions.LoggerService, machineCreationOptions.ExecutionFlowRecorder, machineCreationOptions.RecordData);
        if(machineCreationOptions.Configuration.Xms && machineCreationOptions.Configuration.Ems) {
            throw new UnrecoverableException("Cannot have XMS and EMS at the same time");
        }
        if(machineCreationOptions.Configuration.Xms) {
            Xms = new(this);
        }
        if(machineCreationOptions.Configuration.Ems) {
            Xms ??= new(this);
            Ems = new(this);
        }
        
        // Breakpoints
        MachineBreakpoints = new MachineBreakpoints(this, machineCreationOptions.LoggerService);

        // IO devices
        IoPortDispatcher = new IOPortDispatcher(
            this,
            machineCreationOptions.LoggerService,
            machineCreationOptions.Configuration);
        Cpu.IoPortDispatcher = IoPortDispatcher;

        DmaController = new DmaController(this, machineCreationOptions.Configuration, machineCreationOptions.LoggerService);
        Register(DmaController);

        DualPic = new DualPic(this, machineCreationOptions.Configuration, machineCreationOptions.LoggerService);
        Register(DualPic);

        VgaRegisters = new VideoState();
        VgaIoPortHandler = new VgaIoPortHandler(this, machineCreationOptions.LoggerService, machineCreationOptions.Configuration, VgaRegisters);
        Register(VgaIoPortHandler);

        const uint videoBaseAddress = MemoryMap.GraphicVideoMemorySegment << 4;
        IVideoMemory vgaMemory = new VideoMemory(VgaRegisters);
        Memory.RegisterMapping(videoBaseAddress, vgaMemory.Size, vgaMemory);
        VgaRenderer = new Renderer(VgaRegisters, vgaMemory);
        VgaCard = new VgaCard(machineCreationOptions.Gui, VgaRenderer, machineCreationOptions.LoggerService);
        
        Timer = new Timer(this, machineCreationOptions.LoggerService, DualPic, VgaCard, machineCreationOptions.CounterConfigurator, machineCreationOptions.Configuration);
        Register(Timer);
        Keyboard = new Keyboard(this, machineCreationOptions.LoggerService, machineCreationOptions.Gui, machineCreationOptions.Configuration);
        Register(Keyboard);
        Joystick = new Joystick(this, machineCreationOptions.Configuration, machineCreationOptions.LoggerService);
        Register(Joystick);
        PcSpeaker = new PcSpeaker(this, machineCreationOptions.LoggerService, machineCreationOptions.Configuration);
        Register(PcSpeaker);
        OPL3FM = new OPL3FM(this, machineCreationOptions.Configuration, machineCreationOptions.LoggerService);
        Register(OPL3FM);
        SoundBlaster = new SoundBlaster(this, machineCreationOptions.Configuration, machineCreationOptions.LoggerService);
        Register(SoundBlaster);
        SoundBlaster.AddEnvironmentVariable();
        GravisUltraSound = new GravisUltraSound(this, machineCreationOptions.Configuration, machineCreationOptions.LoggerService);
        Register(GravisUltraSound);
        MidiDevice = new Midi(this, machineCreationOptions.Configuration, machineCreationOptions.LoggerService);
        Register(MidiDevice);

        // Services
        CallbackHandler = new CallbackHandler(this, machineCreationOptions.LoggerService, MemoryMap.InterruptHandlersSegment);
        Cpu.CallbackHandler = CallbackHandler;
        
        VgaRom = new VgaRom();
        Memory.RegisterMapping(MemoryMap.VideoBiosSegment << 4, VgaRom.Size, VgaRom);
        VgaFunctions = new VgaFunctionality(Memory, IoPortDispatcher, BiosDataArea, VgaRom);
        VideoInt10Handler = new VgaBios(this, VgaFunctions, BiosDataArea, machineCreationOptions.LoggerService);
        Register(VideoInt10Handler);
        
        TimerInt8Handler = new TimerInt8Handler(this, machineCreationOptions.LoggerService);
        Register(TimerInt8Handler);
        BiosKeyboardInt9Handler = new BiosKeyboardInt9Handler(this, machineCreationOptions.LoggerService);
        Register(BiosKeyboardInt9Handler);
        
        BiosEquipmentDeterminationInt11Handler = new BiosEquipmentDeterminationInt11Handler(this, machineCreationOptions.LoggerService);
        Register(BiosEquipmentDeterminationInt11Handler);
        SystemBiosInt15Handler = new SystemBiosInt15Handler(this, machineCreationOptions.LoggerService);
        Register(SystemBiosInt15Handler);
        KeyboardInt16Handler = new KeyboardInt16Handler(
            this,
            machineCreationOptions.LoggerService,
            BiosKeyboardInt9Handler.BiosKeyboardBuffer);
        Register(KeyboardInt16Handler);
        SystemClockInt1AHandler = new SystemClockInt1AHandler(
            this,
            machineCreationOptions.LoggerService,
            TimerInt8Handler);
        Register(SystemClockInt1AHandler);

        // Initialize DOS.
        Dos = new Dos(this, machineCreationOptions.LoggerService);
        Dos.Initialize();
        
        MouseInt33Handler = new MouseInt33Handler(this, machineCreationOptions.LoggerService, machineCreationOptions.Gui);
        Register(MouseInt33Handler);
        
        _dmaThread = new Thread(DmaLoop) {
            Name = "DMAThread"
        };
        if(Xms is not null) {
            Register((IDeviceCallbackProvider)Xms);
            Register((ICallback)Xms);
        }
        if(machineCreationOptions.Configuration.Ems) {
            Ems = new(this, machineCreationOptions.LoggerService);
            Register((ICallback)Ems);
        }
    }
    public void Register(IDeviceCallbackProvider callbackProvider) {
        int id = DeviceCallbackProviders.Count;
        callbackProvider.CallbackAddress = this.Memory.AddCallbackHandler((byte)id, callbackProvider.IsHookable);
        DeviceCallbackProviders.Add((uint)id, callbackProvider);

        Span<byte> machineCode = stackalloc byte[3];
        machineCode[0] = 0x0F;
        machineCode[1] = 0x56;
        machineCode[2] = (byte)id;
        callbackProvider.SetRaiseCallbackInstruction(machineCode);
    }
    
    /// <summary>
    /// Registers a callback, such as an interrupt handler.
    /// </summary>
    /// <param name="callback">The callback implementation.</param>
    public void Register(ICallback callback) {
        CallbackHandler.AddCallback(callback);
    }

    /// <summary>
    /// Registers a I/O port handler, such as a sound card.
    /// </summary>
    /// <param name="ioPortHandler">The I/O port handler.</param>
    /// <exception cref="ArgumentException"></exception>
    public void Register(IIOPortHandler ioPortHandler) {
        ioPortHandler.InitPortHandlers(IoPortDispatcher);

        if (ioPortHandler is not IDmaDevice8 dmaDevice) {
            return;
        }

        if (dmaDevice.Channel < 0 || dmaDevice.Channel >= DmaController.Channels.Count) {
            throw new ArgumentException("Invalid DMA channel on DMA device.");
        }

        DmaController.Channels[dmaDevice.Channel].Device = dmaDevice;
        _dmaDeviceChannels.Add(DmaController.Channels[dmaDevice.Channel]);
    }

    /// <summary>
    /// https://techgenix.com/direct-memory-access/
    /// </summary>
    private void DmaLoop() {
        while (Cpu.IsRunning && !_exitDmaLoop && !_exitEmulationLoop && !_disposed) {
            for (int i = 0; i < _dmaDeviceChannels.Count; i++) {
                DmaChannel dmaChannel = _dmaDeviceChannels[i];
                if (Gui?.IsPaused == true || IsPaused) {
                    Gui?.WaitForContinue();
                }
                dmaChannel.Transfer(Memory);
                if (!_exitDmaLoop) {
                    _dmaResetEvent.WaitOne(1);
                }
            }
        }
    }

    /// <summary>
    /// Returns a string that dumps the call stack.
    /// </summary>
    /// <returns>A string laying out the call stack.</returns>
    public string DumpCallStack() {
        FunctionHandler inUse = Cpu.FunctionHandlerInUse;
        StringBuilder sb = new();
        if (inUse.Equals(Cpu.FunctionHandlerInExternalInterrupt)) {
            sb.AppendLine("From external interrupt:");
        }

        sb.Append(inUse.DumpCallStack());
        return sb.ToString();
    }

    /// <summary>
    /// Installs all the callback in the dispatch table in emulated memory.
    /// </summary>
    public void InstallAllCallbacksInInterruptTable() {
        CallbackHandler.InstallAllCallbacksInInterruptTable();
    }

    /// <summary>
    /// Peeks at the return address.
    /// </summary>
    /// <returns>The return address string.</returns>
    public string PeekReturn() {
        return ToString(Cpu.FunctionHandlerInUse.PeekReturnAddressOnMachineStackForCurrentFunction());
    }

    /// <summary>
    /// Peeks at the return address.
    /// </summary>
    /// <param name="returnCallType">The expected call type.</param>
    /// <returns>The return address string.</returns>
    public string PeekReturn(CallType returnCallType) {
        return ToString(Cpu.FunctionHandlerInUse.PeekReturnAddressOnMachineStack(returnCallType));
    }

    /// <summary>
    /// Implements the emulation loop.
    /// </summary>
    /// <exception cref="InvalidVMOperationException">When an unhandled exception occurs. This can occur if the target program is not supported (yet).</exception>
    public void Run() {
        State state = Cpu.State;
        FunctionHandler functionHandler = Cpu.FunctionHandler;
        if (!_dmaThreadStarted) {
            _dmaThread.Start();
            _dmaThreadStarted = true;
        }
        if (Debugger.IsAttached) {
            try {
                StartRunLoop(functionHandler, state);
            } catch (HaltRequestedException) {
                // Actually a signal generated code requested Exit
                Dispose(disposing: true);
            }
        } else {
            try {
                StartRunLoop(functionHandler, state);
            } catch (InvalidVMOperationException e) {
                e.Demystify();
                throw;
            } catch (HaltRequestedException) {
                // Actually a signal generated code requested Exit
                Dispose(disposing: true);
            } catch (Exception e) {
                e.Demystify();
                throw new InvalidVMOperationException(this, e);
            }
        }
        MachineBreakpoints.OnMachineStop();
        functionHandler.Ret(CallType.MACHINE);
    }

    private void StartRunLoop(FunctionHandler functionHandler, State state)
    {
        // Entry could be overridden and could throw exceptions
        functionHandler.Call(CallType.MACHINE, state.CS, state.IP, null, null, "entry", false);
        RunLoop();
    }

    /// <summary>
    /// Whether the emulation is paused.
    /// </summary>
    public bool IsPaused { get; private set; }

    private bool _exitEmulationLoop;

    /// <summary>
    /// Forces the emulation loop to exit.
    /// </summary>
    public void ExitEmulationLoop() => _exitEmulationLoop = true;

    private void RunLoop() {
        _exitEmulationLoop = false;
        while (Cpu.IsRunning && !_exitEmulationLoop && !_disposed) {
            PauseIfAskedTo();
            if (RecordData) {
                MachineBreakpoints.CheckBreakPoint();
            }
            Cpu.ExecuteNextInstruction();
            Timer.Tick();
        }
    }

    /// <summary>
    /// Performs DMA transfers when invoked.
    /// </summary>
    public void PerformDmaTransfers() {
        if (!_disposed && !_exitDmaLoop) {
            _dmaResetEvent.Set();
        }
    }

    private void PauseIfAskedTo() {
        if(Gui?.PauseEmulatorOnStart == true) {
            Gui?.PauseEmulationOnStart();
            Gui?.WaitForContinue();
        }
        if (Gui?.IsPaused == true) {
            IsPaused = true;
            Paused?.Invoke();
            if (!_programExecutor.Step()) {
                Gui.IsPaused = true;
                Gui?.WaitForContinue();
            }
            Resumed?.Invoke();
            IsPaused = false;
        }
    }

    private static string ToString(SegmentedAddress? segmentedAddress) {
        if (segmentedAddress is not null) {
            return segmentedAddress.ToString();
        }

        return "null";
    }

    /// <summary>
    /// Releases all resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                _dmaResetEvent.Set();
                _exitDmaLoop = true;
                if (_dmaThread.IsAlive && _dmaThreadStarted) {
                    _dmaThread.Join();
                }
                _dmaResetEvent.Dispose();
                MidiDevice.Dispose();
                SoundBlaster.Dispose();
                OPL3FM.Dispose();
                PcSpeaker.Dispose();
                MachineBreakpoints.Dispose();
            }
            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}