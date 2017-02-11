namespace FluentHelium.Module
{
    /// <summary>
    /// Module as unit for apllication building
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// All needed information for mudule identification and activation
        /// Immutable during all lifecycle of module
        /// </summary>
        IModuleDescriptor Descriptor { get; }

        /// <summary>
        /// Activation - fail if exist non-implemented dependencies or unhandled exception during initialization or module already initialized
        /// Thread-unsafe
        /// </summary>
        /// <param name="dependencies">Provider of all input depedencies</param>
        /// <returns>Provider of all output dependencies, use Dispose for module finalization (thread-unsafe)</returns>
        Usable<IDependencyProvider> Activate(IDependencyProvider dependencies);
    }
}