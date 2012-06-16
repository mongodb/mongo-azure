/*
 * Copyright 2010-2012 10gen Inc.
 * file : AvailableNodes.cs
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

namespace MongoDB.WindowsAzure.InstanceMaintainer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class AvailableNodes : IEnumerable<NodeAlias>
    {
        private List<NodeAlias> nodes;

        internal AvailableNodes()
        {
            nodes = new List<NodeAlias>();
        }

        internal void Add(NodeAlias node)
        {
            nodes.Add(node);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<NodeAlias> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            foreach (var node in nodes)
            {
                hash = 37 * hash + node.GetHashCode();
            }
            return hash;
        }

        public bool Equals(AvailableNodes rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType())
            {
                return false;
            }
            return object.ReferenceEquals(this, rhs) || nodes.SequenceEqual(rhs.nodes);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AvailableNodes); // works even if obj is null
        }

    }

}
