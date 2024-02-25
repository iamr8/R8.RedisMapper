using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace R8.RedisMapper
{
    public sealed class PipelineContextPropertyCollection<T> : IEnumerable<string>
    {
        private readonly List<string> _properties = new List<string>();

        internal PipelineContextPropertyCollection()
        {
        }

        public void Add(Expression<Func<T, object>> property)
        {
            switch (property.Body)
            {
                case MemberExpression memberExpression:
                    _properties.Add(memberExpression.Member.Name);
                    break;
                case UnaryExpression { Operand: MemberExpression memberExpression2 }:
                    _properties.Add(memberExpression2.Member.Name);
                    break;
            }
        }

        public void Add(string property)
        {
            _properties.Add(property);
        }

        public int Count => _properties.Count;

        public string this[int index] => _properties[index];

        public IEnumerator<string> GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}