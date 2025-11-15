namespace Spice86.Libs.Sound.Filters.IirFilters.Filters.RBJ;

using Spice86.Libs.Sound.Filters.IirFilters.Common.State;

/// <summary>
/// Represents low pass.
/// </summary>
public class LowPass<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void Setup(double sampleRate, double cutoffFrequency) {
        SetupLowPass(cutoffFrequency / sampleRate, OneOverSqrtTwo);
    }

    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="q">The q.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double q) {
        SetupLowPass(cutoffFrequency / sampleRate, q);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void SetupN(double cutoffFrequency) {
        SetupLowPass(cutoffFrequency, OneOverSqrtTwo);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="q">The q.</param>
    public void SetupN(double cutoffFrequency, double q) {
        SetupLowPass(cutoffFrequency, q);
    }
}

/// <summary>
/// Represents low pass.
/// </summary>
public sealed class LowPass : LowPass<DirectFormIiState>;

/// <summary>
/// Represents high pass.
/// </summary>
public class HighPass<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void Setup(double sampleRate, double cutoffFrequency) {
        SetupHighPass(cutoffFrequency / sampleRate, OneOverSqrtTwo);
    }


    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="q">The q.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double q) {
        SetupHighPass(cutoffFrequency / sampleRate, q);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    public void SetupN(double cutoffFrequency) {
        SetupHighPass(cutoffFrequency, OneOverSqrtTwo);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="q">The q.</param>
    public void SetupN(double cutoffFrequency, double q) {
        SetupHighPass(cutoffFrequency, q);
    }
}

/// <summary>
/// Represents high pass.
/// </summary>
public sealed class HighPass : HighPass<DirectFormIiState>;

/// <summary>
/// Represents band pass 1.
/// </summary>
public class BandPass1<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="bandWidth">The band width.</param>
    public void Setup(double sampleRate, double centerFrequency, double bandWidth) {
        SetupBandPass1(centerFrequency / sampleRate, bandWidth);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="bandWidth">The band width.</param>
    public void SetupN(double centerFrequency, double bandWidth) {
        SetupBandPass1(centerFrequency, bandWidth);
    }
}

/// <summary>
/// Represents band pass 1.
/// </summary>
public sealed class BandPass1 : BandPass1<DirectFormIiState>;

/// <summary>
/// Represents band pass 2.
/// </summary>
public class BandPass2<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="bandWidth">The band width.</param>
    public void Setup(double sampleRate, double centerFrequency, double bandWidth) {
        SetupBandPass2(centerFrequency / sampleRate, bandWidth);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="bandWidth">The band width.</param>
    public void SetupN(double centerFrequency, double bandWidth) {
        SetupBandPass2(centerFrequency, bandWidth);
    }
}

/// <summary>
/// Represents band pass 2.
/// </summary>
public sealed class BandPass2 : BandPass2<DirectFormIiState>;

/// <summary>
/// Represents band stop.
/// </summary>
public class BandStop<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="bandWidth">The band width.</param>
    public void Setup(double sampleRate, double centerFrequency, double bandWidth) {
        SetupBandStop(centerFrequency / sampleRate, bandWidth);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="bandWidth">The band width.</param>
    public void SetupN(double centerFrequency, double bandWidth) {
        SetupBandStop(centerFrequency, bandWidth);
    }
}

/// <summary>
/// Represents band stop.
/// </summary>
public sealed class BandStop : BandStop<DirectFormIiState>;

/// <summary>
/// Represents iir notch.
/// </summary>
public class IirNotch<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    public void Setup(double sampleRate, double centerFrequency, double qFactor = 10.0) {
        SetupNotch(centerFrequency / sampleRate, qFactor);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    public void SetupN(double centerFrequency, double qFactor = 10.0) {
        SetupNotch(centerFrequency, qFactor);
    }
}

/// <summary>
/// Represents iir notch.
/// </summary>
public sealed class IirNotch : IirNotch<DirectFormIiState>;

/// <summary>
/// Represents low shelf.
/// </summary>
public class LowShelf<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double shelfSlope = 1.0) {
        SetupLowShelf(cutoffFrequency / sampleRate, gainDb, shelfSlope);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void SetupN(double cutoffFrequency, double gainDb, double shelfSlope = 1.0) {
        SetupLowShelf(cutoffFrequency, gainDb, shelfSlope);
    }
}

/// <summary>
/// Represents low shelf.
/// </summary>
public sealed class LowShelf : LowShelf<DirectFormIiState>;

/// <summary>
/// Represents high shelf.
/// </summary>
public class HighShelf<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void Setup(double sampleRate, double cutoffFrequency, double gainDb, double shelfSlope = 1.0) {
        SetupHighShelf(cutoffFrequency / sampleRate, gainDb, shelfSlope);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="cutoffFrequency">The cutoff frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    public void SetupN(double cutoffFrequency, double gainDb, double shelfSlope = 1.0) {
        SetupHighShelf(cutoffFrequency, gainDb, shelfSlope);
    }
}

/// <summary>
/// Represents high shelf.
/// </summary>
public sealed class HighShelf : HighShelf<DirectFormIiState>;

/// <summary>
/// Represents band shelf.
/// </summary>
public class BandShelf<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="bandWidth">The band width.</param>
    public void Setup(double sampleRate, double centerFrequency, double gainDb, double bandWidth) {
        SetupBandShelf(centerFrequency / sampleRate, gainDb, bandWidth);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="centerFrequency">The center frequency.</param>
    /// <param name="gainDb">The gain db.</param>
    /// <param name="bandWidth">The band width.</param>
    public void SetupN(double centerFrequency, double gainDb, double bandWidth) {
        SetupBandShelf(centerFrequency, gainDb, bandWidth);
    }
}

/// <summary>
/// Represents band shelf.
/// </summary>
public sealed class BandShelf : BandShelf<DirectFormIiState>;

/// <summary>
/// Represents all pass.
/// </summary>
public class AllPass<TState> : RbjFilterBase<TState>
    where TState : struct, ISectionState {
    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="phaseFrequency">The phase frequency.</param>
    public void Setup(double sampleRate, double phaseFrequency) {
        SetupAllPass(phaseFrequency / sampleRate, OneOverSqrtTwo);
    }


    /// <summary>
    /// Sets up.
    /// </summary>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="phaseFrequency">The phase frequency.</param>
    /// <param name="q">The q.</param>
    public void Setup(double sampleRate, double phaseFrequency, double q) {
        SetupAllPass(phaseFrequency / sampleRate, q);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="phaseFrequency">The phase frequency.</param>
    public void SetupN(double phaseFrequency) {
        SetupAllPass(phaseFrequency, OneOverSqrtTwo);
    }

    /// <summary>
    /// Sets upn.
    /// </summary>
    /// <param name="phaseFrequency">The phase frequency.</param>
    /// <param name="q">The q.</param>
    public void SetupN(double phaseFrequency, double q) {
        SetupAllPass(phaseFrequency, q);
    }
}

/// <summary>
/// Represents all pass.
/// </summary>
public sealed class AllPass : AllPass<DirectFormIiState>;