namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.RBJ;

using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents the LowPass class.
/// </summary>
public class LowPass<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency) {
        SetupLowPass(cutoffFrequency / sampleRate, OneOverSqrtTwo);
    }

    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double q) {
        SetupLowPass(cutoffFrequency / sampleRate, q);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency) {
        SetupLowPass(cutoffFrequency, OneOverSqrtTwo);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double q) {
        SetupLowPass(cutoffFrequency, q);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class LowPass : LowPass<DirectFormIiState>;

/// <summary>
/// Represents the HighPass class.
/// </summary>
public class HighPass<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency) {
        SetupHighPass(cutoffFrequency / sampleRate, OneOverSqrtTwo);
    }


    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double q) {
        SetupHighPass(cutoffFrequency / sampleRate, q);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency) {
        SetupHighPass(cutoffFrequency, OneOverSqrtTwo);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double q) {
        SetupHighPass(cutoffFrequency, q);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class HighPass : HighPass<DirectFormIiState>;

/// <summary>
/// Represents the BandPass1 class.
/// </summary>
public class BandPass1<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double bandWidth) {
        SetupBandPass1(centerFrequency / sampleRate, bandWidth);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double bandWidth) {
        SetupBandPass1(centerFrequency, bandWidth);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class BandPass1 : BandPass1<DirectFormIiState>;

/// <summary>
/// Represents the BandPass2 class.
/// </summary>
public class BandPass2<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double bandWidth) {
        SetupBandPass2(centerFrequency / sampleRate, bandWidth);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double bandWidth) {
        SetupBandPass2(centerFrequency, bandWidth);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class BandPass2 : BandPass2<DirectFormIiState>;

/// <summary>
/// Represents the BandStop class.
/// </summary>
public class BandStop<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double bandWidth) {
        SetupBandStop(centerFrequency / sampleRate, bandWidth);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double bandWidth) {
        SetupBandStop(centerFrequency, bandWidth);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class BandStop : BandStop<DirectFormIiState>;

/// <summary>
/// Represents the IirNotch class.
/// </summary>
public class IirNotch<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double qFactor = 10.0) {
        SetupNotch(centerFrequency / sampleRate, qFactor);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double qFactor = 10.0) {
        SetupNotch(centerFrequency, qFactor);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class IirNotch : IirNotch<DirectFormIiState>;

/// <summary>
/// Represents the LowShelf class.
/// </summary>
public class LowShelf<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double shelfSlope = 1.0) {
        SetupLowShelf(cutoffFrequency / sampleRate, gainDb, shelfSlope);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double gainDb, double shelfSlope = 1.0) {
        SetupLowShelf(cutoffFrequency, gainDb, shelfSlope);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class LowShelf : LowShelf<DirectFormIiState>;

/// <summary>
/// Represents the HighShelf class.
/// </summary>
public class HighShelf<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double shelfSlope = 1.0) {
        SetupHighShelf(cutoffFrequency / sampleRate, gainDb, shelfSlope);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double cutoffFrequency, double gainDb, double shelfSlope = 1.0) {
        SetupHighShelf(cutoffFrequency, gainDb, shelfSlope);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class HighShelf : HighShelf<DirectFormIiState>;

/// <summary>
/// Represents the BandShelf class.
/// </summary>
public class BandShelf<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double centerFrequency, double gainDb, double bandWidth) {
        SetupBandShelf(centerFrequency / sampleRate, gainDb, bandWidth);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double centerFrequency, double gainDb, double bandWidth) {
        SetupBandShelf(centerFrequency, gainDb, bandWidth);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class BandShelf : BandShelf<DirectFormIiState>;

/// <summary>
/// Represents the AllPass class.
/// </summary>
public class AllPass<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double phaseFrequency) {
        SetupAllPass(phaseFrequency / sampleRate, OneOverSqrtTwo);
    }


    /// <summary>
    /// Setup method.
    /// </summary>
    public void Setup(double sampleRate, double phaseFrequency, double q) {
        SetupAllPass(phaseFrequency / sampleRate, q);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double phaseFrequency) {
        SetupAllPass(phaseFrequency, OneOverSqrtTwo);
    }

    /// <summary>
    /// SetupN method.
    /// </summary>
    public void SetupN(double phaseFrequency, double q) {
        SetupAllPass(phaseFrequency, q);
    }
}

/// <summary>
/// The class.
/// </summary>
public sealed class AllPass : AllPass<DirectFormIiState>;