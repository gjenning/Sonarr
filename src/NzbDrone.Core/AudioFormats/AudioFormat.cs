using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.AudioFormats
{
    public class AudioFormat : ModelBase, IEquatable<AudioFormat>
    {
        public AudioFormat()
        {
        }

        public AudioFormat(string name, params IAudioFormatSpecification[] specs)
        {
            Name = name;
            Specifications = specs.ToList();
        }

        public string Name { get; set; }

        public List<IAudioFormatSpecification> Specifications { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(AudioFormat other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return int.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((AudioFormat)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
