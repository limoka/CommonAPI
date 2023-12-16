#nullable enable
using System;

namespace CommonAPI
{
#pragma warning disable 649
    /// <summary>
    /// Attribute to have at the top of your BaseUnityPlugin class if you want to load a specific R2API Submodule.
    /// Parameter(s) are the nameof the submodules.
    /// e.g: [CommonAPISubmoduleDependency("", "")]
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class CommonAPISubmoduleDependency : Attribute {
        public string?[]? SubmoduleNames { get; }

        public CommonAPISubmoduleDependency(params string[] submoduleName) {
            SubmoduleNames = submoduleName;
        }
    }
}