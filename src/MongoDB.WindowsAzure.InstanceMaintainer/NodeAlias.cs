/*
 * Copyright 2010-2012 10gen Inc.
 * file : NodeAlias.cs
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace WinHostsUpdater
{

    using System;

    internal class NodeAlias
    {
        internal string Alias { get; set; }
        internal string IpAddress { get; set; }

        public override bool Equals(object rhs)
        {
            if (rhs is NodeAlias)
            {
                return this.Equals((NodeAlias)rhs);
            }
            return false;
        }

        public bool Equals(NodeAlias rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType())
            {
                return false;
            }

            return Alias.Equals(rhs.Alias, StringComparison.OrdinalIgnoreCase) &&
                IpAddress.Equals(rhs.IpAddress, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + Alias.ToLower().GetHashCode();
            hash = 37 * hash + IpAddress.ToLower().GetHashCode();
            return hash;
        }

    }
}
