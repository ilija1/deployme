using System;

namespace DeployMe.Api.Models
{
    public sealed class DeploymentPackage : IEquatable<DeploymentPackage>
    {
        public string Name { get; set; }
        public string Version { get; set; }

        public bool Equals(DeploymentPackage other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(Version, other.Version, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is DeploymentPackage other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Name) : 0) * 397) ^
                       (Version != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Version) : 0);
            }
        }

        public static bool operator ==(DeploymentPackage left, DeploymentPackage right) => Equals(left, right);

        public static bool operator !=(DeploymentPackage left, DeploymentPackage right) => !Equals(left, right);
    }
}
